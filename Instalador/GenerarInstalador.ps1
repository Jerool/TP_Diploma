<#
    GenerarInstalador.ps1
    -----------------------------------------------------------------------
    Arma el instalador de TechFlow de punta a punta:
      1) Compila la solución en Release (con MSBuild de Visual Studio).
      2) Compila el instalador con Inno Setup (ISCC.exe).
      3) Deja el setup.exe en  Instalador\Output\TechFlow_Setup_GO44.exe

    Uso (desde PowerShell, parado en la carpeta del repo o en Instalador\):
        .\Instalador\GenerarInstalador.ps1

    Requisitos:
      - Visual Studio / MSBuild (ya instalado en esta PC).
      - Inno Setup 6  ->  https://jrsoftware.org/isdl.php
    -----------------------------------------------------------------------
#>

$ErrorActionPreference = 'Stop'

# Carpeta de este script y raíz del repo (un nivel arriba).
$here     = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $here
$sln      = Join-Path $repoRoot 'TP Diploma.slnx'
$iss      = Join-Path $here 'TechFlow_Setup_GO44.iss'

Write-Host '== 1/2  Compilando la solución en Release ==' -ForegroundColor Cyan

# Buscar MSBuild (VS 2022/2026, cualquier edición).
$msbuild = $null
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vswhere) {
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild `
                 -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $msbuild) {
    $msbuild = Get-ChildItem "C:\Program Files\Microsoft Visual Studio","C:\Program Files (x86)\Microsoft Visual Studio" `
                 -Recurse -Filter MSBuild.exe -ErrorAction SilentlyContinue |
                 Where-Object { $_.FullName -match '\\Bin\\MSBuild.exe$' } |
                 Select-Object -First 1 -ExpandProperty FullName
}
if (-not $msbuild) { throw 'No encontré MSBuild. Abrí la "Developer PowerShell for VS" o instalá Build Tools.' }

& $msbuild "$sln" /t:Build /p:Configuration=Release /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "Falló la compilación de la solución (código $LASTEXITCODE)." }

Write-Host '== 2/2  Compilando el instalador con Inno Setup ==' -ForegroundColor Cyan

# Buscar ISCC.exe (compilador de Inno Setup).
$iscc = $null
foreach ($p in @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe")) {
    if (Test-Path $p) { $iscc = $p; break }
}
if (-not $iscc) {
    $iscc = Get-ChildItem "C:\Program Files","C:\Program Files (x86)" -Recurse -Filter ISCC.exe `
              -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $iscc) { throw 'No encontré ISCC.exe. Instalá Inno Setup 6 desde https://jrsoftware.org/isdl.php' }

& $iscc "$iss"
if ($LASTEXITCODE -ne 0) { throw "Falló la compilación del instalador (código $LASTEXITCODE)." }

$salida = Join-Path $here 'Output\TechFlow_Setup_GO44.exe'
Write-Host ''
Write-Host "LISTO -> $salida" -ForegroundColor Green
