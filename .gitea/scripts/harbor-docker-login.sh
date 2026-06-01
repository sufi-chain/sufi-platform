#!/usr/bin/env bash
set -euo pipefail

REGISTRY="${HARBOR_REGISTRY:-reg.sabp.ir}"

if [ -z "${HARBOR_USERNAME:-}" ] || [ -z "${HARBOR_PASSWORD:-}" ]; then
  echo "HARBOR_USERNAME and HARBOR_PASSWORD secrets are required." >&2
  exit 1
fi

echo "$HARBOR_PASSWORD" | docker login "$REGISTRY" \
  -u "$HARBOR_USERNAME" \
  --password-stdin

echo "Logged in to $REGISTRY"
