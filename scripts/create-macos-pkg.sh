#!/usr/bin/env bash

set -euo pipefail

usage() {
  echo "Usage: $0 <publish-directory> <version> <output.pkg>" >&2
  echo "  publish-directory must contain the published git-flow executable." >&2
}

if [[ $# -ne 3 ]]; then
  usage
  exit 2
fi

publish_directory=$1
version=${2#v}
output_package=$3

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "Error: macOS installer packages must be built on macOS." >&2
  exit 1
fi

if ! command -v pkgbuild >/dev/null 2>&1; then
  echo "Error: pkgbuild is not available. Install the Xcode command line tools." >&2
  exit 1
fi

executable="$publish_directory/git-flow"
if [[ ! -f "$executable" || ! -x "$executable" ]]; then
  echo "Error: executable '$executable' was not found or is not executable." >&2
  exit 1
fi

if [[ ! "$version" =~ ^[0-9]+([.][0-9]+){1,3}([.-][A-Za-z0-9.-]+)?$ ]]; then
  echo "Error: version '$2' must be a numeric version, optionally prefixed with 'v'." >&2
  exit 2
fi

output_directory=$(dirname "$output_package")
mkdir -p "$output_directory"
output_package="$output_directory/$(basename "$output_package")"

staging_directory=$(mktemp -d "${TMPDIR:-/tmp}/karls-gitflow-pkg.XXXXXX")
trap 'rm -rf "$staging_directory"' EXIT

mkdir -p "$staging_directory/usr/local/bin"
install -m 755 "$executable" "$staging_directory/usr/local/bin/git-flow"

pkgbuild \
  --root "$staging_directory" \
  --identifier com.karl-sjogren.karls-gitflow \
  --version "$version" \
  --install-location / \
  "$output_package"

echo "Created macOS installer package: $output_package"
