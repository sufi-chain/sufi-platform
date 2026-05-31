#!/usr/bin/env bash
# Upload or download a NuGet package bundle tarball via Harbor (ORAS OCI artifact).
set -euo pipefail

usage() {
  echo "Usage:" >&2
  echo "  $0 upload <package-dir> <harbor-ref> <version>" >&2
  echo "  $0 download <harbor-ref> <version> <extract-dir>" >&2
  exit 1
}

MEDIA_TYPE="application/vnd.sufi.nupkg.bundle.v1+tar+gzip"

if [ "$#" -lt 2 ]; then
  usage
fi

command="$1"
shift

case "$command" in
  upload)
    [ "$#" -eq 3 ] || usage
    package_dir="$1"
    harbor_ref="$2"
    version="$3"
    bundle="nuget-${version}.tar.gz"

    if [ ! -d "$package_dir" ]; then
      echo "Package directory not found: $package_dir" >&2
      exit 1
    fi

    tar -czf "$bundle" -C "$package_dir" .
    oras push "$harbor_ref" "${bundle}:${MEDIA_TYPE}"
    rm -f "$bundle"
    echo "Uploaded bundle to $harbor_ref"
    ;;

  download)
    [ "$#" -eq 3 ] || usage
    harbor_ref="$1"
    version="$2"
    extract_dir="$3"
    bundle="nuget-${version}.tar.gz"

    mkdir -p "$extract_dir"
    oras pull "$harbor_ref" -o .
    if [ ! -f "$bundle" ]; then
      echo "Expected bundle file not found after pull: $bundle" >&2
      ls -la
      exit 1
    fi
    tar -xzf "$bundle" -C "$extract_dir"
    rm -f "$bundle"
    echo "Downloaded and extracted bundle from $harbor_ref to $extract_dir"
    ;;

  *)
    usage
    ;;
esac
