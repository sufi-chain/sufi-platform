#!/usr/bin/env bash
set -euo pipefail

: "${PACKAGE_OUTPUT:?PACKAGE_OUTPUT is required}"
: "${NUGET_ORG_SOURCE_URL:?NUGET_ORG_SOURCE_URL is required}"
: "${NUGET_ORG_API_KEY:?NUGET_ORG_API_KEY is required}"
: "${VERSION:?VERSION is required}"

package_output="${PACKAGE_OUTPUT#./}"

mapfile -t skipped_packages < <(find "$package_output" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' ! -name "*.${VERSION}.nupkg" | sort)
if [ ${#skipped_packages[@]} -gt 0 ]; then
  echo "Removing ${#skipped_packages[@]} package(s) that do not match resolved version $VERSION:"
  printf '  %s\n' "${skipped_packages[@]}"
  rm -f -- "${skipped_packages[@]}"
fi

mapfile -t packages < <(find "$package_output" -type f -name "*.${VERSION}.nupkg" ! -name '*.symbols.nupkg' | sort)
if [ ${#packages[@]} -eq 0 ]; then
  echo "No NuGet packages found for resolved version $VERSION in artifact."
  exit 1
fi

echo "Pushing ${#packages[@]} package(s) for version $VERSION to nuget.org: $NUGET_ORG_SOURCE_URL"

printf '%s\0' "${packages[@]}" | xargs -0 -n 1 -P "${NUGET_PUSH_PARALLELISM:-4}" sh -c '
  package="$1"
  for attempt in 1 2 3; do
    if dotnet nuget push "$package" \
      --source "$NUGET_ORG_SOURCE_URL" \
      --api-key "$NUGET_ORG_API_KEY" \
      --skip-duplicate; then
      exit 0
    fi
    echo "Push failed for $package (attempt $attempt/3)." >&2
    sleep $((attempt * 5))
  done
  exit 1
' sh
