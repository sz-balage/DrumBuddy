#define AppVersion GetCustomOption("AppVersion", "0.0.0")
#define AppDir GetCustomOption("AppDir", ".")

[Setup]
AppId={{B2C3C0B5-1234-4F0C-ABCD-123456789ABC}
AppName=DrumBuddy
AppVersion={#AppVersion}
AppPublisher=Your Name
DefaultDirName={pf}\DrumBuddy
DefaultGroupName=DrumBuddy
OutputDir=installer\Output
OutputBaseFilename=DrumBuddy-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
; If you later add a .ico: SetupIconFile=images\DrumBuddy.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; DRUMBUDDY_BUILD_DIR points to publish\win-x64 in the workflow
Source: "{#AppDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\DrumBuddy"; Filename: "{app}\DrumBuddy.Desktop.exe"
Name: "{commondesktop}\DrumBuddy"; Filename: "{app}\DrumBuddy.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"
