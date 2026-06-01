#!/usr/bin/env bash
# Clone the current Gitea repository into a target directory.
# Uses host git when available; otherwise clones via Docker (runners without git, e.g. act).
set -euo pipefail

TARGET_DIR="${1:-.}"
REPOSITORY="${REPOSITORY:-sufi-chain/sufi-abp}"
REF_NAME="${REF_NAME:-}"
GIT_HOST="${GIT_HOST:-git.sabp.ir}"
repo_url="https://${GIT_HOST}/${REPOSITORY}.git"

clone_with_git() {
  local dest="$1"
  if [ -n "$REF_NAME" ]; then
    git clone --depth 1 --branch "$REF_NAME" "$repo_url" "$dest"
  else
    git clone --depth 1 "$repo_url" "$dest"
  fi
}

clone_with_docker() {
  local dest="$1"
  local ws parent mount_dest

  if [ "$dest" = "." ]; then
    ws="$(pwd)"
    parent="$(dirname "$ws")"
    mount_dest="$(basename "$ws")"
    docker run --rm \
      -v "${parent}:/work" \
      -w "/work/${mount_dest}" \
      -e REF_NAME \
      -e repo_url \
      "$GIT_CLONE_IMAGE" \
      bash -c '
        set -euo pipefail
        if [ -n "${REF_NAME:-}" ]; then
          git clone --depth 1 --branch "$REF_NAME" "$repo_url" _checkout
        else
          git clone --depth 1 "$repo_url" _checkout
        fi
        shopt -s dotglob
        mv _checkout/* ./
        rmdir _checkout
      '
    return
  fi

  ws="$(pwd)"
  mkdir -p "$dest"
  docker run --rm \
    -v "${ws}:/work" \
    -w /work \
    -e REF_NAME \
    -e repo_url \
    -e dest="$dest" \
    "$GIT_CLONE_IMAGE" \
    bash -c '
      set -euo pipefail
      if [ -n "${REF_NAME:-}" ]; then
        git clone --depth 1 --branch "$REF_NAME" "$repo_url" "$dest"
      else
        git clone --depth 1 "$repo_url" "$dest"
      fi
    '
}

if command -v git >/dev/null 2>&1; then
  clone_with_git "$TARGET_DIR"
else
  if [ -z "${GIT_CLONE_IMAGE:-}" ]; then
    echo "GIT_CLONE_IMAGE is required when git is not installed on the runner." >&2
    exit 1
  fi
  echo "git not found on runner; cloning with Docker image: $GIT_CLONE_IMAGE"
  clone_with_docker "$TARGET_DIR"
fi

echo "Checkout complete: $TARGET_DIR (${REPOSITORY}${REF_NAME:+, ref ${REF_NAME}})"
