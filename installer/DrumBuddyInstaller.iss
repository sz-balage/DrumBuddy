; If AppVersion/AppDir are not provided via /D on the command line,
; fall back to sensible defaults.
#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#ifndef AppDir
  #define AppDir "."
#endif

[Setup]
AppId={{B2C3C0B5-1234-4F0C-ABCD-123456789ABC}
AppName=DrumBuddy
AppVersion={#AppVersion}
AppPublisher=Your Name
DefaultDirName={pf}\DrumBuddy
DefaultGroupName=DrumBuddy
OutputDir=Output
OutputBaseFilename=DrumBuddy-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
; SetupIconFile=images\DrumBuddy.ico  ; optional, if you add an .ico later

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; AppDir is passed from CI as the publish\win-x64 folder
Source: "{#AppDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\DrumBuddy"; Filename: "{app}\DrumBuddy.Desktop.exe"
Name: "{commondesktop}\DrumBuddy"; Filename: "{app}\DrumBuddy.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"
