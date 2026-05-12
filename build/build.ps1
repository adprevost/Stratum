param([string]$Sample = "Counter")
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$Out = Join-Path $Root "dist/$Sample"
if (Test-Path $Out) { Remove-Item $Out -Recurse -Force }
New-Item -ItemType Directory -Force -Path $Out | Out-Null

Write-Host "Building Stratum libraries..."
dotnet build "src/Stratum.Core/Stratum.Core.csproj"           -c Release --nologo -v q
dotnet build "src/Stratum.Runtime/Stratum.Runtime.csproj"     -c Release --nologo -v q
dotnet build "src/Stratum.Controls/Stratum.Controls.csproj"   -c Release --nologo -v q
dotnet build "src/Stratum.DSL/Stratum.DSL.csproj"             -c Release --nologo -v q

# Stage loader files into the sample's wwwroot so publish picks them up.
$WwwRoot = Join-Path $Root "samples/$Sample/wwwroot"
New-Item -ItemType Directory -Force -Path $WwwRoot | Out-Null
(Get-Content "loader/Stratum.html") -replace '{{APP_NAME}}', $Sample |
    Set-Content (Join-Path $WwwRoot "index.html")
Copy-Item "loader/Stratum.js" $WwwRoot -Force
Copy-Item "loader/main.js"    $WwwRoot -Force

Write-Host "Publishing sample: $Sample..."
dotnet publish "samples/$Sample" -c Release -o $Out --nologo

# The WebAssembly SDK puts the actual served site under <out>/wwwroot.
# Flatten it so `dotnet serve -d dist/<Sample>` (or any static server) just works.
$Www = Join-Path $Out "wwwroot"
if (Test-Path $Www) {
    Write-Host "Flattening wwwroot/ into $Out ..."
    Get-ChildItem -Path $Www -Force | ForEach-Object {
        $dest = Join-Path $Out $_.Name
        if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
        Move-Item -LiteralPath $_.FullName -Destination $dest -Force
    }
    Remove-Item $Www -Recurse -Force
}

Write-Host ""
Write-Host "Done. Output in: $Out"
Write-Host "Serve with:  dotnet serve -d `"$Out`" -p 8080   ->  http://localhost:8080"
Write-Host "Or:          cd `"$Out`"; python -m http.server 8080"
