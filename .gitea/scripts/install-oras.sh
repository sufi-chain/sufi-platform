#!/usr/bin/env bash
# Installs ORAS CLI into /usr/local/bin (requires root) or $ORAS_INSTALL_DIR.
set -euo pipefail

ORAS_VERSION="${ORAS_VERSION:-1.2.2}"
INSTALL_DIR="${ORAS_INSTALL_DIR:-/usr/local/bin}"
ARCH="$(uname -m)"

case "$ARCH" in
  x86_64) ORAS_ARCH=amd64 ;;
  aarch64) ORAS_ARCH=arm64 ;;
  *)
    echo "Unsupported architecture: $ARCH" >&2
    exit 1
    ;;
esac

tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT

archive="oras_${ORAS_VERSION}_linux_${ORAS_ARCH}.tar.gz"
curl -fsSL "https://github.com/oras-project/oras/releases/download/v${ORAS_VERSION}/${archive}" \
  -o "$tmp_dir/$archive"
tar -xzf "$tmp_dir/$archive" -C "$tmp_dir" oras

mkdir -p "$INSTALL_DIR"
install -m 0755 "$tmp_dir/oras" "$INSTALL_DIR/oras"
oras version
