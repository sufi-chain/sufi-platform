#!/usr/bin/env bash
set -euo pipefail

mapfile -t packages < <(find "$PACKAGE_OUTPUT" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' | sort)
if [ ${#packages[@]} -eq 0 ]; then
  echo "No NuGet packages found in artifact."
  exit 1
fi

printf '%s\0' "${packages[@]}" | xargs -0 -n 1 -P "${NUGET_PUSH_PARALLELISM:-4}" sh -c '
  dotnet nuget push "$1" \
    --source "$BAGET_SOURCE_URL" \
    --api-key "$NUGET_API_KEY" \
    --skip-duplicate
' sh
