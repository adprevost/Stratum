#!/usr/bin/env bash
# init.sh — One-shot bootstrap for a Stratum dev environment.
#
# Installs required workloads, the dotnet-serve tool, packs Stratum NuGets
# locally, registers a local feed, installs the project template, and (optionally)
# scaffolds a fresh Stratum app.
#
# Usage:
#   ./init.sh                  # set up the dev environment only
#   ./init.sh MyApp            # also create a sample app at ./MyApp

set -e
APP_NAME="${1:-}"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

step() { printf '\n==> %s\n' "$1"; }

step "Verifying .NET 10 SDK..."
SDK="$(dotnet --version 2>/dev/null || true)"
if [ -z "$SDK" ]; then
    echo ".NET SDK not found. Install .NET 10 from https://dotnet.microsoft.com/download/dotnet/10.0" >&2
    exit 1
fi
MAJOR="${SDK%%.*}"
if [ "$MAJOR" -lt 10 ]; then
    echo ".NET 10 SDK required (found $SDK)." >&2
    exit 1
fi
echo "    .NET SDK $SDK OK"

step "Installing wasm workloads (may require sudo)..."
dotnet workload install wasm-tools wasm-experimental --skip-sign-check

step "Installing dotnet-serve global tool..."
if ! dotnet tool update -g dotnet-serve >/dev/null 2>&1; then
    dotnet tool install -g dotnet-serve
fi

step "Building Stratum and packing local NuGets..."
# nuget/pack.ps1 is PowerShell; run it via pwsh if available, otherwise use dotnet pack directly.
if command -v pwsh >/dev/null 2>&1; then
    pwsh "$ROOT/nuget/pack.ps1"
else
    ARTIFACTS="$ROOT/nuget/artifacts"
    mkdir -p "$ARTIFACTS"
    rm -f "$ARTIFACTS"/*.nupkg
    for proj in src/Stratum.Core src/Stratum.Runtime src/Stratum.Controls src/Stratum.DSL template/Stratum.Templates; do
        echo "Packing $proj ..."
        dotnet pack "$proj" -c Release -o "$ARTIFACTS" --nologo
    done
fi

ARTIFACTS="$ROOT/nuget/artifacts"
step "Registering local NuGet feed: $ARTIFACTS"
if dotnet nuget list source | grep -q "StratumLocal"; then
    echo "    StratumLocal source already registered"
else
    dotnet nuget add source "$ARTIFACTS" --name StratumLocal
fi

step "Installing the stratum-app project template..."
dotnet new install Stratum.Templates --force

if [ -n "$APP_NAME" ]; then
    step "Scaffolding new app: $APP_NAME"
    if [ -e "$APP_NAME" ]; then
        echo "Folder '$APP_NAME' already exists. Pick another name or delete it first." >&2
        exit 1
    fi
    dotnet new stratum-app -n "$APP_NAME"
    echo ""
    echo "Done. Try it:"
    echo "  cd $APP_NAME"
    echo "  dotnet publish -o dist"
    echo "  dotnet serve -d dist -p 8080"
else
    echo ""
    echo "Stratum dev environment ready."
    echo "Create a new app with:  dotnet new stratum-app -n MyApp"
    echo "Or run a sample with:   ./build/build.sh Counter"
fi
