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

mapfile -t calendar_packages < <(
  find modules/calendar/src -type f -name '*.csproj' | sort | while read -r csproj; do
    package_id="$(sed -n 's:.*<PackageId>\(.*\)</PackageId>.*:\1:p' "$csproj" | head -n 1)"
    if [ -z "$package_id" ]; then
      package_id="$(basename "$csproj" .csproj)"
    fi
    echo "$package_id"
  done | sort -u
)

missing_calendar_packages=()
for package_id in "${calendar_packages[@]}"; do
  if [ ! -f "$package_output/${package_id}.${VERSION}.nupkg" ]; then
    missing_calendar_packages+=("$package_id")
  fi
done

if [ ${#missing_calendar_packages[@]} -gt 0 ]; then
  echo "Calendar package verification failed after root pack."
  echo "Missing ${#missing_calendar_packages[@]} package(s):"
  printf '  %s\n' "${missing_calendar_packages[@]}"
  echo "Running targeted calendar restore/build/pack for src projects only..."

  calendar_slnx="modules/calendar/SufiChain.SufiAbp.Calendar.slnx"
  calendar_release_slnx="modules/calendar/SufiChain.SufiAbp.Calendar.ReleasePack.slnx"
  grep -v '/test/' "$calendar_slnx" > "$calendar_release_slnx"
  trap 'rm -f "$calendar_release_slnx"' EXIT

  dotnet restore "$calendar_release_slnx" \
    --configfile /tmp/ci-nuget.config \
    --verbosity minimal \
    -p:NuGetAudit=false

  dotnet build "$calendar_release_slnx" \
    --configuration Release \
    --no-restore \
    --verbosity minimal \
    -m \
    -p:PackageVersion="$VERSION" \
    -p:ContinuousIntegrationBuild=true \
    -p:BuildInParallel=true

  dotnet pack "$calendar_release_slnx" \
    --configuration Release \
    --no-build \
    --output "$package_output" \
    --verbosity minimal \
    -p:PackageVersion="$VERSION" \
    -p:ContinuousIntegrationBuild=true

  missing_calendar_packages=()
  for package_id in "${calendar_packages[@]}"; do
    if [ ! -f "$package_output/${package_id}.${VERSION}.nupkg" ]; then
      missing_calendar_packages+=("$package_id")
    fi
  done

  if [ ${#missing_calendar_packages[@]} -gt 0 ]; then
    echo "Calendar package verification still failing after targeted repack."
    echo "Missing ${#missing_calendar_packages[@]} package(s):"
    printf '  %s\n' "${missing_calendar_packages[@]}"
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
