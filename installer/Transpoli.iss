[Setup]
AppId={{A5C9E1C2-7B3E-4F3A-9C0D-7A1B2C3D4E5F}}
AppName=Transpoli
AppVersion=0.1.0
AppPublisher=Felipe Andrade
AppPublisherURL=https://github.com/felipeandrade08/TransPoli
DefaultDirName={localappdata}\Programs\Transpoli
DefaultGroupName=Transpoli
UninstallDisplayName=Transpoli
OutputDir=..\installer-output
OutputBaseFilename=Transpoli-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
Uninstallable=yes

[Files]
Source: "..\publish\Transpoli\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Transpoli"; Filename: "{app}\Transpoli.exe"
Name: "{autodesktop}\Transpoli"; Filename: "{app}\Transpoli.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos:"

[Run]
Filename: "{app}\Transpoli.exe"; Description: "Executar o Transpoli"; Flags: nowait postinstall skipifsilent
