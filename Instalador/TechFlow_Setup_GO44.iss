; ============================================================================
;  INSTALADOR DE TechFlow / ElectroPoint  (Trabajo de Diploma - GO44)
;  Hecho con Inno Setup 6.  Compilar con:  ISCC.exe "TechFlow_Setup_GO44.iss"
;
;  Qué hace este instalador:
;   1) Copia la aplicación ya compilada (Release) a Archivos de programa, con
;      todas sus DLLs, el script EsquemaCompleto.sql y la carpeta de idiomas,
;      de modo que la app corra SOLA, sin Visual Studio.
;   2) Muestra una pantalla propia para ELEGIR LA INSTANCIA de SQL Server donde
;      se va a crear la base "Gestion Usuario" (detecta LocalDB y SQL Server).
;   3) Después de copiar, llama a la app en modo "--setup-db" para fijar esa
;      instancia en el .exe.config y crear la base con EsquemaCompleto.sql.
;   4) Si detecta que la app YA está instalada, ofrece REINSTALAR / reparar.
;   5) Crea accesos directos y un desinstalador.
; ============================================================================

#define MyAppName        "TechFlow"
#define MyAppVersion     "1.0"
#define MyAppPublisher   "GO44 - Trabajo de Diploma"
#define MyAppExeName     "PROYECTO ING DE SOFTWARE.exe"
; GUID fijo: identifica la app para la detección de reinstalación.
#define MyAppId          "{B7E4B2A1-9C3D-4F58-A6E1-2D7F8C1A4E90}"
; Carpeta de salida del build Release (relativa a este .iss).
#define CarpetaRelease   "..\PROYECTO ING DE SOFTWARE\bin\Release"

[Setup]
AppId={{#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=Output
OutputBaseFilename=TechFlow_Setup_GO44
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; Instalación para toda la máquina (Archivos de programa) -> requiere admin.
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#CarpetaRelease}\{#MyAppExeName}";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#CarpetaRelease}\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#CarpetaRelease}\*.dll";                  DestDir: "{app}"; Flags: ignoreversion
Source: "{#CarpetaRelease}\EsquemaCompleto.sql";    DestDir: "{app}"; Flags: ignoreversion
Source: "{#CarpetaRelease}\Migracion_PatenteProductosCambios_GO44.sql"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#CarpetaRelease}\Idiomas\*";              DestDir: "{app}\Idiomas"; Flags: ignoreversion recursesubdirs createallsubdirs
; Nota: los .pdb (símbolos de depuración) NO se incluyen a propósito.

[Icons]
Name: "{group}\{#MyAppName}";              Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}";  Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}";        Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

; ============================================================================
[Code]
var
  PaginaInstancia: TWizardPage;
  ComboInstancia:  TNewComboBox;
  BackupARestaurar: String;   { ruta del .bak elegido para restaurar ('' = no restaurar) }

{ --- Detección de instancias de SQL instaladas en la PC ------------------- }
procedure DetectarInstancias(Lista: TStrings);
var
  Nombres: TArrayOfString;
  i: Integer;
  Equipo: String;
begin
  Lista.Clear;

  { LocalDB (el que trae Visual Studio) }
  if RegKeyExists(HKLM64, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions') then
    Lista.Add('(localdb)\MSSQLLocalDB');

  { Instancias de SQL Server / Express instaladas como servicio }
  if RegGetValueNames(HKLM64, 'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL', Nombres) then
  begin
    Equipo := GetComputerNameString;
    for i := 0 to GetArrayLength(Nombres) - 1 do
    begin
      if CompareText(Nombres[i], 'MSSQLSERVER') = 0 then
        Lista.Add(Equipo)              { instancia por defecto -> NOMBRE-PC }
      else
        Lista.Add('.\' + Nombres[i]);  { instancia con nombre -> .\SQLEXPRESS }
    end;
  end;

  { Si no se detectó nada, ofrecer las opciones más comunes para tipear/elegir }
  if Lista.Count = 0 then
  begin
    Lista.Add('(localdb)\MSSQLLocalDB');
    Lista.Add('.\SQLEXPRESS');
  end;
end;

{ --- Pantalla propia para elegir la instancia ----------------------------- }
procedure InitializeWizard();
var
  Lista: TStringList;
  Lbl: TNewStaticText;
begin
  PaginaInstancia := CreateCustomPage(wpSelectDir,
    'Instancia de SQL Server',
    'Elegí dónde se va a crear la base de datos "Gestion Usuario".');

  Lbl := TNewStaticText.Create(PaginaInstancia);
  Lbl.Parent := PaginaInstancia.Surface;
  Lbl.Top := ScaleY(4);
  Lbl.Width := PaginaInstancia.SurfaceWidth;
  Lbl.Height := ScaleY(46);
  Lbl.AutoSize := False;
  Lbl.WordWrap := True;
  Lbl.Caption := 'Seleccioná una instancia detectada o escribí la tuya.' + #13#10 +
                 'Ejemplos:  (localdb)\MSSQLLocalDB    .\SQLEXPRESS    NOMBRE-PC\SQLEXPRESS';

  ComboInstancia := TNewComboBox.Create(PaginaInstancia);
  ComboInstancia.Parent := PaginaInstancia.Surface;
  ComboInstancia.Top := Lbl.Top + Lbl.Height + ScaleY(8);
  ComboInstancia.Width := PaginaInstancia.SurfaceWidth;
  ComboInstancia.Style := csDropDown;   { permite elegir de la lista O escribir }

  Lista := TStringList.Create;
  try
    DetectarInstancias(Lista);
    ComboInstancia.Items.Assign(Lista);
    if ComboInstancia.Items.Count > 0 then
      ComboInstancia.ItemIndex := 0
    else
      ComboInstancia.Text := '(localdb)\MSSQLLocalDB';
  finally
    Lista.Free;
  end;
end;

{ Instancia elegida por el usuario (sin espacios sobrantes). }
function GetInstancia(Param: String): String;
begin
  Result := Trim(ComboInstancia.Text);
  if Result = '' then
    Result := '(localdb)\MSSQLLocalDB';
end;

{ No dejar avanzar si el combo quedó vacío. }
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if (PaginaInstancia <> nil) and (CurPageID = PaginaInstancia.ID) then
  begin
    if Trim(ComboInstancia.Text) = '' then
    begin
      MsgBox('Ingresá o elegí una instancia de SQL Server.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

{ --- Detección de instalación previa: ofrecer REINSTALAR ------------------ }
function HayInstalacionPrevia(): Boolean;
var
  Clave: String;
begin
  Clave := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1';
  Result := RegKeyExists(HKLM, Clave) or RegKeyExists(HKLM64, Clave) or
            RegKeyExists(HKCU, Clave);
end;

{ Devuelve la ruta del .bak más reciente en C:\ProgramData\TechFlow\Backups, o '' si no hay. }
function UltimoBackup(): String;
var
  Carpeta, MejorNombre: String;
  Rec: TFindRec;
begin
  Result := '';
  Carpeta := ExpandConstant('{commonappdata}\TechFlow\Backups');
  MejorNombre := '';
  if FindFirst(Carpeta + '\GestionUsuario_*.bak', Rec) then
  begin
    try
      repeat
        { el timestamp va en el nombre, así que el mayor alfabéticamente es el más nuevo }
        if Rec.Name > MejorNombre then
          MejorNombre := Rec.Name;
      until not FindNext(Rec);
    finally
      FindClose(Rec);
    end;
  end;
  if MejorNombre <> '' then
    Result := Carpeta + '\' + MejorNombre;
end;

function InitializeSetup(): Boolean;
var
  Bak: String;
begin
  Result := True;
  BackupARestaurar := '';
  if HayInstalacionPrevia() then
  begin
    if MsgBox('{#MyAppName} ya está instalado en esta computadora.' + #13#10 + #13#10 +
              '¿Querés REINSTALARLO / repararlo?' + #13#10 +
              'Se van a volver a copiar los archivos y vas a poder elegir la instancia ' +
              'de SQL otra vez. Los datos que ya tengas en la base NO se borran, y ' +
              'se hace un backup automático por las dudas.',
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;   { el usuario no quiso reinstalar -> se cancela }
      Exit;
    end;

    { Si hay algún backup previo, ofrecer RESTAURARLO. }
    Bak := UltimoBackup();
    if Bak <> '' then
    begin
      if MsgBox('Encontré un backup anterior de la base:' + #13#10 + #13#10 +
                Bak + #13#10 + #13#10 +
                '¿Querés RESTAURAR ese backup?' + #13#10 +
                'Sí = volver la base a ese estado.' + #13#10 +
                'No = dejar la base como está ahora (igual se hace un backup de seguridad).',
                mbConfirmation, MB_YESNO) = IDYES then
        BackupARestaurar := Bak;
    end;
  end;
end;

{ --- Después de copiar los archivos: configurar instancia y crear la base - }
procedure CurStepChanged(CurStep: TSetupStep);
var
  Exe, Instancia, LogPath: String;
  Codigo: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    Exe := ExpandConstant('{app}\{#MyAppExeName}');
    Instancia := GetInstancia('');
    LogPath := ExpandConstant('{app}\instalador_db.log');

    WizardForm.StatusLabel.Caption := 'Configurando la base de datos en ' + Instancia + ' ...';

    if Exec(Exe, '--setup-db "' + Instancia + '"', ExpandConstant('{app}'),
            SW_HIDE, ewWaitUntilTerminated, Codigo) then
    begin
      if Codigo <> 0 then
        MsgBox('La aplicación se instaló bien, pero la base de datos no se pudo crear ' +
               'automáticamente (código ' + IntToStr(Codigo) + ').' + #13#10 + #13#10 +
               'No es grave: se va a crear sola la primera vez que abras la aplicación.' + #13#10 +
               'Detalle técnico en:' + #13#10 + LogPath,
               mbInformation, MB_OK);
    end
    else
      MsgBox('No se pudo ejecutar la configuración de la base de datos.' + #13#10 +
             'Se creará automáticamente al abrir la aplicación por primera vez.',
             mbInformation, MB_OK);

    { Si el usuario eligió restaurar un backup, se hace DESPUÉS de configurar la base. }
    if BackupARestaurar <> '' then
    begin
      WizardForm.StatusLabel.Caption := 'Restaurando la base desde el backup ...';
      if Exec(Exe, '--restore-db "' + Instancia + '" "' + BackupARestaurar + '"',
              ExpandConstant('{app}'), SW_HIDE, ewWaitUntilTerminated, Codigo) then
      begin
        if Codigo <> 0 then
          MsgBox('No se pudo restaurar el backup (código ' + IntToStr(Codigo) + ').' + #13#10 +
                 'La base quedó en su estado actual. Detalle en:' + #13#10 + LogPath,
                 mbError, MB_OK)
        else
          MsgBox('La base se restauró desde el backup:' + #13#10 + BackupARestaurar,
                 mbInformation, MB_OK);
      end
      else
        MsgBox('No se pudo ejecutar la restauración de la base.', mbError, MB_OK);
    end;
  end;
end;
