# SufiAbp Template CDN Deployment Script
# Uploads template ZIP and manifest to CDN

param(
    [string]$Version = "1.0.0-alpha.1.0",
    [string]$SourceDir = "../dist/templates",
    [string]$CdnPath = "cdn.sabp.ir:/var/www/cdn/sufi-abp"
)

$ErrorActionPreference = "Stop"

Write-Host "=== SufiAbp CDN Deployment ===" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow

# Paths
$RepoRoot = Split-Path $PSScriptRoot -Parent
$SourcePath = Join-Path $RepoRoot $SourceDir
$ZipFile = Join-Path $SourcePath "app-blazor-webapp-unified.zip"
$ManifestFile = Join-Path $SourcePath "latest.json"

# Validate files exist
if (-not (Test-Path $ZipFile)) {
    Write-Error "Template ZIP not found: $ZipFile. Run generate-template-zip.ps1 first."
    exit 1
}

if (-not (Test-Path $ManifestFile)) {
    Write-Error "Manifest not found: $ManifestFile. Run generate-template-zip.ps1 first."
    exit 1
}

Write-Host "`nFiles to upload:" -ForegroundColor Green
Write-Host "  ZIP: $ZipFile" -ForegroundColor Gray
Write-Host "  Manifest: $ManifestFile" -ForegroundColor Gray

# CDN structure: /var/www/cdn/sufi-abp/{version}/templates/
$CdnVersionPath = "$CdnPath/$Version/templates"

Write-Host "`nCDN Destination:" -ForegroundColor Green
Write-Host "  $CdnVersionPath" -ForegroundColor Gray

# Upload using SCP (requires SSH access to CDN server)
Write-Host "`nUploading to CDN..." -ForegroundColor Yellow

# Create version directory on CDN
Write-Host "Creating CDN directory structure..." -ForegroundColor Gray
ssh cdn.sabp.ir "mkdir -p $CdnVersionPath"

# Upload ZIP
Write-Host "Uploading template ZIP..." -ForegroundColor Gray
scp $ZipFile "cdn.sabp.ir:$CdnVersionPath/"

# Upload manifest to version directory
Write-Host "Uploading version manifest..." -ForegroundColor Gray
scp $ManifestFile "cdn.sabp.ir:$CdnVersionPath/../"

# Update latest.json at root
Write-Host "Updating latest.json at CDN root..." -ForegroundColor Gray
scp $ManifestFile "cdn.sabp.ir:$CdnPath/latest.json"

# Set permissions
Write-Host "Setting permissions..." -ForegroundColor Gray
ssh cdn.sabp.ir "chmod -R 755 $CdnPath/$Version && chown -R www-data:www-data $CdnPath/$Version"

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Template URL: https://cdn.sabp.ir/sufi-abp/$Version/templates/app-blazor-webapp-unified.zip" -ForegroundColor White
Write-Host "Manifest URL: https://cdn.sabp.ir/sufi-abp/latest.json" -ForegroundColor White

Write-Host "`nTest the deployment:" -ForegroundColor Yellow
Write-Host "  curl https://cdn.sabp.ir/sufi-abp/latest.json" -ForegroundColor Gray
