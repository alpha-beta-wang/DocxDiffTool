#!/bin/bash
set -e
cd "$(dirname "$0")/../../dotnet-app"

APP_NAME="DocxDiffTool"
DIST_DIR="dist/osx"
APP_DIR="$DIST_DIR/${APP_NAME}.app"
INSTALLER_DIR="dist/installer"

echo "[1/4] Publishing for macOS..."
dotnet publish -c Release -r osx-x64 -o "$DIST_DIR" --self-contained true -p:PublishSingleFile=true

echo "[2/4] Creating .app bundle..."
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"
cp "$DIST_DIR/${APP_NAME}" "$APP_DIR/Contents/MacOS/"
cp "$DIST_DIR/Photino.Native.dylib" "$APP_DIR/Contents/MacOS/" 2>/dev/null || true
cp "$(dirname "$0")/Info.plist" "$APP_DIR/Contents/"

echo "[3/4] Copying icon..."
cp "../../assets/logo.png" "$APP_DIR/Contents/Resources/appicon.png" 2>/dev/null || \
cp "../assets/logo.png" "$APP_DIR/Contents/Resources/appicon.png" 2>/dev/null || \
echo "Warning: logo.png not found, skipping icon"

# Clean up raw publish files
rm -f "$DIST_DIR/${APP_NAME}" "$DIST_DIR/Photino.Native.dylib" 2>/dev/null || true

# Create zip for distribution
mkdir -p "$INSTALLER_DIR"
echo "   Creating zip..."
(cd "$DIST_DIR" && zip -r "../../$INSTALLER_DIR/${APP_NAME}_macOS.zip" "${APP_NAME}.app")

echo "[4/4] Creating .dmg..."
if command -v hdiutil &>/dev/null; then
    hdiutil create -volname "${APP_NAME}" \
        -srcfolder "$APP_DIR" \
        -ov -format UDZO \
        "$INSTALLER_DIR/${APP_NAME}.dmg"
    echo "Done!"
    echo "  DMG: $INSTALLER_DIR/${APP_NAME}.dmg"
else
    echo "hdiutil not available (not on macOS), skipping .dmg."
fi
echo "  Zip: $INSTALLER_DIR/${APP_NAME}_macOS.zip"
echo "  .app at $APP_DIR"
