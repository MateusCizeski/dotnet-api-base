#!/bin/bash

# =============================================================================
# publish.sh — Build, test and publish all ApiBase NuGet packages (manual fallback).
#
# Prefer the GitHub Actions release (push a tag v*): it runs the tests and uses
# Trusted Publishing, with no API key on your machine.
#
# Usage (the key comes from an environment variable, never from an argument,
# so it does not stay in the shell history):
#   read -s NUGET_API_KEY && export NUGET_API_KEY
#   ./publish.sh
#
# The version comes from Directory.Build.props (<Version>).
# =============================================================================

set -euo pipefail

NUGET_SOURCE="https://api.nuget.org/v3/index.json"

PACKAGES=(
  "ApiBase.Domain"
  "ApiBase.Infra"
  "ApiBase.Repository"
  "ApiBase.Application"
  "ApiBase.Controller"
)

if [ -z "${NUGET_API_KEY:-}" ]; then
  echo "Error: set the NUGET_API_KEY environment variable first (read -s NUGET_API_KEY && export NUGET_API_KEY)."
  exit 1
fi

echo "Cleaning previous artifacts..."
rm -rf ./nupkgs
mkdir ./nupkgs

echo "Restoring, building and testing in Release..."
dotnet restore ApiBase.Core.sln
dotnet build ApiBase.Core.sln -c Release --no-restore
dotnet test ApiBase.Core.sln -c Release --no-build

echo "Packing..."
for PACKAGE in "${PACKAGES[@]}"; do
  dotnet pack "$PACKAGE/$PACKAGE.csproj" -c Release --no-build -o ./nupkgs
done

echo "Publishing to NuGet..."
FAILED=0
for NUPKG in ./nupkgs/*.nupkg; do
  if dotnet nuget push "$NUPKG" --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE" --skip-duplicate; then
    echo "  published $(basename "$NUPKG")"
  else
    echo "  FAILED $(basename "$NUPKG")"
    FAILED=$((FAILED + 1))
  fi
done

if [ $FAILED -gt 0 ]; then
  exit 1
fi
