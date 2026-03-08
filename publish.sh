#!/bin/bash

# =============================================================================
# publish.sh — Build and publish all ApiBase NuGet packages
#
# Usage:
#   ./publish.sh <api-key> [version]
#
# Examples:
#   ./publish.sh oy2abc...xyz
#   ./publish.sh oy2abc...xyz 2.1.0
#
# If version is provided, it updates all .csproj files before packing.
# =============================================================================

set -e

API_KEY=$1
VERSION=$2
NUGET_SOURCE="https://api.nuget.org/v3/index.json"

PACKAGES=(
  "ApiBase.Domain"
  "ApiBase.Infra"
  "ApiBase.Repository"
  "ApiBase.Application"
  "ApiBase.Controller"
)

# -------------------------
# Validation
# -------------------------

if [ -z "$API_KEY" ]; then
  echo "❌  Error: API key is required."
  echo ""
  echo "Usage: ./publish.sh <api-key> [version]"
  exit 1
fi

# -------------------------
# Optional: bump version
# -------------------------

if [ -n "$VERSION" ]; then
  echo "🔖  Bumping version to $VERSION in all .csproj files..."
  for PACKAGE in "${PACKAGES[@]}"; do
    CSPROJ="$PACKAGE/$PACKAGE.csproj"
    if [ -f "$CSPROJ" ]; then
      sed -i "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" "$CSPROJ"
      echo "    ✔ $CSPROJ updated"
    else
      echo "    ⚠  $CSPROJ not found, skipping"
    fi
  done
  echo ""
fi

# -------------------------
# Clean previous artifacts
# -------------------------

echo "🧹  Cleaning previous build artifacts..."
for PACKAGE in "${PACKAGES[@]}"; do
  rm -rf "$PACKAGE/bin" "$PACKAGE/obj"
done
rm -rf ./nupkgs
mkdir ./nupkgs
echo ""

# -------------------------
# Restore
# -------------------------

echo "🔄  Restoring solution..."
dotnet restore
echo ""

# -------------------------
# Build
# -------------------------

echo "🔨  Building solution in Release..."
dotnet build -c Release --no-restore
echo ""

# -------------------------
# Pack
# -------------------------

echo "📦  Packing all packages..."
for PACKAGE in "${PACKAGES[@]}"; do
  echo "    → $PACKAGE"
  dotnet pack "$PACKAGE/$PACKAGE.csproj" \
    -c Release \
    --no-build \
    -o ./nupkgs
done
echo ""

# -------------------------
# Publish
# -------------------------

echo "🚀  Publishing to NuGet..."
PUBLISHED=0
FAILED=0

for NUPKG in ./nupkgs/*.nupkg; do
  FILENAME=$(basename "$NUPKG")
  echo "    Pushing $FILENAME..."

  if dotnet nuget push "$NUPKG" \
      --api-key "$API_KEY" \
      --source "$NUGET_SOURCE" \
      --skip-duplicate; then
    echo "    ✔ $FILENAME published"
    PUBLISHED=$((PUBLISHED + 1))
  else
    echo "    ✖ $FILENAME failed"
    FAILED=$((FAILED + 1))
  fi
done

echo ""
echo "============================================="
echo "  Done: $PUBLISHED published, $FAILED failed"
echo "============================================="

if [ $FAILED -gt 0 ]; then
  exit 1
fi