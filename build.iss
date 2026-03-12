[Setup]
AppName=我的专属屏保
AppVersion=1.0
DefaultDirName={userappdata}\MyScreensaver
OutputDir=Output
OutputBaseFilename=Install_Screensaver
Compression=lzma2/ultra64
SolidCompression=yes
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
DisableFinishedPage=yes
PrivilegesRequired=lowest

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Registry]
Root: HKCU; Subkey: "Control Panel\Desktop"; ValueType: string; ValueName: "SCRNSAVE.EXE"; ValueData: "{app}\MyScreensaver.scr"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Control Panel\Desktop"; ValueType: string; ValueName: "ScreenSaveActive"; ValueData: "1"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Control Panel\Desktop"; ValueType: string; ValueName: "ScreenSaveTimeOut"; ValueData: "300"; Flags: uninsdeletevalue

[Code]
const
  SPI_SETSCREENSAVEACTIVE = 17;
  SPIF_UPDATEINIFILE = 1;
  SPIF_SENDWININICHANGE = 2;

function SystemParametersInfo(uiAction: UINT; uiParam: UINT; pvParam: PChar; fWinIni: UINT): BOOL;
  external 'SystemParametersInfoA@user32.dll stdcall';

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 1, nil, SPIF_UPDATEINIFILE or SPIF_SENDWININICHANGE);
    MsgBox('屏保安装成功！已自动生效。', mbInformation, MB_OK);
  end;
end;
