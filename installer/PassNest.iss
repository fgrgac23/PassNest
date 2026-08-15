#define MyAppName "PassNest"
#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif
#define MyAppPublisher "Filip Grgac"
#define MyAppExeName "PassNest.exe"

[Setup]
AppId={{8F3A2B1C-4D5E-4F60-9A7B-1C2D3E4F5A6B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableDirPage=auto
UsePreviousAppDir=yes
PrivilegesRequired=lowest
OutputDir=..\installer-output
OutputBaseFilename=PassNest-Setup
SetupIconFile=..\Software\PassNest\Assets\PassNest-icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Kreiraj prečac na radnoj površini"; GroupDescription: "Dodatne opcije:"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Pokreni PassNest"; Flags: nowait postinstall skipifsilent
