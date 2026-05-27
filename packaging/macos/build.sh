#!/bin/bash
set -e
cd "$(dirname "$0")/../../dotnet-app"

APP_NAME="DocxDiffTool"
DIST_DIR="dist/osx"
APP_DIR="$DIST_DIR/${APP_NAME}.app"

echo "[1/3] Publishing for macOS..."
dotnet publish -c Release -r osx-x64 -o "$DIST_DIR" --self-contained true -p:PublishSingleFile=true

echo "[2/3] Creating .app bundle..."
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"
cp "$DIST_DIR/${APP_NAME}" "$APP_DIR/Contents/MacOS/"
cp "$DIST_DIR/Photino.Native.dylib" "$APP_DIR/Contents/MacOS/" 2>/dev/null || true
cp "$(dirname "$0")/Info.plist" "$APP_DIR/Contents/"

echo "[3/3] Creating .dmg..."
if command -v hdiutil &>/dev/null; then
    hdiutil create -volname "${APP_NAME}" \
        -srcfolder "$APP_DIR" \
        -ov -format UDZO \
        "$DIST_DIR/${APP_NAME}.dmg"
    echo "Done! DMG at $DIST_DIR/${APP_NAME}.dmg"
else
    echo "hdiutil not available (not on macOS), skipping .dmg."
    echo "Done! .app at $APP_DIR"
fi
