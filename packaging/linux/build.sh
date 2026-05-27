#!/bin/bash
set -e
cd "$(dirname "$0")/../../dotnet-app"

APP_NAME="DocxDiffTool"
DIST_DIR="dist/linux"

echo "[1/2] Publishing for Linux..."
dotnet publish -c Release -r linux-x64 -o "$DIST_DIR" --self-contained true -p:PublishSingleFile=true

echo "[2/2] Creating portable archive..."
mkdir -p "$DIST_DIR/installer"
tar -czf "$DIST_DIR/installer/${APP_NAME}_Portable.tar.gz" -C "$DIST_DIR" "${APP_NAME}" Photino.Native.so 2>/dev/null || \
tar -czf "$DIST_DIR/installer/${APP_NAME}_Portable.tar.gz" -C "$DIST_DIR" "${APP_NAME}"

echo "Done! Archive at $DIST_DIR/installer/${APP_NAME}_Portable.tar.gz"
echo "Linux requires: libwebkit2gtk-4.1-0 libgtk-3-dev"
