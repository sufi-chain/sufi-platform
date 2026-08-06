#!/usr/bin/env bash
set -euo pipefail

TARGET_DIR="${1:-.}"
REPOSITORY="${REPOSITORY:-sufi-chain/sufi-platform}"
REF_NAME="${REF_NAME:-}"
GIT_SERVER_URL="${GIT_SERVER_URL:-https://github.com}"
SDK_IMAGE="${SDK_IMAGE:-${GIT_CLONE_IMAGE:-mcr.microsoft.com/dotnet/sdk:10.0}}"

script_dir="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
workspace="$("$script_dir/resolve-workspace.sh")"

CLONE_URL="${GIT_SERVER_URL%/}/${REPOSITORY}.git"
if [ -n "${GITEATOKEN:-}" ]; then
  CLONE_URL="$(printf '%s' "$CLONE_URL" | sed "s#^https://#https://x-access-token:${GITEATOKEN}@#")"
fi
export CLONE_URL

if [ "$TARGET_DIR" = "." ]; then
  docker run --rm \
    -v "${workspace}:/work" \
    -w /work \
    -e REF_NAME \
    -e CLONE_URL \
    "$SDK_IMAGE" \
    sh -c '
      set -eu
      if [ -n "${REF_NAME:-}" ]; then
        git clone --depth 1 --branch "$REF_NAME" "$CLONE_URL" _checkout
      else
        git clone --depth 1 "$CLONE_URL" _checkout
      fi
      cp -a _checkout/. ./
      rm -rf _checkout
    '
else
  mkdir -p "$workspace/$TARGET_DIR"
  docker run --rm \
    -v "${workspace}:/work" \
    -w /work \
    -e REF_NAME \
    -e CLONE_URL \
    -e TARGET_DIR \
    "$SDK_IMAGE" \
    sh -c '
      set -eu
      if [ -n "${REF_NAME:-}" ]; then
        git clone --depth 1 --branch "$REF_NAME" "$CLONE_URL" "$TARGET_DIR"
      else
        git clone --depth 1 "$CLONE_URL" "$TARGET_DIR"
      fi
    '
fi

echo "Checkout complete: $TARGET_DIR (${REPOSITORY}${REF_NAME:+, ref ${REF_NAME}})"
