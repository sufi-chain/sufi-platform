#!/usr/bin/env bash
set -euo pipefail

mkdir -p "$PACKAGE_OUTPUT"

dotnet restore "$ROOT_SLNX" \
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
