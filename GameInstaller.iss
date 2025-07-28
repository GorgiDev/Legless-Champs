[Setup]
AppName=Unlegged Robot
AppVersion=0.1
DefaultDirName={pf}\UnleggedRobot
DefaultGroupName=UnlegedRobot
OutputBaseFilename=UnleggedRobot_Installer
Compression=lzma
SolidCompression=yes

[Files]
; Your game files
Source: "Builds\Windows\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

; Visual C++ Redistributable installer
Source: "Dependencies\VC_redist.x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Run]
; Run the Visual C++ Redistributable installer silently (only if needed)
Filename: "{tmp}\VC_redist.x64.exe"; Parameters: "/install /quiet /norestart"; StatusMsg: "Installing prerequisites..."; Flags: waituntilterminated

; Run your game after install
Filename: "{app}\Unlegged Robot.exe"; Description: "Launch The Game"; Flags: nowait postinstall skipifsilent