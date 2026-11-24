/* ============================================================================
   MIGRACIÓN — Patente "Maestros.ProductosCambios" (Control de cambios) y su
   asignación a los roles supervisores del negocio.

   Aplica SOLO sobre una base YA EXISTENTE, sin borrar nada. Idempotente
   (IF NOT EXISTS). Ejecutar contra [Gestion Usuario]. Mismo estilo que
   Migracion_UsuariosDemo_GO44.sql.

   Qué hace:
     1) Da de alta la patente "Maestros.ProductosCambios" si no existe
        (el Id lo asigna IDENTITY).
     2) Crea los roles supervisores "Supervisor de Ventas" y
        "Supervisor de Compras" si no existen (Id por IDENTITY).
     3) Asigna esa patente a esos dos roles en RolPatente si no la tienen.

   Nota: tras tocar RolPatente/Patente/Roles, el recálculo de los dígitos
   verificadores (DVH/DVV) lo hace la app al correr esta migración
   (InstaladorBD_GO44 -> DALIntegridad.RecalcularTabla).
   ============================================================================ */
USE [Gestion Usuario]
GO

/* 1) Alta de la patente de Control de cambios (si no existe). */
IF NOT EXISTS (SELECT 1 FROM dbo.Patente WHERE DataKey = N'Maestros.ProductosCambios')
    INSERT dbo.Patente (Nombre, DataKey)
    VALUES (N'Maestros - Bitácora de cambios de productos', N'Maestros.ProductosCambios')
GO

/* 2) Roles supervisores del negocio (si no existen). */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = N'Supervisor de Ventas')
    INSERT dbo.Roles (Nombre) VALUES (N'Supervisor de Ventas')
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = N'Supervisor de Compras')
    INSERT dbo.Roles (Nombre) VALUES (N'Supervisor de Compras')
GO

/* 3) Asignación de la patente a los dos roles supervisores (si no la tienen). */
DECLARE @idPat     int = (SELECT Id FROM dbo.Patente WHERE DataKey = N'Maestros.ProductosCambios');
DECLARE @idVentas  int = (SELECT Id FROM dbo.Roles   WHERE Nombre  = N'Supervisor de Ventas');
DECLARE @idCompras int = (SELECT Id FROM dbo.Roles   WHERE Nombre  = N'Supervisor de Compras');

IF @idPat IS NOT NULL AND @idVentas IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = @idVentas AND IdPatente = @idPat)
    INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (@idVentas, @idPat);

IF @idPat IS NOT NULL AND @idCompras IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = @idCompras AND IdPatente = @idPat)
    INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (@idCompras, @idPat);
GO

PRINT 'Migración ProductosCambios: patente y roles supervisores asegurados.';
GO
