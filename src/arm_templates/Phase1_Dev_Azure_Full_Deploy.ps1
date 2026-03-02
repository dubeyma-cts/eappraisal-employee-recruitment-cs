# Phase 1 Dev - Full Deployment Orchestrator (PowerShell + Azure CLI)
# Flow: Provision infra -> Apply app settings -> Deploy service artifacts/JARs
#
# Examples:
# 1) Full run (uses values in ARM parameters file)
#    .\Phase1_Dev_Azure_Full_Deploy.ps1
#
# 2) Full run with SQL password override at runtime (recommended)
#    .\Phase1_Dev_Azure_Full_Deploy.ps1 -SqlAdminPassword "<secure-password>"
#
# 3) Skip infra and only apply settings + deploy jars (sequential mode)
#    .\Phase1_Dev_Azure_Full_Deploy.ps1 -SkipInfra -SkipSettings -SkipBuild -SequentialDeploy
#
# 3) Skip Maven build and deploy existing jars
#    .\Phase1_Dev_Azure_Full_Deploy.ps1 -SkipBuild

param(
  [string]$ResourceGroup = 'rg-eappraisal-dev-win',
  [string]$TemplateFile = '.\\arm\\eappraisal-phase1-single-spoke-vnet.json',
  [string]$ParametersFile = '.\\arm\\eappraisal-phase1-single-spoke-vnet.parameters.json',
  [string]$SqlAdminPassword = '',
  [switch]$SkipInfra,
  [switch]$SkipSettings,
  [switch]$SkipBuild,
  [switch]$SkipJarDeploy,
  [switch]$SkipTests,
  [switch]$UseScmHostsOverride,
  [switch]$SequentialDeploy
)

$ErrorActionPreference = 'Stop'

$global:RetryCount = 5
$global:RetryDelaySeconds = 15

function Assert-Command([string]$name) {
  if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
    throw "Required command not found: $name"
  }
}

function Invoke-Az {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Arguments,
    [switch]$SuppressOutput
  )

  if ($SuppressOutput) {
    & az @Arguments --output none
  }
  else {
    & az @Arguments
  }

  if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI command failed: az $($Arguments -join ' ')"
  }
}

function Invoke-WithRetry {
  param(
    [Parameter(Mandatory = $true)]
    [scriptblock]$Action,
    [Parameter(Mandatory = $true)]
    [string]$Description,
    [int]$MaxAttempts = 5,
    [int]$DelaySeconds = 15
  )

  $attempt = 1
  while ($attempt -le $MaxAttempts) {
    try {
      & $Action
      return
    }
    catch {
      if ($attempt -ge $MaxAttempts) {
        throw "Failed after $MaxAttempts attempts: $Description. Last error: $($_.Exception.Message)"
      }
      Write-Warning "Attempt $attempt failed for: $Description. Retrying in $DelaySeconds sec..."
      Start-Sleep -Seconds $DelaySeconds
      $attempt++
    }
  }
}

function Assert-DnsResolvable {
  param(
    [Parameter(Mandatory = $true)]
    [string]$HostName
  )

  try {
    $null = Resolve-DnsName -Name $HostName -ErrorAction Stop
  }
  catch {
    throw "DNS resolution failed for $HostName. Check network/DNS before deployment."
  }
}

function Test-DnsResolvable {
  param(
    [Parameter(Mandatory = $true)]
    [string]$HostName
  )

  try {
    $null = Resolve-DnsName -Name $HostName -ErrorAction Stop
    return $true
  }
  catch {
    return $false
  }
}

function Resolve-HostIpWithFallback {
  param(
    [Parameter(Mandatory = $true)]
    [string]$HostName
  )

  $dnsServers = @($null, '8.8.8.8', '1.1.1.1')
  foreach ($server in $dnsServers) {
    try {
      if ([string]::IsNullOrWhiteSpace($server)) {
        $records = Resolve-DnsName -Name $HostName -ErrorAction Stop
      }
      else {
        $records = Resolve-DnsName -Name $HostName -Server $server -ErrorAction Stop
      }

      $aRecord = $records | Where-Object { $_.Type -eq 'A' } | Select-Object -First 1
      if ($null -ne $aRecord -and -not [string]::IsNullOrWhiteSpace($aRecord.IPAddress)) {
        return $aRecord.IPAddress
      }
    }
    catch {
      continue
    }
  }

  return $null
}

function Ensure-AdminForHostsUpdate {
  $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
  $principal = New-Object Security.Principal.WindowsPrincipal($identity)
  return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Update-ScmHostsEntries {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Apps
  )

  if (-not (Ensure-AdminForHostsUpdate)) {
    Write-Warning "-UseScmHostsOverride requested, but shell is not elevated. Skipping hosts override."
    return
  }

  $hostsPath = Join-Path $env:SystemRoot 'System32\drivers\etc\hosts'
  if (-not (Test-Path $hostsPath)) {
    throw "Hosts file not found: $hostsPath"
  }

  $tag = '# eappraisal-scm'
  $existing = Get-Content -Path $hostsPath -ErrorAction Stop
  $filtered = @($existing | Where-Object { $_ -notmatch [regex]::Escape($tag) })

  $newLines = @()
  foreach ($app in $Apps) {
    $scmHost = "$app.scm.azurewebsites.net"
    $ip = Resolve-HostIpWithFallback -HostName $scmHost
    if ([string]::IsNullOrWhiteSpace($ip)) {
      throw "Unable to resolve SCM host via fallback DNS: $scmHost"
    }
    $newLines += "$ip`t$scmHost`t$tag"
  }

  $allLines = @($filtered + $newLines)
  Set-Content -Path $hostsPath -Value $allLines -Encoding ascii -ErrorAction Stop

  ipconfig /flushdns | Out-Null
  Write-Host "SCM hosts entries updated and DNS cache flushed." -ForegroundColor Green
}

function Get-JsonParamValue($obj, [string]$name) {
  if ($null -eq $obj.parameters.$name) {
    return $null
  }
  return $obj.parameters.$name.value
}

Assert-Command 'az'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptRoot

if (-not (Test-Path $TemplateFile)) {
  throw "Template file not found: $TemplateFile"
}
if (-not (Test-Path $ParametersFile)) {
  throw "Parameters file not found: $ParametersFile"
}

# Validate Azure context
Invoke-Az -Arguments @('account', 'show') -SuppressOutput
Assert-DnsResolvable -HostName 'management.azure.com'

$paramsJson = Get-Content -Raw -Path $ParametersFile | ConvertFrom-Json
$location = Get-JsonParamValue $paramsJson 'location'
if ([string]::IsNullOrWhiteSpace($location)) { $location = 'centralus' }

$appNames = Get-JsonParamValue $paramsJson 'webAppNames'
if ($null -eq $appNames -or $appNames.Count -eq 0) {
  throw "webAppNames is missing or empty in parameters file."
}

$sqlServerName = Get-JsonParamValue $paramsJson 'sqlServerName'
$sqlDatabaseName = Get-JsonParamValue $paramsJson 'sqlDatabaseName'
$sqlAdminLogin = Get-JsonParamValue $paramsJson 'sqlAdminLogin'
$storageContainerExports = Get-JsonParamValue $paramsJson 'storageContainerExports'
if ([string]::IsNullOrWhiteSpace($storageContainerExports)) { $storageContainerExports = 'exports' }

# Ensure RG exists
$exists = az group exists --name $ResourceGroup
if ($exists -eq 'false') {
  Write-Host "Creating resource group: $ResourceGroup ($location)" -ForegroundColor Cyan
  Invoke-Az -Arguments @('group', 'create', '--name', $ResourceGroup, '--location', $location) -SuppressOutput
}

if (-not $SkipInfra) {
  Write-Host "[1/4] Provisioning infrastructure via ARM template..." -ForegroundColor Cyan

  if ([string]::IsNullOrWhiteSpace($SqlAdminPassword)) {
    az deployment group create `
      --resource-group $ResourceGroup `
      --template-file $TemplateFile `
      --parameters "@$ParametersFile" `
      --output none
  }
  else {
    az deployment group create `
      --resource-group $ResourceGroup `
      --template-file $TemplateFile `
      --parameters "@$ParametersFile" `
      --parameters sqlAdminPassword="$SqlAdminPassword" `
      --output none
  }

  Write-Host "Infrastructure provisioning completed." -ForegroundColor Green
}
else {
  Write-Host "[1/4] Skipped infrastructure provisioning." -ForegroundColor Yellow
}

# Fetch storage account name from current RG (created by ARM template)
$storageAccountName = az storage account list --resource-group $ResourceGroup --query "[0].name" --output tsv
if ([string]::IsNullOrWhiteSpace($storageAccountName)) {
  throw "No storage account found in resource group $ResourceGroup."
}

if (-not $SkipSettings) {
  Write-Host "[2/4] Applying app settings to all services..." -ForegroundColor Cyan

  $dbUrl = "jdbc:sqlserver://$sqlServerName.database.windows.net:1433;database=$sqlDatabaseName;encrypt=true;trustServerCertificate=false;hostNameInCertificate=*.database.windows.net;loginTimeout=30;"

  if ([string]::IsNullOrWhiteSpace($SqlAdminPassword)) {
    if ([string]::IsNullOrWhiteSpace((Get-JsonParamValue $paramsJson 'sqlAdminPassword'))) {
      throw "SQL admin password is empty. Provide -SqlAdminPassword or set sqlAdminPassword in parameters file."
    }
    $dbPassword = Get-JsonParamValue $paramsJson 'sqlAdminPassword'
  }
  else {
    $dbPassword = $SqlAdminPassword
  }

  $commonSettings = @(
    "SPRING_PROFILES_ACTIVE=dev",
    "DB_URL=$dbUrl",
    "DB_DRIVER_CLASS_NAME=com.microsoft.sqlserver.jdbc.SQLServerDriver",
    "HIBERNATE_DIALECT=org.hibernate.dialect.SQLServerDialect",
    "DB_USERNAME=$sqlAdminLogin",
    "DB_PASSWORD=$dbPassword",
    "STORAGE_ACCOUNT_NAME=$storageAccountName",
    "STORAGE_CONTAINER_EXPORTS=$storageContainerExports"
  )

  foreach ($app in $appNames) {
    Write-Host " -> $app" -ForegroundColor Yellow
    Invoke-WithRetry -Description "Set app settings for $app" -MaxAttempts $global:RetryCount -DelaySeconds $global:RetryDelaySeconds -Action {
      Invoke-Az -Arguments @('webapp', 'config', 'appsettings', 'set', '--resource-group', $ResourceGroup, '--name', $app, '--settings', $commonSettings) -SuppressOutput
    }

    Invoke-WithRetry -Description "Enable HTTPS only for $app" -MaxAttempts $global:RetryCount -DelaySeconds $global:RetryDelaySeconds -Action {
      Invoke-Az -Arguments @('webapp', 'update', '--resource-group', $ResourceGroup, '--name', $app, '--https-only', 'true') -SuppressOutput
    }
  }

  # Appraisal service needs employee master base URL
  $employeeApp = 'app-eappraisal-dev-svc-employee-master'
  $appraisalApp = 'app-eappraisal-dev-svc-appraisal-workflow'
  $employeeMasterBaseUrl = "https://$employeeApp.azurewebsites.net"

  Invoke-WithRetry -Description "Set EMPLOYEE_MASTER_BASE_URL for $appraisalApp" -MaxAttempts $global:RetryCount -DelaySeconds $global:RetryDelaySeconds -Action {
    Invoke-Az -Arguments @('webapp', 'config', 'appsettings', 'set', '--resource-group', $ResourceGroup, '--name', $appraisalApp, '--settings', "EMPLOYEE_MASTER_BASE_URL=$employeeMasterBaseUrl") -SuppressOutput
  }

  Write-Host "App settings applied." -ForegroundColor Green
}
else {
  Write-Host "[2/4] Skipped app settings." -ForegroundColor Yellow
}

$repoRoot = Split-Path -Parent $scriptRoot
$parentPom = Join-Path $repoRoot 'eappraisal-parent\\pom.xml'

if (-not $SkipBuild) {
  Assert-Command 'mvn'
  if (-not (Test-Path $parentPom)) {
    throw "Parent pom.xml not found: $parentPom"
  }

  Write-Host "[3/4] Building services with Maven..." -ForegroundColor Cyan
  if ($SkipTests) {
    mvn -f $parentPom clean package -DskipTests
  }
  else {
    mvn -f $parentPom clean package
  }

  if ($LASTEXITCODE -ne 0) {
    throw "Maven build failed."
  }
  Write-Host "Build completed." -ForegroundColor Green
}
else {
  Write-Host "[3/4] Skipped Maven build." -ForegroundColor Yellow
}

if (-not $SkipJarDeploy) {
  Write-Host "[4/4] Deploying JARs to App Services..." -ForegroundColor Cyan

  $moduleToApp = @{
    'svc-identity-access' = 'app-eappraisal-dev-svc-identity-access'
    'svc-employee-master' = 'app-eappraisal-dev-svc-employee-master'
    'svc-appraisal-workflow' = 'app-eappraisal-dev-svc-appraisal-workflow'
    'svc-comments-feedback' = 'app-eappraisal-dev-svc-comments-feedback'
    'svc-compensation-ctc' = 'app-eappraisal-dev-svc-compensation-ctc'
    'svc-audit-compliance' = 'app-eappraisal-dev-svc-audit-compliance'
  }

  if ($UseScmHostsOverride) {
    Write-Host "Applying SCM hosts override for local DNS stability..." -ForegroundColor Cyan
    Update-ScmHostsEntries -Apps @($moduleToApp.Values)
  }

  $deploymentMode = if ($SequentialDeploy) { "Sequential" } else { "Parallel" }
  Write-Host "Deployment mode: $deploymentMode" -ForegroundColor Cyan

  $deploymentIndex = 0
  foreach ($module in $moduleToApp.Keys) {
    $deploymentIndex++
    $app = $moduleToApp[$module]
    $targetDir = Join-Path $repoRoot ("eappraisal-parent\\$module\\target")

    if (-not (Test-Path $targetDir)) {
      throw "Target folder not found for module ${module}: $targetDir"
    }

    $jar = Get-ChildItem -Path $targetDir -Filter '*.jar' |
      Where-Object { $_.Name -notlike '*sources*' -and $_.Name -notlike '*javadoc*' -and $_.Name -notlike 'original-*' } |
      Sort-Object LastWriteTime -Descending |
      Select-Object -First 1

    if ($null -eq $jar) {
      throw "No deployable JAR found for module $module in $targetDir"
    }

    Write-Host " [$deploymentIndex/6] Deploying $($jar.Name) to $app" -ForegroundColor Yellow

    $scmHost = "$app.scm.azurewebsites.net"
    if (-not (Test-DnsResolvable -HostName $scmHost)) {
      if ($UseScmHostsOverride) {
        Write-Warning "SCM host is not currently resolvable: $scmHost. Continuing; hosts override/retries may still succeed."
      }
      else {
        Write-Warning "SCM host is not currently resolvable: $scmHost. Continuing with retries; rerun with -UseScmHostsOverride (as Administrator) if failures persist."
      }
    }

    Invoke-WithRetry -Description "Deploy jar to $app" -MaxAttempts $global:RetryCount -DelaySeconds $global:RetryDelaySeconds -Action {
      Invoke-Az -Arguments @('webapp', 'deploy', '--resource-group', $ResourceGroup, '--name', $app, '--src-path', $jar.FullName, '--type', 'jar', '--clean', 'true', '--restart', 'true', '--async', 'true', '--track-status', 'false') -SuppressOutput
    }

    # Sequential mode: wait between deployments to reduce Kudu load
    if ($SequentialDeploy -and $deploymentIndex -lt $moduleToApp.Count) {
      $waitSeconds = 45
      Write-Host "Waiting $waitSeconds seconds before next deployment (sequential mode)..." -ForegroundColor Cyan
      Start-Sleep -Seconds $waitSeconds
    }
  }

  Write-Host "JAR deployment completed." -ForegroundColor Green
}
else {
  Write-Host "[4/4] Skipped JAR deployment." -ForegroundColor Yellow
}

Write-Host "Full flow completed successfully." -ForegroundColor Green
