#!/usr/bin/env bash
set -euo pipefail

: "${PACKAGE_OUTPUT:?PACKAGE_OUTPUT is required}"
: "${BAGET_SOURCE_URL:?BAGET_SOURCE_URL is required}"
: "${NUGET_API_KEY:?NUGET_API_KEY is required}"

mapfile -t packages < <(find "$PACKAGE_OUTPUT" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' | sort)
if [ ${#packages[@]} -eq 0 ]; then
  echo "No NuGet packages found in artifact."
  exit 1
fi

echo "Pushing ${#packages[@]} package(s) to BaGet: $BAGET_SOURCE_URL"

printf '%s\0' "${packages[@]}" | xargs -0 -n 1 -P "${NUGET_PUSH_PARALLELISM:-4}" sh -c '
  package="$1"
  for attempt in 1 2 3; do
    if dotnet nuget push "$package" \
      --source "$BAGET_SOURCE_URL" \
      --api-key "$NUGET_API_KEY" \
      --skip-duplicate; then
      exit 0
    fi
    echo "Push failed for $package (attempt $attempt/3)." >&2
    sleep $((attempt * 5))
  done
  exit 1
' sh
