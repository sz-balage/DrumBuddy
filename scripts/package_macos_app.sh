#!/bin/bash
set -e

# Usage: ./scripts/package_macos_app.sh osx-arm64
TARGET_OS="$1"

if [ -z "$TARGET_OS" ]; then
    echo "❌ Error: Missing target argument (expected osx-x64 or osx-arm64)"
    exit 1
fi

APP_NAME="DrumBuddy.app"
PUBLISH_OUTPUT_DIRECTORY="./publish/${TARGET_OS}"
INFO_PLIST="./Info.plist"
ICON_FILE="./images/DrumBuddy.icns"
APP_OUTPUT="./publish/${APP_NAME}"

echo "🍏 Packaging for ${TARGET_OS}..."

# Clean up old app
rm -rf "$APP_OUTPUT"

# Create app structure
mkdir -p "$APP_OUTPUT/Contents/MacOS"
mkdir -p "$APP_OUTPUT/Contents/Resources"

# Copy resources
cp "$INFO_PLIST" "$APP_OUTPUT/Contents/Info.plist"
cp "$ICON_FILE" "$APP_OUTPUT/Contents/Resources/DrumBuddy.icns"

# Copy published files into Contents/MacOS
cp -a "$PUBLISH_OUTPUT_DIRECTORY/." "$APP_OUTPUT/Contents/MacOS/"

echo "✅ Created ${APP_OUTPUT}"
