#!/usr/bin/env bash
set -e

echo "📦 Publishing DrumBuddy for all platforms..."
dotnet publish DrumBuddy.Desktop -r linux-x64 -c Release -o ./publish/linux-x64
dotnet publish DrumBuddy.Desktop -r win-x64   -c Release -o ./publish/win-x64
dotnet publish DrumBuddy.Desktop -r osx-x64   -c Release -o ./publish/osx-x64
dotnet publish DrumBuddy.Desktop -r osx-arm64 -c Release -o ./publish/osx-arm64
echo "✅ All builds complete!"
