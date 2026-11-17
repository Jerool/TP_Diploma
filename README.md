# TechFlow · ElectroPoint — Sistema de Gestión (TP Diploma)

Sistema de escritorio (WinForms / .NET Framework 4.7.2) para una casa de componentes de
hardware: ventas, compras, maestros, seguridad con roles y permisos, bitácora, control de
integridad y reportes.

Esta guía es para **ejecutar y probar** el sistema desde cero.

---

## 1. Requisitos

- **Windows** con **Visual Studio 2022** (o superior) con la carga de trabajo *Desarrollo de escritorio .NET*.
- **SQL Server** en cualquiera de sus formas:
  - LocalDB (viene con Visual Studio) → `(localdb)\MSSQLLocalDB`
  - SQL Server Express → `.\SQLEXPRESS`
  - Una instancia con nombre → `NOMBRE-PC\SQLEXPRESS`
- La base se llama **`Gestion Usuario`** y **se crea sola** la primera vez (ver paso 3).

---

## 2. Cambiar la cadena de conexión

Antes de ejecutar, indicá cuál es tu instancia de SQL en el archivo:

```
PROYECTO ING DE SOFTWARE/App.config
```

Buscá la sección `connectionStrings` y cambiá **solo el `Data Source`**:

```xml
<add name="GestionUsuario"
     connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Gestion Usuario;Integrated Security=True"
     providerName="System.Data.SqlClient" />
```

| Tu SQL es… | Poné en `Data Source=` |
|---|---|
| LocalDB (default de Visual Studio) | `(localdb)\MSSQLLocalDB` |
| SQL Server Express | `.\SQLEXPRESS` |
| Instancia con nombre | `NOMBRE-PC\SQLEXPRESS` |

> Dejá igual `Initial Catalog=Gestion Usuario` e `Integrated Security=True`.

---

## 3. Ejecutar el sistema

### Opción A — Automática (recomendada)

1. Abrí **`TP Diploma.slnx`** en Visual Studio.
2. Asegurate de que el proyecto de inicio sea **`PROYECTO ING DE SOFTWARE`** (clic derecho → *Establecer como proyecto de inicio*).
3. Presioná **F5**.

La primera vez, si la base **no existe**, la aplicación la **crea automáticamente** ejecutando
`EsquemaCompleto.sql` (tablas + procedimientos + datos de prueba + usuarios de demo). No hay
que hacer nada más.

### Opción B — Manual (correr el script en SSMS)

Si preferís crear la base a mano:

1. Abrí **SQL Server Management Studio (SSMS)** y conectate a tu instancia.
2. Abrí el archivo **`PROYECTO ING DE SOFTWARE/EsquemaCompleto.sql`**.
3. Ejecutá todo el script (**F5** en SSMS). Crea la base `Gestion Usuario` con todo adentro.
4. Volvé a Visual Studio y ejecutá la app con **F5**.

> El script es un **snapshot completo**: pensado para correr sobre una base **limpia**.

---

## 4. Credenciales de acceso

Se incluyen **usuarios de demo, uno por rol**, para probar el sistema de permisos.
**En todos, la contraseña es igual al nombre de usuario.**

| Usuario | Contraseña | Rol | Qué puede hacer |
|---|---|---|---|
| `admin` | `admin` | Admin | **Todo el sistema** (administración, maestros, ventas, compras, reportes) |
| `cajero` | `cajero` | Cajero | Ventas (cargar carrito, facturar, cobrar), clientes y reportes |
| `vendedor` | `vendedor` | Vendedor | Ventas (cargar carrito) y clientes |
| `almacenista` | `almacenista` | Almacenista | Compras → Recepción de mercadería |
| `compras` | `compras` | Encargado de Compras | Compras (proveedor, solicitud de cotización, orden de compra, pago) |

> Para ver **toda** la funcionalidad de una sola vez, entrá con **`admin` / `admin`**.
> Los demás usuarios muestran cómo el menú se **habilita/oculta según los permisos** del rol.

---

## 5. Cosas a tener en cuenta al probar

- **Bloqueo por intentos:** a los **3 intentos** de login fallidos (dentro de 1 hora) la cuenta se
  **bloquea**. Para desbloquearla, entrá como `admin` → *Admin → Gestión de usuarios → Desbloquear*
  (eso resetea la contraseña a `nombre + últimos 3 del DNI`).
- **Primer cambio de contraseña:** los usuarios de demo **no** están obligados a cambiarla al entrar.
- **Idioma:** se puede cambiar entre español e inglés desde el menú *Usuario → Idioma*.
- **Integridad y backups:** el sistema verifica dígitos verificadores al iniciar y hace backups
  automáticos en `C:\Backups\GestionUsuario`. En una instalación nueva, la primera corrida
  calcula la "línea base" y queda todo íntegro.
- **Envío de factura por email (opcional):** está **deshabilitado** por defecto. Para activarlo,
  seguí las instrucciones comentadas en `App.config` (sección de email). La app funciona igual sin esto.

---

## 6. Si necesitás solo los usuarios de demo (base ya creada)

Si ya tenías la base de una corrida anterior y querés **solo agregar** los usuarios de demo sin
reinstalar, corré en SSMS (sobre `Gestion Usuario`):

```
PROYECTO ING DE SOFTWARE/Migracion_UsuariosDemo_GO44.sql
```

Es idempotente (no duplica nada si ya existen).
