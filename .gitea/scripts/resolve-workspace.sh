#!/usr/bin/env bash
# Prints the absolute job workspace directory (no trailing slash).
set -euo pipefail

workspace="${WORKSPACE_DIR:-${GITHUB_WORKSPACE:-${GITEA_WORKSPACE:-$PWD}}}"
cd "$workspace"
printf '%s\n' "$(pwd)"
