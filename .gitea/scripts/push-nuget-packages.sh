#!/usr/bin/env bash
# Push all .nupkg files from a directory to a NuGet feed in parallel.
set -euo pipefail

if [ "$#" -lt 3 ]; then
  echo "Usage: $0 <package-dir> <source-url> <api-key> [parallelism]" >&2
  exit 1
fi

PACKAGE_DIR="$1"
SOURCE_URL="$2"
API_KEY="$3"
PARALLELISM="${4:-4}"

if [ -z "$API_KEY" ]; then
  echo "API key is required." >&2
  exit 1
fi

mapfile -t packages < <(find "$PACKAGE_DIR" -type f -name '*.nupkg' ! -name '*.symbols.nupkg' | sort)
if [ ${#packages[@]} -eq 0 ]; then
  echo "No NuGet packages found in $PACKAGE_DIR" >&2
  exit 1
fi

echo "Pushing ${#packages[@]} package(s) to $SOURCE_URL (parallelism=$PARALLELISM)"

export SOURCE_URL API_KEY
printf '%s\0' "${packages[@]}" | xargs -0 -n 1 -P "$PARALLELISM" sh -c '
  dotnet nuget push "$1" \
    --source "$SOURCE_URL" \
    --api-key "$API_KEY" \
    --skip-duplicate
' sh

echo "Push completed."
