$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Artifacts = Join-Path $Root "nuget\artifacts"
Set-Location $Root

if (Test-Path $Artifacts) { Remove-Item "$Artifacts\*.nupkg" -Force -ErrorAction SilentlyContinue }
New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null

$Projects = @(
    "src\Stratum.Core\Stratum.Core.csproj",
    "src\Stratum.Runtime\Stratum.Runtime.csproj",
    "src\Stratum.Controls\Stratum.Controls.csproj",
    "src\Stratum.DSL\Stratum.DSL.csproj",
    "template\Stratum.Templates\Stratum.Templates.csproj"
)

foreach ($proj in $Projects) {
    Write-Host "Packing $proj ..."
    dotnet pack $proj -c Release -o $Artifacts --nologo
}

Write-Host ""
Write-Host "Packages in $Artifacts:"
Get-ChildItem $Artifacts -Filter "*.nupkg" | ForEach-Object { Write-Host "  $($_.Name)" }
Write-Host ""
Write-Host "To use locally:"
Write-Host ('  dotnet nuget add source "' + $Artifacts + '" --name StratumLocal')
Write-Host "  dotnet new install Stratum.Templates"
Write-Host "  dotnet new stratum-app -n MyApp"
