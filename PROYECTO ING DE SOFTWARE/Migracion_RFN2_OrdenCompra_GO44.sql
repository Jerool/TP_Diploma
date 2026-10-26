/* ============================================================================
   MIGRACIÓN RFN2 — CU08 Orden de Compra · CU09 Pagar OC · CU10 Recepción
   Aplica SOLO los objetos nuevos sobre una base YA EXISTENTE, sin borrar datos.
   Es idempotente: se puede correr varias veces sin romper nada.
   Ejecutar contra la base [Gestion Usuario].
   ============================================================================ */
USE [Gestion Usuario]
GO

/* ---------- Tablas ---------- */
IF OBJECT_ID('dbo.OrdenesCompra', 'U') IS NULL
CREATE TABLE [dbo].[OrdenesCompra](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [NumeroOC] AS ('OC-'+right('000000'+CONVERT([varchar](6),[Id]),(6))) PERSISTED,
    [IdSolicitud] [int] NULL,
    [CuitProveedor] [varchar](11) NOT NULL,
    [LoginEncargado] [varchar](50) NOT NULL,
    [FechaEmision] [datetime] NOT NULL CONSTRAINT [DF_OC_FechaEmision] DEFAULT (getdate()),
    [FechaEntrega] [datetime] NOT NULL,
    [Total] [decimal](18, 2) NOT NULL,
    [Estado] [varchar](25) NOT NULL CONSTRAINT [DF_OC_Estado] DEFAULT ('Pendiente'),
 CONSTRAINT [PK_OrdenesCompra] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO
IF OBJECT_ID('dbo.LineasOrdenCompra', 'U') IS NULL
CREATE TABLE [dbo].[LineasOrdenCompra](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdOrdenCompra] [int] NOT NULL,
    [IdComponente] [int] NOT NULL,
    [Cantidad] [int] NOT NULL,
    [PrecioUnitario] [decimal](18, 2) NOT NULL,
    [Subtotal] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_LineasOrdenCompra] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO
IF OBJECT_ID('dbo.PagosOrdenCompra', 'U') IS NULL
CREATE TABLE [dbo].[PagosOrdenCompra](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdOrdenCompra] [int] NOT NULL,
    [CVU] [varchar](22) NOT NULL,
    [Banco] [varchar](80) NOT NULL,
    [Titular] [varchar](160) NOT NULL,
    [MontoAbonado] [decimal](18, 2) NOT NULL,
    [FechaOperacion] [datetime] NOT NULL CONSTRAINT [DF_POC_FechaOperacion] DEFAULT (getdate()),
    [NroComprobante] [varchar](50) NOT NULL,
    [Estado] [varchar](20) NOT NULL CONSTRAINT [DF_POC_Estado] DEFAULT ('Pagado'),
    [LoginEncargado] [varchar](50) NOT NULL,
 CONSTRAINT [PK_PagosOrdenCompra] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO
IF OBJECT_ID('dbo.RecepcionesMercaderia', 'U') IS NULL
CREATE TABLE [dbo].[RecepcionesMercaderia](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdOrdenCompra] [int] NOT NULL,
    [LoginAlmacenista] [varchar](50) NOT NULL,
    [FechaRecepcion] [datetime] NOT NULL CONSTRAINT [DF_REC_FechaRecepcion] DEFAULT (getdate()),
    [ConFaltantes] [bit] NOT NULL CONSTRAINT [DF_REC_ConFaltantes] DEFAULT ((0)),
    [Observaciones] [varchar](400) NULL,
 CONSTRAINT [PK_RecepcionesMercaderia] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO
IF OBJECT_ID('dbo.LineasRecepcion', 'U') IS NULL
CREATE TABLE [dbo].[LineasRecepcion](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdRecepcion] [int] NOT NULL,
    [IdComponente] [int] NOT NULL,
    [CantidadPedida] [int] NOT NULL,
    [CantidadRecibida] [int] NOT NULL,
 CONSTRAINT [PK_LineasRecepcion] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

/* ---------- FKs y Checks ---------- */
IF OBJECT_ID('dbo.FK_OC_Proveedor', 'F') IS NULL
ALTER TABLE [dbo].[OrdenesCompra] WITH CHECK ADD CONSTRAINT [FK_OC_Proveedor]
    FOREIGN KEY([CuitProveedor]) REFERENCES [dbo].[Proveedores] ([CUIT])
GO
IF OBJECT_ID('dbo.FK_OC_Solicitud', 'F') IS NULL
ALTER TABLE [dbo].[OrdenesCompra] WITH CHECK ADD CONSTRAINT [FK_OC_Solicitud]
    FOREIGN KEY([IdSolicitud]) REFERENCES [dbo].[SolicitudesCotizacion] ([Id])
GO
IF OBJECT_ID('dbo.CK_OC_Estado', 'C') IS NULL
ALTER TABLE [dbo].[OrdenesCompra] WITH CHECK ADD CONSTRAINT [CK_OC_Estado]
    CHECK ([Estado] IN ('Pendiente','Pagada','Recibida','RecibidaConFaltantes','Cancelada'))
GO
IF OBJECT_ID('dbo.FK_LOC_Orden', 'F') IS NULL
ALTER TABLE [dbo].[LineasOrdenCompra] WITH CHECK ADD CONSTRAINT [FK_LOC_Orden]
    FOREIGN KEY([IdOrdenCompra]) REFERENCES [dbo].[OrdenesCompra] ([Id])
GO
IF OBJECT_ID('dbo.FK_LOC_Componente', 'F') IS NULL
ALTER TABLE [dbo].[LineasOrdenCompra] WITH CHECK ADD CONSTRAINT [FK_LOC_Componente]
    FOREIGN KEY([IdComponente]) REFERENCES [dbo].[Componentes] ([Id])
GO
IF OBJECT_ID('dbo.FK_POC_Orden', 'F') IS NULL
ALTER TABLE [dbo].[PagosOrdenCompra] WITH CHECK ADD CONSTRAINT [FK_POC_Orden]
    FOREIGN KEY([IdOrdenCompra]) REFERENCES [dbo].[OrdenesCompra] ([Id])
GO
IF OBJECT_ID('dbo.FK_REC_Orden', 'F') IS NULL
ALTER TABLE [dbo].[RecepcionesMercaderia] WITH CHECK ADD CONSTRAINT [FK_REC_Orden]
    FOREIGN KEY([IdOrdenCompra]) REFERENCES [dbo].[OrdenesCompra] ([Id])
GO
IF OBJECT_ID('dbo.FK_LREC_Recepcion', 'F') IS NULL
ALTER TABLE [dbo].[LineasRecepcion] WITH CHECK ADD CONSTRAINT [FK_LREC_Recepcion]
    FOREIGN KEY([IdRecepcion]) REFERENCES [dbo].[RecepcionesMercaderia] ([Id])
GO
IF OBJECT_ID('dbo.FK_LREC_Componente', 'F') IS NULL
ALTER TABLE [dbo].[LineasRecepcion] WITH CHECK ADD CONSTRAINT [FK_LREC_Componente]
    FOREIGN KEY([IdComponente]) REFERENCES [dbo].[Componentes] ([Id])
GO

/* ---------- Stored Procedures ---------- */
IF OBJECT_ID('dbo.sp_OrdenCompra_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_Insertar_GO44]
    @IdSolicitud    INT,
    @CuitProveedor  VARCHAR(11),
    @LoginEncargado VARCHAR(50),
    @FechaEntrega   DATETIME,
    @Total          DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.OrdenesCompra (IdSolicitud, CuitProveedor, LoginEncargado, FechaEmision, FechaEntrega, Total, Estado)
    VALUES (@IdSolicitud, @CuitProveedor, @LoginEncargado, GETDATE(), @FechaEntrega, @Total, 'Pendiente');
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO
IF OBJECT_ID('dbo.sp_LineaOrdenCompra_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_LineaOrdenCompra_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_LineaOrdenCompra_Insertar_GO44]
    @IdOrdenCompra  INT,
    @IdComponente   INT,
    @Cantidad       INT,
    @PrecioUnitario DECIMAL(18,2),
    @Subtotal       DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LineasOrdenCompra (IdOrdenCompra, IdComponente, Cantidad, PrecioUnitario, Subtotal)
    VALUES (@IdOrdenCompra, @IdComponente, @Cantidad, @PrecioUnitario, @Subtotal);
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO
IF OBJECT_ID('dbo.sp_OrdenCompra_CambiarEstado_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_CambiarEstado_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_CambiarEstado_GO44]
    @Id     INT,
    @Estado VARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.OrdenesCompra SET Estado = @Estado WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO
IF OBJECT_ID('dbo.sp_OrdenCompra_Listar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_OrdenCompra_Listar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_OrdenCompra_Listar_GO44]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT O.Id, O.NumeroOC, O.IdSolicitud, S.NumeroSC, O.CuitProveedor, P.Empresa AS EmpresaProveedor,
           O.LoginEncargado, O.FechaEmision, O.FechaEntrega, O.Total, O.Estado
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
           O.LoginEncargado, O.FechaEmision, O.FechaEntrega, O.Total, O.Estado
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
           O.LoginEncargado, O.FechaEmision, O.FechaEntrega, O.Total, O.Estado
    FROM dbo.OrdenesCompra O
    INNER JOIN dbo.Proveedores P ON P.CUIT = O.CuitProveedor
    LEFT JOIN dbo.SolicitudesCotizacion S ON S.Id = O.IdSolicitud
    WHERE O.Id = @Id;
END
GO
IF OBJECT_ID('dbo.sp_LineaOrdenCompra_ListarPorOrden_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_LineaOrdenCompra_ListarPorOrden_GO44]
GO
CREATE PROCEDURE [dbo].[sp_LineaOrdenCompra_ListarPorOrden_GO44]
    @IdOrdenCompra INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT L.Id, L.IdOrdenCompra, L.IdComponente, L.Cantidad, L.PrecioUnitario, L.Subtotal,
           C.Codigo AS ComponenteCodigo, C.Nombre AS ComponenteNombre, C.Categoria AS ComponenteCategoria
    FROM dbo.LineasOrdenCompra L
    INNER JOIN dbo.Componentes C ON C.Id = L.IdComponente
    WHERE L.IdOrdenCompra = @IdOrdenCompra
    ORDER BY C.Nombre;
END
GO
IF OBJECT_ID('dbo.sp_SolicitudCotizacion_CambiarEstado_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_SolicitudCotizacion_CambiarEstado_GO44]
GO
CREATE PROCEDURE [dbo].[sp_SolicitudCotizacion_CambiarEstado_GO44]
    @Id     INT,
    @Estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SolicitudesCotizacion SET Estado = @Estado WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO
IF OBJECT_ID('dbo.sp_Componente_AumentarStock_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_Componente_AumentarStock_GO44]
GO
CREATE PROCEDURE [dbo].[sp_Componente_AumentarStock_GO44]
    @Id       INT,
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Componentes SET StockActual = StockActual + @Cantidad WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO
IF OBJECT_ID('dbo.sp_PagoOrdenCompra_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_PagoOrdenCompra_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_PagoOrdenCompra_Insertar_GO44]
    @IdOrdenCompra  INT,
    @CVU            VARCHAR(22),
    @Banco          VARCHAR(80),
    @Titular        VARCHAR(160),
    @MontoAbonado   DECIMAL(18,2),
    @NroComprobante VARCHAR(50),
    @LoginEncargado VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PagosOrdenCompra (IdOrdenCompra, CVU, Banco, Titular, MontoAbonado, FechaOperacion, NroComprobante, Estado, LoginEncargado)
    VALUES (@IdOrdenCompra, @CVU, @Banco, @Titular, @MontoAbonado, GETDATE(), @NroComprobante, 'Pagado', @LoginEncargado);
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
    SELECT Id, IdOrdenCompra, CVU, Banco, Titular, MontoAbonado, FechaOperacion, NroComprobante, Estado, LoginEncargado
    FROM dbo.PagosOrdenCompra
    WHERE IdOrdenCompra = @IdOrdenCompra;
END
GO
IF OBJECT_ID('dbo.sp_Recepcion_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_Recepcion_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_Recepcion_Insertar_GO44]
    @IdOrdenCompra    INT,
    @LoginAlmacenista VARCHAR(50),
    @ConFaltantes     BIT,
    @Observaciones    VARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.RecepcionesMercaderia (IdOrdenCompra, LoginAlmacenista, FechaRecepcion, ConFaltantes, Observaciones)
    VALUES (@IdOrdenCompra, @LoginAlmacenista, GETDATE(), @ConFaltantes, @Observaciones);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO
IF OBJECT_ID('dbo.sp_LineaRecepcion_Insertar_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_LineaRecepcion_Insertar_GO44]
GO
CREATE PROCEDURE [dbo].[sp_LineaRecepcion_Insertar_GO44]
    @IdRecepcion      INT,
    @IdComponente     INT,
    @CantidadPedida   INT,
    @CantidadRecibida INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LineasRecepcion (IdRecepcion, IdComponente, CantidadPedida, CantidadRecibida)
    VALUES (@IdRecepcion, @IdComponente, @CantidadPedida, @CantidadRecibida);
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO
IF OBJECT_ID('dbo.sp_Recepcion_BuscarPorOrden_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_Recepcion_BuscarPorOrden_GO44]
GO
CREATE PROCEDURE [dbo].[sp_Recepcion_BuscarPorOrden_GO44]
    @IdOrdenCompra INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, IdOrdenCompra, LoginAlmacenista, FechaRecepcion, ConFaltantes, Observaciones
    FROM dbo.RecepcionesMercaderia
    WHERE IdOrdenCompra = @IdOrdenCompra;
END
GO
IF OBJECT_ID('dbo.sp_LineaRecepcion_ListarPorRecepcion_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_LineaRecepcion_ListarPorRecepcion_GO44]
GO
CREATE PROCEDURE [dbo].[sp_LineaRecepcion_ListarPorRecepcion_GO44]
    @IdRecepcion INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT L.Id, L.IdRecepcion, L.IdComponente, L.CantidadPedida, L.CantidadRecibida,
           C.Codigo AS ComponenteCodigo, C.Nombre AS ComponenteNombre, C.Categoria AS ComponenteCategoria
    FROM dbo.LineasRecepcion L
    INNER JOIN dbo.Componentes C ON C.Id = L.IdComponente
    WHERE L.IdRecepcion = @IdRecepcion
    ORDER BY C.Nombre;
END
GO

/* ---------- Patentes y permisos (CU08/CU09/CU10) ---------- */
SET IDENTITY_INSERT [dbo].[Patente] ON
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Patente WHERE DataKey = 'Compras.GenerarOC')
    INSERT [dbo].[Patente] ([Id], [Nombre], [DataKey]) VALUES (36, N'Compras - Generar orden de compra', N'Compras.GenerarOC')
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Patente WHERE DataKey = 'Compras.PagarOC')
    INSERT [dbo].[Patente] ([Id], [Nombre], [DataKey]) VALUES (37, N'Compras - Pagar orden de compra', N'Compras.PagarOC')
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Patente WHERE DataKey = 'Almacen.RecibirMercaderia')
    INSERT [dbo].[Patente] ([Id], [Nombre], [DataKey]) VALUES (38, N'Almacén - Recibir mercadería', N'Almacen.RecibirMercaderia')
GO
SET IDENTITY_INSERT [dbo].[Patente] OFF
GO
/* Asignación a roles: Admin(1), Encargado de Compras(28), Almacenista(26) */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 1 AND IdPatente = 36) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (1, 36)
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 1 AND IdPatente = 37) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (1, 37)
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 1 AND IdPatente = 38) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (1, 38)
IF EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = 28)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 28 AND IdPatente = 36) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (28, 36)
    IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 28 AND IdPatente = 37) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (28, 37)
END
IF EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = 26)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 26 AND IdPatente = 38) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (26, 38)
END
GO

PRINT 'Migración RFN2 (CU08/CU09/CU10) aplicada correctamente.';
GO
