#!/usr/bin/env bash
# Guard the open-source OTP / channels surface: no pro-modules, commercial-modules,
# or the private commercial OTP vendor name. Historical pro-module test hosts under
# other test/ trees are out of scope for #435 (see test/suficom-channels README).
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
fail=0
# Split so this script file itself does not contain the forbidden token.
vendor_pat='fa''nap'

scan_paths=(
  "${root}/modules/suficom-channels"
  "${root}/test/suficom-channels"
  "${root}/framework/SufiChain.SufiPlatform.SufiCom.Abstractions"
  "${root}/framework/SufiChain.SufiPlatform.CLI.Core/Modules"
)

echo "no-pro-references: scanning OSS OTP / channels surface under ${root}"

for path in "${scan_paths[@]}"; do
  if [[ ! -d "${path}" ]]; then
    echo "ERROR: expected path missing: ${path}" >&2
    fail=1
    continue
  fi

  while IFS= read -r -d '' file; do
    if grep -Eiq 'pro-modules|commercial-modules' "${file}"; then
      echo "ERROR: ${file} references pro-modules or commercial-modules" >&2
      fail=1
    fi
  done < <(find "${path}" \( -name '*.csproj' -o -name '*.props' -o -name '*.slnx' \) -print0 2>/dev/null)
done

# Private commercial OTP vendor must not appear anywhere in the public platform checkout.
while IFS= read -r -d '' file; do
  case "${file}" in
    */bin/*|*/obj/*|*/.git/*|*/checks/no-pro-references.sh) continue ;;
  esac
  if grep -Eiq "${vendor_pat}" "${file}"; then
    echo "ERROR: forbidden commercial OTP vendor reference in ${file}" >&2
    fail=1
  fi
done < <(find "${root}" -type f \
  ! -path '*/.git/*' ! -path '*/bin/*' ! -path '*/obj/*' \
  ! -path '*/node_modules/*' \
  -print0 2>/dev/null)

if [[ "${fail}" -ne 0 ]]; then
  echo "no-pro-references: FAILED" >&2
  exit 1
fi

echo "no-pro-references: OK"
exit 0
