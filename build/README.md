# SufiAbp Template Build & Deployment

This directory contains scripts for generating and deploying SufiAbp templates to CDN.

## Scripts

### 1. generate-template-zip.ps1
Generates the unified template ZIP containing all architecture variants.

**Usage:**
```powershell
# Generate with default version (1.0.0-alpha.1.0)
.\build\generate-template-zip.ps1

# Generate with specific version
.\build\generate-template-zip.ps1 -Version "1.0.0-alpha.2.0"

# Custom output directory
.\build\generate-template-zip.ps1 -Version "1.0.0-alpha.1.0" -OutputDir "../dist/templates"
```

**Output:**
- `dist/templates/app-blazor-webapp-unified.zip` - Unified template ZIP
- `dist/templates/latest.json` - Version manifest

### 2. deploy-to-cdn.ps1
Uploads template ZIP and manifest to CDN server.

**Prerequisites:**
- SSH access to cdn.sabp.ir
- SSH key configured for passwordless login

**Usage:**
```powershell
# Deploy with default version
.\build\deploy-to-cdn.ps1

# Deploy specific version
.\build\deploy-to-cdn.ps1 -Version "1.0.0-alpha.2.0"
```

**CDN Structure:**
```
/var/www/cdn/sufi-abp/
├── latest.json                                    # Points to latest version
├── 1.0.0-alpha.1.0/
│   ├── latest.json                               # Version-specific manifest
│   └── templates/
│       └── app-blazor-webapp-unified.zip
└── 1.0.0-alpha.2.0/
    ├── latest.json
    └── templates/
        └── app-blazor-webapp-unified.zip
```

## Workflow

### For Platform Developers (Debug Mode)

When developing SufiAbp framework:

1. CLI runs from `framework/SufiChain.SufiAbp.CLI.Core/bin/Debug/net10.0/`
2. Template loaded from `sufi-abp/templates/app/aspnet-core/` (filesystem)
3. No ZIP generation needed for local development

### For Release (End Users)

When releasing a new CLI version:

1. **Update version** in `versions.props`:
   ```xml
   <SufiVersion>1.0.0-alpha.2.0</SufiVersion>
   ```

2. **Generate template ZIP**:
   ```powershell
   .\build\generate-template-zip.ps1 -Version "1.0.0-alpha.2.0"
   ```

3. **Deploy to CDN**:
   ```powershell
   .\build\deploy-to-cdn.ps1 -Version "1.0.0-alpha.2.0"
   ```

4. **Build and publish CLI**:
   ```bash
   cd framework/SufiChain.SufiAbp.CLI
   dotnet pack -c Release
   dotnet nuget push bin/Release/SufiChain.SufiAbp.CLI.1.0.0-alpha.2.0.nupkg --source https://nuget.sabp.ir/v3/index.json
   ```

5. **Test end-user scenario**:
   ```bash
   dotnet tool install --global SufiChain.SufiAbp.CLI --version 1.0.0-alpha.2.0 --add-source https://nuget.sabp.ir/v3/index.json
   sufi new MyTestApp
   ```

## Template Structure

The unified template at `sufi-abp/templates/app/aspnet-core/` contains:

- **All architecture variants** (single, layered, layered-tiered) in one structure
- **Template markers** (`<TEMPLATE-REMOVE>`, `<TEMPLATE-ONLY>`) processed by CLI
- **Build files**: `versions.props`, `Directory.Build.props`, `common.props`
- **Project references** to modules (for Debug mode)

## Troubleshooting

### Template not found in Debug mode
- Verify path: `sufi-abp/templates/app/aspnet-core/` exists
- Check for `.sln` file in template directory

### CDN download fails
- Verify `latest.json` is accessible: `curl https://cdn.sabp.ir/sufi-abp/latest.json`
- Check template ZIP URL in manifest
- Ensure CDN permissions are correct (755 for directories, 644 for files)

### Build errors after scaffolding
- Verify `versions.props` has `<SufiVersion>` defined
- Check project reference paths (should be `..\..\..\..\..\..\modules\`, not `..\..\..\..\..\..\src\modules\`)
