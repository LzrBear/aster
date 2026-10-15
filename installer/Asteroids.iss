; Asteroids – Inno Setup installer script
; Prerequisites:
;   1. Publish the game first:
;        dotnet publish Asteroids\Asteroids.csproj /p:PublishProfile=win-x64
;   2. Install Inno Setup 6 (https://jrsoftware.org/isdl.php)
;   3. Compile this script with Inno Setup Compiler (ISCC.exe) or the IDE

#define AppName    "Asteroids"
; AppVersion can be overridden from the command line: ISCC.exe /DAppVersion=1.2.3
#ifndef AppVersion
  #define AppVersion "1.0"
#endif
#define AppPublisher "Sameer Saini"
#define AppExeName "Asteroids.exe"
#define PublishDir "..\Asteroids\publish\win-x64"

[Setup]
AppId={{A124715F-D524-483E-87F9-B9CD77A93ADD}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisherURL=https://github.com/
AppSupportURL=https://github.com/
AppUpdatesURL=https://github.com/
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
LicenseFile=
OutputDir=output
OutputBaseFilename=Asteroids_Setup_{#AppVersion}
SetupIconFile=..\Asteroids\game.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";        Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
