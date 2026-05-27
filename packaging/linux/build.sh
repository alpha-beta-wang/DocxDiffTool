#!/bin/bash
set -e
cd "$(dirname "$0")/../../dotnet-app"

APP_NAME="DocxDiffTool"
DIST_DIR="dist/linux"
INSTALLER_DIR="dist/installer"

echo "[1/2] Publishing for Linux..."
dotnet publish -c Release -r linux-x64 -o "$DIST_DIR" --self-contained true -p:PublishSingleFile=true

echo "[2/2] Creating portable archive..."
mkdir -p "$INSTALLER_DIR"
tar -czf "$INSTALLER_DIR/${APP_NAME}_Linux_Portable.tar.gz" -C "$DIST_DIR" "${APP_NAME}" Photino.Native.so 2>/dev/null || \
tar -czf "$INSTALLER_DIR/${APP_NAME}_Linux_Portable.tar.gz" -C "$DIST_DIR" "${APP_NAME}"

echo "Done! Archive at $INSTALLER_DIR/${APP_NAME}_Linux_Portable.tar.gz"
echo "Linux requires: libwebkit2gtk-4.1-0 libgtk-3-dev"
