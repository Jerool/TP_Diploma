# TP Diploma — TechFlow / ElectroPoint

Sistema de compra y venta de componentes de hardware para la cátedra **Trabajo de Diploma** (UAI). Aplicación de escritorio **WinForms** con **.NET Framework 4.7.2** y **SQL Server**.

El autor es **Jeremias Facundo Gomez** (legajo B00107680-T1, comisión 3A). El sufijo convencional del proyecto es **`_GO44`** (todas las clases nuevas lo usan).

---

## 🎯 Qué hace la app

Sistema de gestión para una tienda de hardware que vende componentes (CPU, GPU, RAM, etc.) a clientes y compra stock a proveedores.

### Requisitos funcionales de negocio (RFN)

- **RFN1 — Ventas**: cargar carrito, facturar, cobrar (efectivo o tarjeta), imprimir/exportar factura, reporte de facturas.
- **RFN2 — Compras**: generar solicitudes de cotización (SC), registrar proveedores, generar órdenes de compra (OC), pagar OC, recibir mercadería.

### Roles del sistema

- **Administrador**: gestión de usuarios, roles, patentes, bitácora, backup, integridad.
- **Cajero**: cargar carrito y facturar / cobrar.
- **Encargado de Compras**: generar SC, registrar proveedores (RFN2).
- **Almacenista**: recepción de componentes (RFN2 — pendiente de implementar).

---

## 🏗️ Arquitectura por capas

```
┌─────────────────────────────────────────────────────────┐
│  UI (PROYECTO ING DE SOFTWARE)                          │
│  WinForms (FRM*.cs) · i18n ES/EN · validaciones UI      │
└───────────────────────┬─────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│  BLL                                                    │
│  Lógica de negocio · reglas · transacciones · bitácora  │
│  Clases: BLLCliente, BLLComponente, BLLCarrito,         │
│  BLLFactura, BLLCobro, BLLProveedor, BLLSolicitud...    │
└───────────────────────┬─────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│  DAL                                                    │
│  Acceso a datos vía Stored Procedures                   │
│  Clases: DALCliente, DALComponente, DALCarrito,         │
│  DALFactura, DALProveedor, DALSolicitud...              │
│  + Acceso (singleton de SqlConnection)                  │
│  + InstaladorBD (ejecuta EsquemaCompleto.sql)           │
└───────────────────────┬─────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│  BE (Entidades)                                         │
│  POCOs sin lógica · BE_Cliente, BE_Componente,          │
│  BE_SolicitudCotizacion, BE_LineaSC, BE_Proveedor...    │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  Servicios (transversal, usado por todas las capas)     │
│  · Validaciones_GO44 (regex: DNI, email, CUIT, CVU...)  │
│  · IdiomaManager_GO44 (Observer, i18n ES/EN JSON)       │
│  · SessionManager_GO44 (Singleton usuario logueado)     │
│  · EncriptadorReversible_GO44 (AES, emails cifrados)    │
│  · Encriptador_GO44 (hash SHA256 para contraseñas)      │
│  · BLLBitacora_GO44 (patron Singleton)                  │
│  · CalculadorIntegridad_GO44 (DVH / DVV)                │
│  · GeneradorPdf_GO44 (PDF nativo sin librerías)         │
│  · ConfiguracionBD_GO44 (lee/guarda conexion.cfg)       │
│  · DetectorInstancias_GO44 (detecta SQL instances)      │
└─────────────────────────────────────────────────────────┘
```

### Proyectos de VS

| Proyecto | Rol | Namespace |
|---|---|---|
| `BE/BE_GO44.csproj` | Entidades POCO | `BE` |
| `DAL/DAL_GO44.csproj` | Acceso a datos | `DAL` |
| `BLL/BLL_GO44.csproj` | Lógica de negocio | `BLL` |
| `Servicios/Servicios_GO44.csproj` | Utilidades transversales | `Servicios`, `Servicios.*` |
| `PROYECTO ING DE SOFTWARE/UI_GO44.csproj` | WinForms + Program.cs | `PROYECTO_ING_DE_SOFTWARE` |
| `PROYECTO_ING_DE_SOFTWARE_Setup/*.vdproj` | Installer MSI (opcional) | — |

Solución: `TP Diploma.slnx`.

---

## 📏 Convenciones del proyecto

### Nomenclatura

- **Sufijo `_GO44`** en todas las clases nuevas (BE_, DAL, BLL, FRM, Servicios).
- **Prefijos** por capa: `BE_`, `DAL`, `BLL`, `FRM`.
- Carpetas: cada proyecto tiene su carpeta homónima. Scripts SQL en `PROYECTO ING DE SOFTWARE/*.sql`.

### Patrones aplicados

- **Singleton**: `Acceso` (DAL), `SessionManager_GO44`, `BLLBitacora_GO44`, `IdiomaManager_GO44`, `EncriptadorReversible_GO44`.
- **Observer**: `IdiomaManager_GO44` notifica a `IObservadorIdioma_GO44` cuando cambia el idioma — todos los forms se re-renderizan.
- **Composite**: `Rol_GO44 → Familia_GO44 → Patente_GO44` (jerarquía de permisos).
- **Repository / DAO**: cada DAL* mapea 1-a-1 con una tabla y usa SPs.
- **Transaction Script**: BLLs orquestan múltiples llamadas DAL dentro de una `SqlTransaction`.

### Acceso a datos — reglas

1. **Toda consulta pasa por un SP**. No hay SQL inline (salvo integridad DVH/DVV y backups).
2. Los SPs usan sufijo `_GO44`.
3. **Nunca hacer `Acceso.Instancia.*` desde BLL** — siempre pasar por el DAL correspondiente (el DAL encapsula el `Acceso`).
   - Para transacciones: el DAL expone `IniciarTransaccion()`, `ConfirmarTransaccion(tx)`, `CancelarTransaccion(tx)` que son wrappers del `Acceso`.
4. **Patrón ABM atómico**: BLL inicia tx → DAL.InsertarEncabezadoEnTx → loop DAL.InsertarLineaEnTx → BLL confirma tx → BLL registra en bitácora.
5. **Métodos `*EnTx`**: cuando una operación forma parte de una transacción, hay una variante que acepta `SqlTransaction tx` como parámetro.

### Validaciones

- Delegadas a `Servicios.Validaciones_GO44`. Regex para DNI (8), teléfono (10), email, CUIT (11), CVU (22), contraseñas (6+ letra+número), login.
- UI aplica `MaxLength`, `KeyPress` filters, `AutoAdvance` entre campos (ej: CBU en 4+4+4+4+6).
- BLL re-valida antes de persistir (defensa en profundidad).

### Internacionalización (i18n)

- Archivos: `PROYECTO ING DE SOFTWARE/Idiomas/es.json` + `en.json`.
- Clase: `IdiomaManager_GO44.T("clave")`.
- Keys por scope: `menu.*`, `cliente.*`, `cobrar.*`, `facturar.*`, `carrito.*`, `productos.*`, `reporte.*`, `clientes.*`, `backup.*`, `bll.*`, `err.*`, `general.*`.
- Forms implementan `IObservadorIdioma_GO44` → se suscriben al `IdiomaManager_GO44.Instancia.Suscribir(this)` en el constructor y desuscriben en `FormClosed`.

### Bitácora (auditoría)

- `BLLBitacora_GO44.Instancia.RegistrarEvento(login, modulo, tipoEvento, detalle, criticidad)`.
- **Best-effort**: todos los métodos privados `Auditar()` en BLLs están envueltos en `try { ... } catch { }` — nunca rompen la operación principal.
- Criticidades: `Baja`, `Media`, `Alta`.
- Los `TipoEvento` deben existir previamente en la tabla `TipoEvento`. Si falta uno, el `catch` del `Auditar` lo tolera.

### Integridad (DVH / DVV)

- Dígito Verificador Horizontal (hash SHA256 por fila) y Vertical (hash SHA256 por tabla).
- Lo recalcula `BLLIntegridad_GO44.RecalcularTabla("NombreTabla")` después de insertar/modificar datos sensibles (clientes, componentes, facturas).
- **No aplica a SC/Proveedor** (datos no financieros críticos).

### Encriptación

- **Emails de clientes**: encriptados en BD con AES (`EncriptadorReversible_GO44`). Se desencriptan al mapear en el DAL.
- **Contraseñas**: hash SHA256 (`Encriptador_GO44.EncriptarContrasena`). Irreversible.

### Formularios WinForms

- Todos los forms se escriben con **InitializeComponent** generado a mano (sin el designer visual de VS), para que sea 100% reproducible desde código.
- Colores corporativos: azul oscuro `Color.FromArgb(13, 71, 161)`, fondo celeste claro `Color.FromArgb(227, 242, 253)`.
- Font: `Segoe UI 9F` body, `Segoe UI Semibold 14F Bold` títulos.
- Botones principales: azul + texto blanco + `FlatStyle.Flat`.
- Botones secundarios: `IndianRed` para cancelar/salir.

---

## 🗄️ Base de datos

- Nombre: **`Gestion Usuario`** (con espacio).
- 23 tablas, 37 Stored Procedures, 25 FKs.
- Script completo (snapshot): `PROYECTO ING DE SOFTWARE/EsquemaCompleto.sql` (~663 KB).
- Al arrancar, `Program.ConfigurarConexionBD()` → `InstaladorBD_GO44.AsegurarBaseDatos()`:
  - Si la BD no existe en la instancia configurada, ejecuta `EsquemaCompleto.sql` entero (tolerante a errores por batch).
  - Si existe, continúa sin hacer nada.
- **El SCRIPTS array del InstaladorBD solo contiene `EsquemaCompleto.sql`** — es un snapshot autosuficiente.

### Connection string

- Lee de `App.config` → clave `GestionUsuario`:
  ```xml
  <add name="GestionUsuario"
       connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Gestion Usuario;Integrated Security=True"
       providerName="System.Data.SqlClient" />
  ```
- Si no está en `App.config`, usa el fallback hardcodeado en `DAL/Acceso.cs → CONNECTION_STRING_DEFAULT`.
- **Cada compañero cambia solo `Data Source=X` en App.config con su instancia** (`.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`, etc.).

### Tablas principales

| Área | Tablas |
|---|---|
| Seguridad | `Usuario`, `Roles`, `Familia`, `Patente`, `RolFamilia`, `RolPatente`, `FamiliaPatente`, `FamiliaIntegrada` |
| Auditoría | `Modulo`, `TipoEvento`, `EVENTOS` |
| Integridad | `IntegridadDVH`, `IntegridadDVV` |
| Negocio | `Clientes`, `Componentes`, `Carrito`, `LineaCarrito` |
| Facturación | `Facturas`, `LineaFactura`, `CobrosFactura` |
| Compras RFN2 | `Proveedores`, `SolicitudesCotizacion`, `LineasSolicitudCotizacion` |

### Columna computada notable

- `SolicitudesCotizacion.NumeroSC` es `AS ('SC-' + RIGHT('000000' + CAST(Id AS VARCHAR(6)), 6)) PERSISTED` → genera formato `SC-000012`.

---

## 📝 Casos de uso implementados

### RFN1 Ventas (todos implementados)

- **CU01** Cargar carrito
- **CU02** Registrar cliente
- **CU03** Seleccionar componente (por id o código)
- **CU04** Generar factura
- **CU05** Cobrar venta (efectivo o tarjeta con validación Luhn + CVV)
- **CU06-RFN1** Reporte de facturas con filtros + exportar PDF
- **CU07-RFN1** Exportar / Importar XML de clientes

### RFN2 Compras

- ✅ **CU06** Generar solicitud de cotización (con preview + loop de líneas + transacción atómica)
- ✅ **CU07** Registrar proveedor (invocado como «extend» del CU06)
- ⏳ **CU08** Generar Orden de Compra (pendiente)
- ⏳ **CU09** Pagar OC (pendiente, con «include» en CU08)
- ⏳ **CU10** Recepción de componentes (pendiente, actor Almacenista)

### Administración

- ABM de Clientes, Productos (Componentes), Usuarios, Proveedores.
- Gestión de Permisos (patron Composite: Rol → Familia → Patente).
- Bitácora de eventos con filtros + exportar PDF.
- Backup/Restore manual + automático cada 3 hs (en `C:\Backups\GestionUsuario`).
- Integridad (recalcular + restore desde backup).

---

## 🚀 Cómo correr el proyecto

### Primera vez (PC nueva)

1. Instalar **Visual Studio** con .NET Framework 4.7.2.
2. Instalar **SQL Server** (LocalDB o Express — cualquiera sirve).
3. Clonar / descomprimir el repo.
4. Abrir `TP Diploma.slnx` en VS.
5. En `PROYECTO ING DE SOFTWARE/App.config`, verificar que el `Data Source=` coincida con la instancia local. Default: `(localdb)\MSSQLLocalDB`.
6. **Build → Rebuild Solution** → **F5**.
7. La primera vez, la app ejecuta `EsquemaCompleto.sql` sola → tarda ~10 segundos → crea la BD con datos de prueba.
8. Login con un usuario del snapshot (ej: `jeremias544` como admin).

### Comandos útiles

```bash
# Rebuild desde CLI (si usás MSBuild directo)
msbuild "TP Diploma.slnx" /t:Rebuild /p:Configuration=Debug

# Correr ejecutable
"./PROYECTO ING DE SOFTWARE/bin/Debug/PROYECTO ING DE SOFTWARE.exe"
```

### Problemas conocidos

- **"ExecuteScalar requires the command to have a transaction..."**: significa que un DAL dentro de una transacción está llamando al método NO transaccional. Usar la variante `*EnTx` que acepta `SqlTransaction tx`.
- **"Items collection cannot be modified when the DataSource property is set"**: no llamar `Items.Clear()` en un ComboBox que tiene `DataSource` seteado. En vez de eso: `cmbX.DataSource = null; cmbX.DataSource = listaNueva;`.
- **"Could not find stored procedure sp_..."**: la BD tiene un snapshot viejo. Hacer DROP DATABASE y F5 para que reinstale desde el `EsquemaCompleto.sql` actual.
- **"TipoEvento 'X' no existe en la tabla TipoEvento"**: el método `Auditar()` lo tolera (está en try/catch) pero conviene agregar el tipo al script o seedearlo en el BLL.

---

## 📂 Archivos de referencia clave

| Archivo | Para qué |
|---|---|
| `PROYECTO ING DE SOFTWARE/Program.cs` | Entry point, instalación de BD, startup |
| `DAL/Acceso.cs` | Singleton SqlConnection + connection string |
| `DAL/InstaladorBD_GO44.cs` | Ejecuta `EsquemaCompleto.sql` por batches |
| `PROYECTO ING DE SOFTWARE/EsquemaCompleto.sql` | Snapshot completo de BD (tablas + SPs + datos) |
| `Servicios/Validaciones_GO44.cs` | Regex + mensajes de validación |
| `Servicios/Observer/IdiomaManager_GO44.cs` | i18n |
| `BLL/BLLBitacora_GO44.cs` | Logging de auditoría |
| `PROYECTO ING DE SOFTWARE/FRMMenuPrincipal_GO44.cs` | Menú principal con permisos por rol |
| `DER_GestionUsuario_EA.js` | JScript para generar DER en Enterprise Architect |

---

## 💡 Reglas de oro al agregar código nuevo

1. **Nuevo caso de uso** = (al menos) nuevo BE + nuevo DAL + nuevo BLL + nuevo FRM + SPs en SQL + patente nueva + entrada en menú con permisos.
2. **Toda clase nueva lleva el sufijo `_GO44`**.
3. **BLL nunca toca `Acceso.Instancia`** directamente — siempre vía DAL.
4. **Toda operación de alta/modificación/baja** debe:
   - Validar con `Validaciones_GO44`.
   - Correr dentro de una transacción si involucra múltiples tablas.
   - Registrar un evento en bitácora.
   - (Opcional) Recalcular DVH/DVV si es un dato crítico.
5. **Nuevo FRM** debe:
   - Implementar `IObservadorIdioma_GO44`.
   - Suscribirse/desuscribirse al `IdiomaManager`.
   - Chequear permisos con `BLLPermisos_GO44.ObtenerArbolRol(rolId)` y mostrar/ocultar controles.
   - Usar `MessageBox` con textos de `IdiomaManager_GO44.T("...")`.
6. **Nueva tabla / SP** debe:
   - Entrar en `EsquemaCompleto.sql` (regenerar snapshot con SSMS "Script Database").
   - Si es FK a tabla existente, agregar la constraint `FK_*`.

---

## 🧪 Dependencias externas

- **Ninguna NuGet obligatoria** — todo con BCL de .NET Framework 4.7.2:
  - `System.Data.SqlClient` para SQL.
  - `System.Net.Mail` para envío de email (opcional, config SMTP en App.config).
  - `System.Xml.Serialization` para export/import XML.
  - `System.Configuration` para App.config.
  - `System.Security.Cryptography` para AES + SHA256.
- **No usamos ORMs** (ni EF, ni Dapper) — todo con `SqlCommand` + SPs.
- **No usamos iTextSharp ni similar** — el PDF se genera byte-a-byte en `BLL/GeneradorPdf_GO44.cs`.

---

## 📬 Email (opcional, deshabilitado por default)

- Si descomentás la sección `<system.net><mailSettings>` en `App.config` y configurás un Gmail con App Password, la app ofrece enviar la factura por email al cliente después de cobrar.
- Clase: `BLL/BLLEmailFactura_GO44.cs`.
- Si no está configurado, no aparece el popup — la facturación sigue andando normal.
