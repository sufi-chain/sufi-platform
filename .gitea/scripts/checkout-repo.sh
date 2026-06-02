#!/usr/bin/env bash
set -euo pipefail

TARGET_DIR="${1:-.}"
REPOSITORY="${REPOSITORY:-sufi-chain/sufi-abp}"
REF_NAME="${REF_NAME:-}"
GIT_HOST="${GIT_HOST:-git.sabp.ir}"
SDK_IMAGE="${SDK_IMAGE:-${GIT_CLONE_IMAGE:-reg.sabp.ir/sufi-chain/dotnet-sdk-build:10.0.202}}"

repo_url="https://${GIT_HOST}/${REPOSITORY}.git"
if [ -n "${GITEATOKEN:-}" ]; then
  repo_url="$(printf '%s' "$repo_url" | sed "s#^https://#https://x-access-token:${GITEATOKEN}@#")"
fi

if [ "$TARGET_DIR" = "." ]; then
  docker run --rm \
    -v "${PWD}:/work" \
    -w /work \
    -e REF_NAME \
    -e repo_url \
    "$SDK_IMAGE" \
    sh -c '
      set -eu
      if [ -n "${REF_NAME:-}" ]; then
        git clone --depth 1 --branch "$REF_NAME" "$repo_url" _checkout
      else
        git clone --depth 1 "$repo_url" _checkout
      fi
      cp -a _checkout/. ./
      rm -rf _checkout
    '
else
  mkdir -p "$TARGET_DIR"
  docker run --rm \
    -v "${PWD}:/work" \
    -w /work \
    -e REF_NAME \
    -e repo_url \
    -e TARGET_DIR \
    "$SDK_IMAGE" \
    sh -c '
      set -eu
      if [ -n "${REF_NAME:-}" ]; then
        git clone --depth 1 --branch "$REF_NAME" "$repo_url" "$TARGET_DIR"
      else
        git clone --depth 1 "$repo_url" "$TARGET_DIR"
      fi
    '
fi

echo "Checkout complete: $TARGET_DIR (${REPOSITORY}${REF_NAME:+, ref ${REF_NAME}})"
