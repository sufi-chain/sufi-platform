# SufiAbp Template ZIP Generator
# Generates unified template ZIP containing all architecture variants

param(
    [string]$Version = "1.0.0-alpha.1.0",
    [string]$OutputDir = "../dist/templates"
)

$ErrorActionPreference = "Stop"

Write-Host "=== SufiAbp Template ZIP Generator ===" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow

# Paths
$RepoRoot = Split-Path $PSScriptRoot -Parent
$TemplateSource = Join-Path $RepoRoot "sufi-abp/templates/app/aspnet-core"
$OutputPath = Join-Path $RepoRoot $OutputDir
$ZipFileName = "app-blazor-webapp-unified.zip"
$ZipFullPath = Join-Path $OutputPath "$ZipFileName"

# Validate source exists
if (-not (Test-Path $TemplateSource)) {
    Write-Error "Template source not found: $TemplateSource"
    exit 1
}

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

# Remove old ZIP if exists
if (Test-Path $ZipFullPath) {
    Write-Host "Removing old ZIP: $ZipFullPath" -ForegroundColor Yellow
    Remove-Item $ZipFullPath -Force
}

Write-Host "Creating unified template ZIP..." -ForegroundColor Green
Write-Host "  Source: $TemplateSource" -ForegroundColor Gray
Write-Host "  Output: $ZipFullPath" -ForegroundColor Gray

# Create ZIP (exclude bin, obj, .vs, node_modules)
$TempDir = Join-Path $env:TEMP "sufiabp-template-$(New-Guid)"
New-Item -ItemType Directory -Force -Path $TempDir | Out-Null

try {
    # Copy template to temp directory
    Write-Host "Copying template files..." -ForegroundColor Gray
    Copy-Item -Path "$TemplateSource\*" -Destination $TempDir -Recurse -Force
    
    # Remove build artifacts and IDE folders
    Write-Host "Cleaning build artifacts..." -ForegroundColor Gray
    Get-ChildItem -Path $TempDir -Include bin,obj,.vs,.idea,node_modules -Recurse -Directory | Remove-Item -Recurse -Force
    Get-ChildItem -Path $TempDir -Include *.user,*.suo -Recurse -File | Remove-Item -Force
    
    # Create ZIP
    Write-Host "Compressing to ZIP..." -ForegroundColor Gray
    Compress-Archive -Path "$TempDir\*" -DestinationPath $ZipFullPath -CompressionLevel Optimal -Force
    
    # Calculate SHA256
    Write-Host "Calculating SHA256..." -ForegroundColor Gray
    $Hash = (Get-FileHash -Path $ZipFullPath -Algorithm SHA256).Hash
    $Size = (Get-Item $ZipFullPath).Length
    
    Write-Host "`n=== Template ZIP Created ===" -ForegroundColor Green
    Write-Host "  File: $ZipFullPath" -ForegroundColor White
    Write-Host "  Size: $([math]::Round($Size / 1MB, 2)) MB" -ForegroundColor White
    Write-Host "  SHA256: $Hash" -ForegroundColor White
    
    # Generate manifest JSON
    $ManifestPath = Join-Path $OutputPath "latest.json"
    $Manifest = @{
        version = $Version
        templates = @{
            "app-blazor-webapp-unified" = @{
                url = "https://cdn.sabp.ir/sufi-abp/$Version/templates/$ZipFileName"
                size = $Size
                sha256 = $Hash.ToLower()
            }
        }
    } | ConvertTo-Json -Depth 10
    
    $Manifest | Out-File -FilePath $ManifestPath -Encoding UTF8 -Force
    Write-Host "`n  Manifest: $ManifestPath" -ForegroundColor White
    
} finally {
    # Cleanup temp directory
    if (Test-Path $TempDir) {
        Remove-Item $TempDir -Recurse -Force
    }
}

Write-Host "`n=== Done ===" -ForegroundColor Cyan
