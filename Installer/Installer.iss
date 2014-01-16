#define AppName "Manga Crawler"
#define AppVersion "1.3"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
VersionInfoVersion={#AppVersion}
AppPublisherURL=http://mangacrawler.codeplex.com/
OutputBaseFilename={#AppName} {#AppVersion}
DefaultGroupName={#AppName}
DefaultDirName={pf}\{#AppName}
UninstallDisplayIcon={app}\MangaCrawler.exe
OutputDir=.
SourceDir=.
AllowNoIcons=yes
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
AppMutex=Manga Crawler 5324532532

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\Ionic.Zip.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\HtmlAgilityPack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\MangaCrawler.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\MangaCrawler.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\MangaCrawlerLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Programowanie\C#\Moje programy\MangaCrawler\MangaCrawler\bin\Release\TomanuExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\MangaCrawler.exe"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\MangaCrawler.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\MangaCrawler.exe"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]

const
  NET_KEY = 'Software\Microsoft\NET Framework Setup\NDP\v4\Full';
  
var 
  NETPageID: Integer;
  NETPageShown: Boolean;

function NETInstalled(): boolean;
var
  Locales: TArrayOfString;
  I: Integer;
  Installed: Cardinal;
begin
  if not RegGetSubkeyNames(HKLM, NET_KEY, Locales) then
  begin
    Result := False;
    Exit;
  end;

  for I:=0 to GetArrayLength(Locales) - 1 do
  begin
    Log(Locales[i]);
    Log(NET_KEY + '\' + Locales[I]);
    if RegQueryDWordValue(HKLM, NET_KEY + '\' + Locales[i], 'Install', Installed) then
    begin
      if Installed <> 0 then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;

  Result := False;

end;

procedure LinkClicked(Sender: TObject);
var 
  Link: String;
  ErrCode: Integer;
begin
   Link := TNewStaticText(Sender).Caption;
   ShellExec('open',  link, '', '', SW_SHOW, ewNoWait, ErrCode);
end;

procedure InitializeWizard;
var
  Page: TWizardPage;
  StaticText1, StaticText2, StaticText3, StaticText4, StaticText5: TNewStaticText;
begin
  if NETInstalled then
    Exit;
    
  Page := CreateCustomPage(wpWelcome, 'NET Framework 4.0 not installed', 'Please install first NET Framework 4.0');
  
  StaticText1 := TNewStaticText.Create(Page);
  StaticText1.Caption := 'NET Framework 4.0 web installer:';
  StaticText1.AutoSize := True;
  StaticText1.Parent := Page.Surface;
  
  StaticText2 := TNewStaticText.Create(Page);
  StaticText2.Top := StaticText1.Top + StaticText1.Height + ScaleY(8);
  StaticText2.Left := ScaleX(20);
  StaticText2.Caption := 'http://www.microsoft.com/download/en/details.aspx?id=17851';
  StaticText2.AutoSize := True;
  StaticText2.Parent := Page.Surface;
  StaticText2.Font.Color := clBlue;
  StaticText2.Font.Style := [fsUnderline];
  StaticText2.OnClick := @LinkClicked;
  StaticText2.Cursor := crHand;
  
  StaticText3 := TNewStaticText.Create(Page);
  StaticText3.Top := StaticText2.Top + StaticText2.Height + ScaleY(8);
  StaticText3.Caption := 'NET Framework 4.0 standalone installer:';
  StaticText3.AutoSize := True;
  StaticText3.Parent := Page.Surface;
  
  StaticText4 := TNewStaticText.Create(Page);
  StaticText4.Top := StaticText3.Top + StaticText3.Height + ScaleY(8);
  StaticText4.Left := ScaleX(20);
  StaticText4.Caption := 'http://www.microsoft.com/download/en/details.aspx?id=17718';
  StaticText4.AutoSize := True;
  StaticText4.Parent := Page.Surface;
  StaticText4.Font.Color := clBlue;
  StaticText4.Font.Style := [fsUnderline];
  StaticText4.OnClick := @LinkClicked;
  StaticText4.Cursor := crHand;
  
  StaticText5 := TNewStaticText.Create(Page);
  StaticText5.Top := StaticText4.Top + StaticText4.Height + ScaleY(8);
  StaticText5.Caption := 'Before downloading choose your language';
  StaticText5.AutoSize := True;
  StaticText5.Parent := Page.Surface;
  
  NETPageID := Page.ID;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = NETPageID then
  begin
    Wizardform.NextButton.Enabled := False;
    Wizardform.BackButton.Enabled := False;
    NETPageShown := True;
  end;
end;

procedure CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
  if (NETPageShown) then
  begin
    Cancel := true;
    Confirm := false;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  case CurUninstallStep of
    usUninstall:
      begin
        if MsgBox('Remove cached data and settings ?', mbInformation, MB_YESNO) = IDYES then
        begin
          DelTree(ExpandConstant('{userappdata}') + '\MangaCrawler\Catalog\*.xml.zip', False, True, False);
          DeleteFile(ExpandConstant('{userappdata}') + '\MangaCrawler\settings.xml');;
          RemoveDir(ExpandConstant('{userappdata}') + '\MangaCrawler\Catalog');
          RemoveDir(ExpandConstant('{userappdata}' + '\MangaCrawler'));
        end;
      end;
  end;
end;