#!/usr/bin/env bash
set -euo pipefail

: "${PACKAGE_OUTPUT:?PACKAGE_OUTPUT is required}"
: "${ROOT_SLNX:?ROOT_SLNX is required}"
: "${VERSION:?VERSION is required}"

package_output="/src/${PACKAGE_OUTPUT#./}"
rm -rf "$package_output"
mkdir -p "$package_output"

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
  -p:BuildInParallel=true

dotnet pack "$ROOT_SLNX" \
  --configuration Release \
  --no-build \
  --output "$package_output" \
  --verbosity minimal \
  -p:PackageVersion="$VERSION" \
  -p:ContinuousIntegrationBuild=true

mapfile -t solution_projects < <(
  sed -n 's:.*<Project Path="\([^"]\+\)".*:\1:p' "$ROOT_SLNX" | sort -u
)

packable_package_ids=()
packable_project_paths=()

for csproj in "${solution_projects[@]}"; do
  if [ ! -f "$csproj" ]; then
    continue
  fi

  if ! grep -q '<IsPackable>true</IsPackable>' "$csproj"; then
    continue
  fi

  package_id="$(sed -n 's:.*<PackageId>\(.*\)</PackageId>.*:\1:p' "$csproj" | head -n 1)"
  if [ -z "$package_id" ]; then
    package_id="$(basename "$csproj" .csproj)"
  fi

  packable_package_ids+=("$package_id")
  packable_project_paths+=("$csproj")
done

if [ ${#packable_package_ids[@]} -eq 0 ]; then
  echo "No packable projects were discovered from $ROOT_SLNX." >&2
  exit 1
fi

echo "Discovered ${#packable_package_ids[@]} packable project(s):"
for i in "${!packable_package_ids[@]}"; do
  printf '  [%d] %s (%s)\n' "$((i+1))" "${packable_package_ids[$i]}" "${packable_project_paths[$i]}"
done

missing_package_ids=()
missing_project_paths=()

for i in "${!packable_package_ids[@]}"; do
  package_id="${packable_package_ids[$i]}"
  csproj="${packable_project_paths[$i]}"
  if [ ! -f "$package_output/${package_id}.${VERSION}.nupkg" ]; then
    missing_package_ids+=("$package_id")
    missing_project_paths+=("$csproj")
  fi
done

if [ ${#missing_package_ids[@]} -gt 0 ]; then
  echo "Package verification failed after root pack."
  echo "Missing ${#missing_package_ids[@]} package(s) out of ${#packable_package_ids[@]} total:"
  for i in "${!missing_package_ids[@]}"; do
    printf '  [%d] %s (%s)\n' "$((i+1))" "${missing_package_ids[$i]}" "${missing_project_paths[$i]}"
  done
  echo "Running targeted restore/build/pack for missing project(s)..."

  for csproj in "${missing_project_paths[@]}"; do
    dotnet restore "$csproj" \
      --configfile /tmp/ci-nuget.config \
      --verbosity minimal \
      -p:NuGetAudit=false

    dotnet build "$csproj" \
      --configuration Release \
      --no-restore \
      --verbosity minimal \
      -m \
      -p:PackageVersion="$VERSION" \
      -p:ContinuousIntegrationBuild=true \
      -p:BuildInParallel=true

    dotnet pack "$csproj" \
      --configuration Release \
      --no-build \
      --output "$package_output" \
      --verbosity minimal \
      -p:PackageVersion="$VERSION" \
      -p:ContinuousIntegrationBuild=true
  done

  missing_package_ids=()
  for i in "${!packable_package_ids[@]}"; do
    package_id="${packable_package_ids[$i]}"
    if [ ! -f "$package_output/${package_id}.${VERSION}.nupkg" ]; then
      missing_package_ids+=("$package_id")
    fi
  done

  if [ ${#missing_package_ids[@]} -gt 0 ]; then
    echo "Package verification still failing after targeted repack."
    echo "Missing ${#missing_package_ids[@]} package(s):"
    printf '  %s\n' "${missing_package_ids[@]}"
    exit 1
  fi
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
