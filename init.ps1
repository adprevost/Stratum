#!/usr/bin/env pwsh
# init.ps1 — One-shot bootstrap for a Stratum dev environment.
#
# Installs required workloads, the dotnet-serve tool, packs Stratum NuGets
# locally, registers a local feed, installs the project template, and (optionally)
# scaffolds a fresh Stratum app.
#
# Usage:
#   ./init.ps1                  # set up the dev environment only
#   ./init.ps1 -AppName MyApp   # also create a sample app at ./MyApp

[CmdletBinding()]
param(
    [string]$AppName = ""
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
Set-Location $Root

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

Write-Step "Verifying .NET 10 SDK..."
$sdk = (dotnet --version) 2>$null
if (-not $sdk -or [Version]($sdk.Split('-')[0]) -lt [Version]"10.0.0") {
    throw ".NET 10 SDK required (found '$sdk'). Install from https://dotnet.microsoft.com/download/dotnet/10.0"
}
Write-Host "    .NET SDK $sdk OK"

Write-Step "Installing wasm workloads (may require sudo / admin)..."
dotnet workload install wasm-tools wasm-experimental --skip-sign-check

Write-Step "Installing dotnet-serve global tool..."
dotnet tool update -g dotnet-serve 2>$null
if ($LASTEXITCODE -ne 0) { dotnet tool install -g dotnet-serve }

Write-Step "Building Stratum and packing local NuGets..."
& "$Root/nuget/pack.ps1"

$Artifacts = Join-Path $Root "nuget/artifacts"
Write-Step "Registering local NuGet feed: $Artifacts"
$existing = (dotnet nuget list source) -match "StratumLocal"
if (-not $existing) {
    dotnet nuget add source $Artifacts --name StratumLocal
} else {
    Write-Host "    StratumLocal source already registered"
}

Write-Step "Installing the stratum-app project template..."
dotnet new install Stratum.Templates --force

if ($AppName) {
    Write-Step "Scaffolding new app: $AppName"
    if (Test-Path $AppName) {
        throw "Folder '$AppName' already exists. Pick another name or delete it first."
    }
    dotnet new stratum-app -n $AppName
    Write-Host ""
    Write-Host "Done. Try it:" -ForegroundColor Green
    Write-Host "  cd $AppName"
    Write-Host "  dotnet publish -o dist"
    Write-Host "  dotnet serve -d dist -p 8080"
} else {
    Write-Host ""
    Write-Host "Stratum dev environment ready." -ForegroundColor Green
    Write-Host "Create a new app with:  dotnet new stratum-app -n MyApp"
    Write-Host "Or run a sample with:   ./build/build.ps1 Counter"
}
