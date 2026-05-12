#!/usr/bin/env bash
set -e

SAMPLE=${1:-"Counter"}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(dirname "$SCRIPT_DIR")"
OUT="$ROOT/dist/$SAMPLE"

cd "$ROOT"

echo "Building Stratum libraries..."
dotnet build "src/Stratum.Core/Stratum.Core.csproj"           -c Release --nologo -q
dotnet build "src/Stratum.Runtime/Stratum.Runtime.csproj"     -c Release --nologo -q
dotnet build "src/Stratum.Controls/Stratum.Controls.csproj"   -c Release --nologo -q
dotnet build "src/Stratum.DSL/Stratum.DSL.csproj"             -c Release --nologo -q

# Stage loader files into the sample's wwwroot
WWWROOT="$ROOT/samples/$SAMPLE/wwwroot"
mkdir -p "$WWWROOT"
sed "s/{{APP_NAME}}/$SAMPLE/g" loader/Stratum.html > "$WWWROOT/index.html"
cp loader/Stratum.js "$WWWROOT/Stratum.js"
cp loader/main.js    "$WWWROOT/main.js"

echo "Publishing sample: $SAMPLE..."
rm -rf "$OUT"
dotnet publish "samples/$SAMPLE" -c Release -o "$OUT" --nologo

# The WebAssembly SDK puts the actual served site under <out>/wwwroot.
# Flatten it so a static server pointed at $OUT just works.
if [ -d "$OUT/wwwroot" ]; then
    echo "Flattening wwwroot/ into $OUT ..."
    shopt -s dotglob
    mv "$OUT/wwwroot"/* "$OUT"/
    shopt -u dotglob
    rmdir "$OUT/wwwroot"
fi

echo ""
echo "Done. Output in: $OUT/"
echo "Serve with:  dotnet serve -d \"$OUT\" -p 8080   ->  http://localhost:8080"
echo "Or:          cd \"$OUT\" && python3 -m http.server 8080"
