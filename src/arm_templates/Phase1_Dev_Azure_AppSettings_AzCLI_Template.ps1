# Phase 1 Dev - Azure App Service Settings Template (PowerShell + Azure CLI)
# Usage:
# 1) Update the variables in the CONFIG section.
# 2) Sign in: az login
# 3) Run: .\Phase1_Dev_Azure_AppSettings_AzCLI_Template.ps1

$ErrorActionPreference = 'Stop'

# -----------------------------
# CONFIG
# -----------------------------
$resourceGroup = 'rg-eappraisal-dev-win'

# Azure SQL (shared across services)
$dbUrl = 'jdbc:sqlserver://<sql-server>.database.windows.net:1433;database=<db-name>;encrypt=true;trustServerCertificate=false;hostNameInCertificate=*.database.windows.net;loginTimeout=30;'
$dbUsername = '<azure-sql-username>'
$dbPassword = '<azure-sql-password>'
$dbDriver = 'com.microsoft.sqlserver.jdbc.SQLServerDriver'
$hibernateDialect = 'org.hibernate.dialect.SQLServerDialect'

# Storage
$storageAccountName = 'stgeappraisaldev'
$storageContainerExports = 'exports'

# Service-specific
$employeeMasterBaseUrl = 'https://app-eappraisal-dev-svc-employee-master.azurewebsites.net'

# Apps
$apps = @(
  'app-eappraisal-dev-svc-identity-access',
  'app-eappraisal-dev-svc-employee-master',
  'app-eappraisal-dev-svc-appraisal-workflow',
  'app-eappraisal-dev-svc-comments-feedback',
  'app-eappraisal-dev-svc-compensation-ctc',
  'app-eappraisal-dev-svc-audit-compliance'
)

# -----------------------------
# COMMON SETTINGS (all services)
# -----------------------------
$commonSettings = @(
  "SPRING_PROFILES_ACTIVE=dev",
  "DB_URL=$dbUrl",
  "DB_DRIVER_CLASS_NAME=$dbDriver",
  "HIBERNATE_DIALECT=$hibernateDialect",
  "DB_USERNAME=$dbUsername",
  "DB_PASSWORD=$dbPassword",
  "STORAGE_ACCOUNT_NAME=$storageAccountName",
  "STORAGE_CONTAINER_EXPORTS=$storageContainerExports"
)

Write-Host "Applying common app settings to all service apps in $resourceGroup..." -ForegroundColor Cyan

foreach ($app in $apps) {
  Write-Host " -> $app" -ForegroundColor Yellow

  az webapp config appsettings set `
    --resource-group $resourceGroup `
    --name $app `
    --settings $commonSettings `
    --output table | Out-Null

  az webapp update `
    --resource-group $resourceGroup `
    --name $app `
    --https-only true `
    --output none
}

# -----------------------------
# SERVICE-SPECIFIC SETTINGS
# -----------------------------
Write-Host "Applying appraisal workflow specific settings..." -ForegroundColor Cyan

az webapp config appsettings set `
  --resource-group $resourceGroup `
  --name 'app-eappraisal-dev-svc-appraisal-workflow' `
  --settings "EMPLOYEE_MASTER_BASE_URL=$employeeMasterBaseUrl" `
  --output table | Out-Null

Write-Host "Done. Settings applied successfully." -ForegroundColor Green
Write-Host "Next: restart apps if needed: az webapp restart --resource-group <rg> --name <app-name>" -ForegroundColor DarkGray
