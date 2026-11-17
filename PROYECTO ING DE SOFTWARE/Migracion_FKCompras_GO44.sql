/* ============================================================================
   MIGRACIÓN — Normalización de usuarios en Compras + FK de historial.
   Aplica SOLO sobre una base YA EXISTENTE (no borra datos). Idempotente.
   Ejecutar contra [Gestion Usuario].

   Qué hace:
   1) Renombra las columnas "Login*" de las tablas de compras a "UserName*"
      y las convierte al mismo tipo que Usuario.UserName: NVARCHAR(250).
         SolicitudesCotizacion.LoginEncargado   -> UserNameEncargado
         OrdenesCompra.LoginEncargado           -> UserNameEncargado
         PagosOrdenCompra.LoginEncargado        -> UserNameEncargado
         RecepcionesMercaderia.LoginAlmacenista -> UserNameAlmacenista
   2) Crea las FK hacia Usuario(UserName): FK_SC_Usuario, FK_OC_Usuario,
      FK_POC_Usuario, FK_REC_Usuario (igual que Carrito y Facturas).
   3) Crea la FK que faltaba: Componentes_C.IdComponente -> Componentes(Id)
      (FK_ComponentesC_Componentes).
   4) Recrea los stored procedures afectados para que usen los nombres nuevos.

   Requisito: todos los valores actuales de esas columnas deben existir como
   UserName en dbo.Usuario (si no, la FK no se puede validar). El bloque final
   avisa si hay valores huérfanos antes de crear las FK.
   ============================================================================ */
USE [Gestion Usuario]
GO
SET NOCOUNT ON;
GO

/* ---------- 1) Renombrar columnas (solo si todavía tienen el nombre viejo) ---------- */
IF COL_LENGTH('dbo.SolicitudesCotizacion', 'LoginEncargado') IS NOT NULL
    EXEC sp_rename 'dbo.SolicitudesCotizacion.LoginEncargado', 'UserNameEncargado', 'COLUMN';
IF COL_LENGTH('dbo.OrdenesCompra', 'LoginEncargado') IS NOT NULL
    EXEC sp_rename 'dbo.OrdenesCompra.LoginEncargado', 'UserNameEncargado', 'COLUMN';
IF COL_LENGTH('dbo.PagosOrdenCompra', 'LoginEncargado') IS NOT NULL
    EXEC sp_rename 'dbo.PagosOrdenCompra.LoginEncargado', 'UserNameEncargado', 'COLUMN';
IF COL_LENGTH('dbo.RecepcionesMercaderia', 'LoginAlmacenista') IS NOT NULL
    EXEC sp_rename 'dbo.RecepcionesMercaderia.LoginAlmacenista', 'UserNameAlmacenista', 'COLUMN';
GO

/* ---------- 2) Unificar el tipo al de Usuario.UserName: NVARCHAR(250) NOT NULL ---------- */
ALTER TABLE dbo.SolicitudesCotizacion  ALTER COLUMN UserNameEncargado   NVARCHAR(250) NOT NULL;
ALTER TABLE dbo.OrdenesCompra          ALTER COLUMN UserNameEncargado   NVARCHAR(250) NOT NULL;
ALTER TABLE dbo.PagosOrdenCompra       ALTER COLUMN UserNameEncargado   NVARCHAR(250) NOT NULL;
ALTER TABLE dbo.RecepcionesMercaderia  ALTER COLUMN UserNameAlmacenista NVARCHAR(250) NOT NULL;
GO

/* ---------- 3) Chequeo de huérfanos (informativo; no corta la ejecución) ---------- */
IF EXISTS (SELECT 1 FROM dbo.SolicitudesCotizacion S LEFT JOIN dbo.Usuario U ON U.UserName = S.UserNameEncargado WHERE U.UserName IS NULL)
    PRINT 'AVISO: SolicitudesCotizacion tiene UserNameEncargado sin usuario en dbo.Usuario; FK_SC_Usuario podría fallar.';
IF EXISTS (SELECT 1 FROM dbo.OrdenesCompra O LEFT JOIN dbo.Usuario U ON U.UserName = O.UserNameEncargado WHERE U.UserName IS NULL)
    PRINT 'AVISO: OrdenesCompra tiene UserNameEncargado sin usuario en dbo.Usuario; FK_OC_Usuario podría fallar.';
IF EXISTS (SELECT 1 FROM dbo.PagosOrdenCompra P LEFT JOIN dbo.Usuario U ON U.UserName = P.UserNameEncargado WHERE U.UserName IS NULL)
    PRINT 'AVISO: PagosOrdenCompra tiene UserNameEncargado sin usuario en dbo.Usuario; FK_POC_Usuario podría fallar.';
IF EXISTS (SELECT 1 FROM dbo.RecepcionesMercaderia R LEFT JOIN dbo.Usuario U ON U.UserName = R.UserNameAlmacenista WHERE U.UserName IS NULL)
    PRINT 'AVISO: RecepcionesMercaderia tiene UserNameAlmacenista sin usuario en dbo.Usuario; FK_REC_Usuario podría fallar.';
IF EXISTS (SELECT 1 FROM dbo.Componentes_C C LEFT JOIN dbo.Componentes P ON P.Id = C.IdComponente WHERE P.Id IS NULL)
    PRINT 'AVISO: Componentes_C tiene IdComponente sin producto en dbo.Componentes; FK_ComponentesC_Componentes podría fallar.';
GO

/* ---------- 4) FK hacia Usuario(UserName) ---------- */
IF OBJECT_ID('dbo.FK_SC_Usuario', 'F') IS NULL
    ALTER TABLE dbo.SolicitudesCotizacion WITH CHECK ADD CONSTRAINT FK_SC_Usuario
        FOREIGN KEY (UserNameEncargado) REFERENCES dbo.Usuario (UserName);
IF OBJECT_ID('dbo.FK_OC_Usuario', 'F') IS NULL
    ALTER TABLE dbo.OrdenesCompra WITH CHECK ADD CONSTRAINT FK_OC_Usuario
        FOREIGN KEY (UserNameEncargado) REFERENCES dbo.Usuario (UserName);
IF OBJECT_ID('dbo.FK_POC_Usuario', 'F') IS NULL
    ALTER TABLE dbo.PagosOrdenCompra WITH CHECK ADD CONSTRAINT FK_POC_Usuario
        FOREIGN KEY (UserNameEncargado) REFERENCES dbo.Usuario (UserName);
IF OBJECT_ID('dbo.FK_REC_Usuario', 'F') IS NULL
    ALTER TABLE dbo.RecepcionesMercaderia WITH CHECK ADD CONSTRAINT FK_REC_Usuario
        FOREIGN KEY (UserNameAlmacenista) REFERENCES dbo.Usuario (UserName);
GO

/* ---------- 5) FK del historial de componentes ---------- */
IF OBJECT_ID('dbo.FK_ComponentesC_Componentes', 'F') IS NULL
    ALTER TABLE dbo.Componentes_C WITH CHECK ADD CONSTRAINT FK_ComponentesC_Componentes
        FOREIGN KEY (IdComponente) REFERENCES dbo.Componentes (Id);
GO

/* ---------- 6) Recrear los stored procedures afectados (nombres nuevos) ---------- */
IF OBJECT_ID('dbo.sp_SolicitudCotizacion_BuscarPorId_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_SolicitudCotizacion_BuscarPorId_GO44]
GO
CREATE PROCEDURE [dbo].[sp_SolicitudCotizacion_BuscarPorId_GO44]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT S.Id, S.NumeroSC, S.CuitProveedor, P.Empresa AS EmpresaProveedor,
           S.UserNameEncargado, S.FechaEmision, S.Estado
    FROM dbo.SolicitudesCotizacion S
    INNER JOIN dbo.Proveedores P ON P.CUIT = S.CuitProveedor
    WHERE S.Id = @Id;
END
GO
IF OBJECT_ID('dbo.sp_SolicitudCotizacion_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_SolicitudCotizacion_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_SolicitudCotizacion_Insertar_GO44]
    @CuitProveedor     VARCHAR(11),
    @UserNameEncargado NVARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.SolicitudesCotizacion (CuitProveedor, UserNameEncargado, FechaEmision, Estado)
    VALUES (@CuitProveedor, @UserNameEncargado, GETDATE(), 'Pendiente');
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO
IF OBJECT_ID('dbo.sp_SolicitudCotizacion_Listar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_SolicitudCotizacion_Listar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_SolicitudCotizacion_Listar_GO44]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT S.Id, S.NumeroSC, S.CuitProveedor, P.Empresa AS EmpresaProveedor,
           S.UserNameEncargado, S.FechaEmision, S.Estado
    FROM dbo.SolicitudesCotizacion S
    INNER JOIN dbo.Proveedores P ON P.CUIT = S.CuitProveedor
    ORDER BY S.FechaEmision DESC;
END
GO
IF OBJECT_ID('dbo.sp_OrdenCompra_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_Insertar_GO44]
    @IdSolicitud       INT,
    @CuitProveedor     VARCHAR(11),
    @UserNameEncargado NVARCHAR(250),
    @FechaEntrega      DATETIME,
    @Total             DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.OrdenesCompra (IdSolicitud, CuitProveedor, UserNameEncargado, FechaEmision, FechaEntrega, Total, Estado)
    VALUES (@IdSolicitud, @CuitProveedor, @UserNameEncargado, GETDATE(), @FechaEntrega, @Total, 'Pendiente');
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO
IF OBJECT_ID('dbo.sp_OrdenCompra_Listar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_Listar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_Listar_GO44]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT O.Id, O.NumeroOC, O.IdSolicitud, S.NumeroSC, O.CuitProveedor, P.Empresa AS EmpresaProveedor,
           O.UserNameEncargado, O.FechaEmision, O.FechaEntrega, O.Total, O.Estado
    FROM dbo.OrdenesCompra O
    INNER JOIN dbo.Proveedores P ON P.CUIT = O.CuitProveedor
    LEFT JOIN dbo.SolicitudesCotizacion S ON S.Id = O.IdSolicitud
    ORDER BY O.FechaEmision DESC;
END
GO
IF OBJECT_ID('dbo.sp_OrdenCompra_ListarPorEstado_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_ListarPorEstado_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_ListarPorEstado_GO44]
    @Estado VARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT O.Id, O.NumeroOC, O.IdSolicitud, S.NumeroSC, O.CuitProveedor, P.Empresa AS EmpresaProveedor,
           O.UserNameEncargado, O.FechaEmision, O.FechaEntrega, O.Total, O.Estado
    FROM dbo.OrdenesCompra O
    INNER JOIN dbo.Proveedores P ON P.CUIT = O.CuitProveedor
    LEFT JOIN dbo.SolicitudesCotizacion S ON S.Id = O.IdSolicitud
    WHERE O.Estado = @Estado
    ORDER BY O.FechaEmision DESC;
END
GO
IF OBJECT_ID('dbo.sp_OrdenCompra_BuscarPorId_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_BuscarPorId_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_BuscarPorId_GO44]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT O.Id, O.NumeroOC, O.IdSolicitud, S.NumeroSC, O.CuitProveedor, P.Empresa AS EmpresaProveedor,
           O.UserNameEncargado, O.FechaEmision, O.FechaEntrega, O.Total, O.Estado
    FROM dbo.OrdenesCompra O
    INNER JOIN dbo.Proveedores P ON P.CUIT = O.CuitProveedor
    LEFT JOIN dbo.SolicitudesCotizacion S ON S.Id = O.IdSolicitud
    WHERE O.Id = @Id;
END
GO
IF OBJECT_ID('dbo.sp_PagoOrdenCompra_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_PagoOrdenCompra_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_PagoOrdenCompra_Insertar_GO44]
    @IdOrdenCompra     INT,
    @CVU               VARCHAR(22),
    @Banco             VARCHAR(80),
    @Titular           VARCHAR(160),
    @MontoAbonado      DECIMAL(18,2),
    @NroComprobante    VARCHAR(50),
    @UserNameEncargado NVARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PagosOrdenCompra (IdOrdenCompra, CVU, Banco, Titular, MontoAbonado, FechaOperacion, NroComprobante, Estado, UserNameEncargado)
    VALUES (@IdOrdenCompra, @CVU, @Banco, @Titular, @MontoAbonado, GETDATE(), @NroComprobante, 'Pagado', @UserNameEncargado);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO
IF OBJECT_ID('dbo.sp_PagoOrdenCompra_BuscarPorOrden_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_PagoOrdenCompra_BuscarPorOrden_GO44]
GO
CREATE PROCEDURE [dbo].[sp_PagoOrdenCompra_BuscarPorOrden_GO44]
    @IdOrdenCompra INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, IdOrdenCompra, CVU, Banco, Titular, MontoAbonado, FechaOperacion, NroComprobante, Estado, UserNameEncargado
    FROM dbo.PagosOrdenCompra
    WHERE IdOrdenCompra = @IdOrdenCompra;
END
GO
IF OBJECT_ID('dbo.sp_Recepcion_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_Recepcion_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_Recepcion_Insertar_GO44]
    @IdOrdenCompra       INT,
    @UserNameAlmacenista NVARCHAR(250),
    @ConFaltantes        BIT,
    @Observaciones       VARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.RecepcionesMercaderia (IdOrdenCompra, UserNameAlmacenista, FechaRecepcion, ConFaltantes, Observaciones)
    VALUES (@IdOrdenCompra, @UserNameAlmacenista, GETDATE(), @ConFaltantes, @Observaciones);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO
IF OBJECT_ID('dbo.sp_Recepcion_BuscarPorOrden_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_Recepcion_BuscarPorOrden_GO44]
GO
CREATE PROCEDURE [dbo].[sp_Recepcion_BuscarPorOrden_GO44]
    @IdOrdenCompra INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, IdOrdenCompra, UserNameAlmacenista, FechaRecepcion, ConFaltantes, Observaciones
    FROM dbo.RecepcionesMercaderia
    WHERE IdOrdenCompra = @IdOrdenCompra;
END
GO

PRINT 'Migración FK Compras aplicada: columnas renombradas, FKs creadas y SPs actualizados.';
GO
