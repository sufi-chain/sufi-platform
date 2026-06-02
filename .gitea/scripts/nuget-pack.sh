#!/usr/bin/env bash
set -euo pipefail

: "${PACKAGE_OUTPUT:?PACKAGE_OUTPUT is required}"
: "${ROOT_SLNX:?ROOT_SLNX is required}"
: "${VERSION:?VERSION is required}"

mkdir -p "$PACKAGE_OUTPUT"

cat > /tmp/ci-nuget.config <<'NUGETEOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="liara" value="https://package-mirror.liara.ir/repository/nuget/index.json" />
    <add key="sufi-abp" value="https://nuget.sabp.ir/v3/index.json" />
  </packageSources>
  <auditSources>
    <clear />
  </auditSources>
</configuration>
NUGETEOF

if [ ! -f "$ROOT_SLNX" ]; then
  echo "Solution file was not found: $ROOT_SLNX" >&2
  exit 1
fi

dotnet restore "$ROOT_SLNX" \
  --configfile /tmp/ci-nuget.config \
  --verbosity minimal \
  -p:NuGetAudit=false

dotnet build "$ROOT_SLNX" \
  --configuration Release \
  --no-restore \
  --verbosity minimal \
  -m \
  -p:PackageVersion="$VERSION" \
  -p:ContinuousIntegrationBuild=true \
  -p:BuildInParallel=true \
  -p:PackageOutputPath="/src/${PACKAGE_OUTPUT#./}" \
  -p:GeneratePackageOnBuild=true
