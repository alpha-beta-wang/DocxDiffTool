#!/bin/bash
set -e
cd "$(dirname "$0")/../../dotnet-app"

APP_NAME="DocxDiffTool"
APP_VERSION="1.0.0"
DIST_DIR="dist/macos"
APP_DIR="$DIST_DIR/${APP_NAME}.app"

echo "[1/3] Publishing for macOS..."
dotnet publish -c Release -r osx-x64 -o "$DIST_DIR/publish" --self-contained true -p:PublishSingleFile=true

echo "[2/3] Creating .app bundle..."
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

cp "$DIST_DIR/publish/${APP_NAME}" "$APP_DIR/Contents/MacOS/"
cp packaging/macos/Info.plist "$APP_DIR/Contents/"

echo "[3/3] Creating .dmg..."
hdiutil create -volname "${APP_NAME}" \
    -srcfolder "$APP_DIR" \
    -ov -format UDZO \
    "$DIST_DIR/${APP_NAME}_${APP_VERSION}.dmg"

echo "Done! DMG at $DIST_DIR/${APP_NAME}_${APP_VERSION}.dmg"
