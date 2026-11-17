# Instalador de TechFlow / ElectroPoint

Instalador de Windows (hecho con **Inno Setup 6**) que deja la aplicación lista
para ejecutarse **sola, sin Visual Studio**, en cualquier PC con Windows 10/11.

## Qué hace

1. **Copia la app compilada** (Release) a `Archivos de programa\TechFlow`, con todas
   sus DLLs, el script `EsquemaCompleto.sql` y la carpeta `Idiomas`.
2. **Deja elegir la instancia de SQL Server** en una pantalla propia del instalador.
   Detecta automáticamente LocalDB y las instancias de SQL Server / Express
   instaladas, y también permite escribir una a mano
   (`(localdb)\MSSQLLocalDB`, `.\SQLEXPRESS`, `NOMBRE-PC\SQLEXPRESS`, etc.).
3. **Configura y crea la base**: tras copiar, llama a la app en modo
   `--setup-db "<instancia>"`, que graba la instancia elegida en el `.exe.config`
   (para todos los usuarios de la PC) y crea la base `Gestion Usuario` con
   `EsquemaCompleto.sql` si todavía no existe.
4. **Reinstalador**: si detecta que la app ya está instalada, **pregunta si querés
   reinstalar / reparar** antes de seguir (sin borrar los datos de la base).
5. Crea accesos directos (menú Inicio y, opcional, escritorio) y un **desinstalador**.

> Si por algún motivo la base no se pudiera crear durante la instalación
> (p. ej. permisos de LocalDB), **no es problema**: la app la crea sola la primera
> vez que se abre, usando la misma instancia configurada. Queda un registro en
> `Archivos de programa\TechFlow\instalador_db.log`.

## Requisitos para GENERAR el instalador

- **Visual Studio / MSBuild** (ya instalado en esta PC).
- **Inno Setup 6** → descargar de <https://jrsoftware.org/isdl.php> e instalar.

## Cómo generar el instalador

Desde PowerShell, parado en la carpeta del repositorio:

```powershell
.\Instalador\GenerarInstalador.ps1
```

El script compila la solución en Release y después compila el instalador.
El resultado queda en:

```
Instalador\Output\TechFlow_Setup_GO44.exe
```

(También se puede compilar solo el instalador abriendo
`Instalador\TechFlow_Setup_GO44.iss` en el IDE de Inno Setup y presionando
**Build → Compile**, siempre que antes se haya compilado la app en Release.)

## Requisitos para USAR el instalador (en la PC destino)

- Windows 10/11 con **.NET Framework 4.7.2** (ya viene en Windows 10/11).
- Una instancia de **SQL Server** disponible: LocalDB, SQL Express o SQL Server
  completo. Si la PC no tiene ninguna, instalar SQL Server Express o LocalDB antes.
- Ejecutar el `TechFlow_Setup_GO44.exe` **como administrador** (lo pide solo, porque
  instala en Archivos de programa).

## Archivos de esta carpeta

| Archivo | Para qué sirve |
|---|---|
| `TechFlow_Setup_GO44.iss` | Script de Inno Setup (define todo el instalador). |
| `GenerarInstalador.ps1`   | Compila la app en Release y genera el `.exe` del instalador. |
| `Output\`                 | Carpeta donde queda el instalador generado. |
