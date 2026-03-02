# Phase 1 Dev - Azure App Service Settings Backup/Restore (PowerShell + Azure CLI)
# Modes:
#   Backup : saves current app settings for all services into JSON files
#   Restore: restores app settings from JSON files
#
# Usage examples:
#   .\Phase1_Dev_Azure_AppSettings_Rollback_Template.ps1 -Mode Backup
#   .\Phase1_Dev_Azure_AppSettings_Rollback_Template.ps1 -Mode Restore

param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('Backup', 'Restore')]
  [string]$Mode,

  [string]$ResourceGroup = 'rg-eappraisal-dev-win',
  [string]$BackupFolder = '.\\appsettings-backup'
)

$ErrorActionPreference = 'Stop'

$apps = @(
  'app-eappraisal-dev-svc-identity-access',
  'app-eappraisal-dev-svc-employee-master',
  'app-eappraisal-dev-svc-appraisal-workflow',
  'app-eappraisal-dev-svc-comments-feedback',
  'app-eappraisal-dev-svc-compensation-ctc',
  'app-eappraisal-dev-svc-audit-compliance'
)

function Backup-AppSettings {
  if (-not (Test-Path $BackupFolder)) {
    New-Item -ItemType Directory -Path $BackupFolder | Out-Null
  }

  foreach ($app in $apps) {
    Write-Host "Backing up app settings: $app" -ForegroundColor Yellow

    $outputPath = Join-Path $BackupFolder "$app.appsettings.json"

    az webapp config appsettings list `
      --resource-group $ResourceGroup `
      --name $app `
      --output json > $outputPath

    if (-not (Test-Path $outputPath)) {
      throw "Backup failed for $app"
    }
  }

  Write-Host "Backup completed. Files are in: $BackupFolder" -ForegroundColor Green
}

function Restore-AppSettings {
  foreach ($app in $apps) {
    $inputPath = Join-Path $BackupFolder "$app.appsettings.json"
    if (-not (Test-Path $inputPath)) {
      throw "Missing backup file: $inputPath"
    }

    Write-Host "Restoring app settings: $app" -ForegroundColor Yellow

    $settings = Get-Content -Raw -Path $inputPath | ConvertFrom-Json

    $kvPairs = @()
    foreach ($item in $settings) {
      if ($null -ne $item.name -and $null -ne $item.value) {
        $kvPairs += ("{0}={1}" -f $item.name, $item.value)
      }
    }

    if ($kvPairs.Count -eq 0) {
      Write-Warning "No settings found in backup for $app; skipping"
      continue
    }

    az webapp config appsettings set `
      --resource-group $ResourceGroup `
      --name $app `
      --settings $kvPairs `
      --output none

    az webapp restart `
      --resource-group $ResourceGroup `
      --name $app `
      --output none
  }

  Write-Host "Restore completed for all apps." -ForegroundColor Green
}

Write-Host "Mode: $Mode | Resource Group: $ResourceGroup" -ForegroundColor Cyan

if ($Mode -eq 'Backup') {
  Backup-AppSettings
} elseif ($Mode -eq 'Restore') {
  Restore-AppSettings
}
