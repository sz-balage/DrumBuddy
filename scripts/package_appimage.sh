#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_NAME="DrumBuddy"
RID="linux-x64"

PUBLISH_DIR="${ROOT_DIR}/publish/${RID}"
APPDIR="${ROOT_DIR}/publish/${APP_NAME}.AppDir"
OUT_APPIMAGE="${ROOT_DIR}/publish/DrumBuddy-${RID}.AppImage"

ICON_SRC="${ROOT_DIR}/images/appicon.png"

echo "📦 Building AppImage from ${PUBLISH_DIR}"

# Clean AppDir
rm -rf "${APPDIR}"
mkdir -p "${APPDIR}/usr/bin"
mkdir -p "${APPDIR}/usr/share/applications"
mkdir -p "${APPDIR}/usr/share/icons/hicolor/256x256/apps"

# Copy published files
cp -r "${PUBLISH_DIR}/"** "${APPDIR}/usr/bin/" 2>/dev/null || true

# Copy icon (256x256 PNG)
if [[ -f "${ICON_SRC}" ]]; then
  cp "${ICON_SRC}" "${APPDIR}/usr/share/icons/hicolor/256x256/apps/${APP_NAME}.png"
else
  echo "⚠️ Icon ${ICON_SRC} not found, continuing without custom icon"
fi

# AppRun (launcher)
cat > "${APPDIR}/AppRun" << 'EOF'
#!/bin/sh
HERE="$(dirname "$(readlink -f "$0")")"
exec "$HERE/usr/bin/DrumBuddy.Desktop" "$@"
EOF
chmod +x "${APPDIR}/AppRun"

# .desktop file
cat > "${APPDIR}/${APP_NAME}.desktop" <<EOF
[Desktop Entry]
Name=DrumBuddy
Exec=DrumBuddy.Desktop
Icon=${APP_NAME}
Type=Application
Categories=Audio;Music;
EOF

# appimagetool
cd "${ROOT_DIR}/publish"
if [ ! -f "appimagetool" ]; then
  echo "⬇️ Downloading appimagetool"
  wget -q https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage -O appimagetool
  chmod +x appimagetool
fi

echo "🧰 Building AppImage -> ${OUT_APPIMAGE}"
APPIMAGE_EXTRACT_AND_RUN=1 ./appimagetool "${APP_NAME}.AppDir" "${OUT_APPIMAGE}"
echo "✅ AppImage created at ${OUT_APPIMAGE}"
