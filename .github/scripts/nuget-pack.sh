#!/usr/bin/env bash
set -euo pipefail

: "${PACKAGE_OUTPUT:?PACKAGE_OUTPUT is required}"
: "${ROOT_SLNX:?ROOT_SLNX is required}"
: "${VERSION:?VERSION is required}"

package_output="${PACKAGE_OUTPUT#./}"
rm -rf "$package_output"
mkdir -p "$package_output"

cat > /tmp/ci-nuget.config <<'NUGETEOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
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
  -p:UseLocalDevelopmentReferences=false \
  -p:NuGetAudit=false

dotnet build "$ROOT_SLNX" \
  --configuration Release \
  --no-restore \
  --verbosity minimal \
  -m \
  -p:PackageVersion="$VERSION" \
  -p:GeneratePackageOnBuild=false \
  -p:UseLocalDevelopmentReferences=false \
  -p:ContinuousIntegrationBuild=true \
  -p:BuildInParallel=true

mapfile -t solution_projects < <(
  sed -n 's:.*<Project Path="\([^"]\+\)".*:\1:p' "$ROOT_SLNX" |
    sed 's:\\:/:g' |
    sort -u
)

mapfile -t production_projects < <(
  find framework modules \
    -path '*/bin' -prune -o \
    -path '*/obj' -prune -o \
    -path '*/test' -prune -o \
    -path '*/host' -prune -o \
    -path 'modules/saas' -prune -o \
    -name '*.csproj' -type f -print |
    grep -v '/SufiChain\.SufiPlatform\.FileManager\.Demo/' |
    sort -u
)

if [ ${#production_projects[@]} -eq 0 ]; then
  echo "No production package projects were discovered." >&2
  exit 1
fi

packable_package_ids=()
packable_project_paths=()

for csproj in "${production_projects[@]}"; do
  package_id="$(sed -n 's:.*<PackageId>\(.*\)</PackageId>.*:\1:p' "$csproj" | head -n 1)"
  if [ -z "$package_id" ]; then
    package_id="$(basename "$csproj" .csproj)"
  fi

  packable_package_ids+=("$package_id")
  packable_project_paths+=("$csproj")
done

echo "Discovered ${#packable_package_ids[@]} production package project(s):"
for i in "${!packable_package_ids[@]}"; do
  printf '  [%d] %s (%s)\n' "$((i+1))" "${packable_package_ids[$i]}" "${packable_project_paths[$i]}"
done

for csproj in "${production_projects[@]}"; do
  if ! printf '%s\n' "${solution_projects[@]}" | grep -Fqx "$csproj"; then
    echo "Prebuilding and packing project outside $ROOT_SLNX: $csproj"
    dotnet restore "$csproj" \
      --configfile /tmp/ci-nuget.config \
      --verbosity minimal \
      -p:UseLocalDevelopmentReferences=false \
      -p:NuGetAudit=false

    dotnet pack "$csproj" \
      --configuration Release \
      --no-restore \
      --output "$package_output" \
      --verbosity minimal \
      -p:PackageVersion="$VERSION" \
      -p:GeneratePackageOnBuild=false \
      -p:UseLocalDevelopmentReferences=false \
      -p:ContinuousIntegrationBuild=true
  fi
done

dotnet pack "$ROOT_SLNX" \
  --configuration Release \
  --no-build \
  --output "$package_output" \
  --verbosity minimal \
  -p:PackageVersion="$VERSION" \
  -p:GeneratePackageOnBuild=false \
  -p:UseLocalDevelopmentReferences=false \
  -p:ContinuousIntegrationBuild=true

missing_package_ids=()
for i in "${!packable_package_ids[@]}"; do
  package_id="${packable_package_ids[$i]}"
  if [ ! -f "$package_output/${package_id}.${VERSION}.nupkg" ]; then
    missing_package_ids+=("$package_id")
  fi
done

if [ ${#missing_package_ids[@]} -gt 0 ]; then
  echo "Package verification failed. Missing ${#missing_package_ids[@]} package(s):" >&2
  printf '  %s\n' "${missing_package_ids[@]}" >&2
  exit 1
fi

package_count="$(find "$package_output" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' | wc -l | tr -d ' ')"
if [ "$package_count" = "0" ]; then
  echo "No NuGet packages were produced in $package_output." >&2
  exit 1
fi

stale_count="$(find "$package_output" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' ! -name "*.${VERSION}.nupkg" | wc -l | tr -d ' ')"
if [ "$stale_count" != "0" ]; then
  echo "Pack produced package(s) that do not match resolved version $VERSION:" >&2
  find "$package_output" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' ! -name "*.${VERSION}.nupkg" | sort >&2
  exit 1
fi

echo "Packed $package_count package(s) for version $VERSION into $package_output."
