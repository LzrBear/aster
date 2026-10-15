; Asteroids – Inno Setup installer script
; Prerequisites:
;   1. Publish the game first:
;        dotnet publish Asteroids\Asteroids.csproj /p:PublishProfile=win-x64
;   2. Install Inno Setup 6 (https://jrsoftware.org/isdl.php)
;   3. Compile this script with Inno Setup Compiler (ISCC.exe) or the IDE
;
; The game requires the .NET 9.0 Desktop Runtime (x64).
; The installer checks for it and opens the download page if it is missing.

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

[Code]
// Check that .NET 9 Desktop Runtime (x64) is installed.
// The key written by the .NET installer under HKLM looks like:
//   SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\9.x.x
function IsDotNet9DesktopInstalled: Boolean;
var
  KeyPath: String;
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;
  KeyPath := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  if RegGetValueNames(HKLM64, KeyPath, Names) then
    for I := 0 to GetArrayLength(Names) - 1 do
      if Copy(Names[I], 1, 2) = '9.' then
      begin
        Result := True;
        Exit;
      end;
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if not IsDotNet9DesktopInstalled then
  begin
    if MsgBox(
      'Asteroids requires the .NET 9.0 Desktop Runtime (x64).' + #13#10 +
      'It does not appear to be installed.' + #13#10#13#10 +
      'Click OK to open the .NET download page, then re-run this installer.',
      mbInformation, MB_OKCANCEL) = IDOK then
      ShellExec('open', 'https://aka.ms/dotnet/9.0/windowsdesktop-runtime-win-x64.exe', '', '', SW_SHOW, ewNoWait);
    Result := False;
  end;
end;

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
