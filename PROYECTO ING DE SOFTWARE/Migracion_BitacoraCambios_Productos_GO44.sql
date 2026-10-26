/* ============================================================================
   MIGRACIÓN — Bitácora de cambios de productos (Componentes_C) por triggers SCD2.
   Aplica SOLO los objetos nuevos sobre una base YA EXISTENTE, sin borrar datos.
   Idempotente: se puede correr varias veces. Ejecutar contra [Gestion Usuario].
   ============================================================================ */
USE [Gestion Usuario]
GO

IF OBJECT_ID('dbo.Componentes_C', 'U') IS NULL
CREATE TABLE [dbo].[Componentes_C](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdComponente] [int] NOT NULL,
    [Codigo] [varchar](30) NOT NULL,
    [Nombre] [varchar](120) NOT NULL,
    [Descripcion] [varchar](500) NULL,
    [Categoria] [varchar](80) NULL,
    [Precio] [decimal](12, 2) NOT NULL,
    [StockActual] [int] NOT NULL,
    [StockMinimo] [int] NOT NULL,
    [ActivoProducto] [bit] NOT NULL,
    [TipoCambio] [varchar](10) NOT NULL,
    [FechaInicio] [datetime] NOT NULL CONSTRAINT [DF_CompC_FechaInicio] DEFAULT (getdate()),
    [FechaFin] [datetime] NULL,
    [Act] [bit] NOT NULL CONSTRAINT [DF_CompC_Act] DEFAULT ((1)),
 CONSTRAINT [PK_Componentes_C] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

IF OBJECT_ID('dbo.trg_Componentes_Insert_GO44', 'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_Componentes_Insert_GO44]
GO
CREATE TRIGGER [dbo].[trg_Componentes_Insert_GO44] ON [dbo].[Componentes] AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Componentes_C
        (IdComponente, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo,
         ActivoProducto, TipoCambio, FechaInicio, FechaFin, Act)
    SELECT i.Id, i.Codigo, i.Nombre, i.Descripcion, i.Categoria, i.Precio, i.StockActual, i.StockMinimo,
           i.Activo, 'ALTA', GETDATE(), NULL, 1
    FROM inserted i;
END
GO

IF OBJECT_ID('dbo.trg_Componentes_Update_GO44', 'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_Componentes_Update_GO44]
GO
CREATE TRIGGER [dbo].[trg_Componentes_Update_GO44] ON [dbo].[Componentes] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c SET c.Act = 0, c.FechaFin = GETDATE()
    FROM dbo.Componentes_C c
    INNER JOIN inserted i ON i.Id = c.IdComponente
    WHERE c.Act = 1;

    INSERT INTO dbo.Componentes_C
        (IdComponente, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo,
         ActivoProducto, TipoCambio, FechaInicio, FechaFin, Act)
    SELECT i.Id, i.Codigo, i.Nombre, i.Descripcion, i.Categoria, i.Precio, i.StockActual, i.StockMinimo,
           i.Activo,
           CASE WHEN i.Activo = 0 AND d.Activo = 1 THEN 'BAJA'
                WHEN i.Activo = 1 AND d.Activo = 0 THEN 'REACTIV'
                ELSE 'MODIF' END,
           GETDATE(), NULL, 1
    FROM inserted i
    INNER JOIN deleted d ON d.Id = i.Id;
END
GO

IF OBJECT_ID('dbo.sp_ComponenteCambio_ListarConFiltros_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_ComponenteCambio_ListarConFiltros_GO44]
GO
CREATE PROCEDURE [dbo].[sp_ComponenteCambio_ListarConFiltros_GO44]
    @Codigo   VARCHAR(30)  = NULL,
    @Nombre   VARCHAR(120) = NULL,
    @FechaIni DATETIME     = NULL,
    @FechaFin DATETIME     = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, IdComponente, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo,
           ActivoProducto, TipoCambio, FechaInicio, FechaFin, Act
    FROM dbo.Componentes_C
    WHERE (@Codigo   IS NULL OR Codigo LIKE '%' + @Codigo + '%')
      AND (@Nombre   IS NULL OR Nombre LIKE '%' + @Nombre + '%')
      AND (@FechaIni IS NULL OR FechaInicio >= @FechaIni)
      AND (@FechaFin IS NULL OR FechaInicio <= @FechaFin)
    ORDER BY IdComponente, FechaInicio DESC;
END
GO

IF OBJECT_ID('dbo.sp_ComponenteCambio_MarcarActivo_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_ComponenteCambio_MarcarActivo_GO44]
GO
CREATE PROCEDURE [dbo].[sp_ComponenteCambio_MarcarActivo_GO44]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdComp INT = (SELECT IdComponente FROM dbo.Componentes_C WHERE Id = @Id);
    IF @IdComp IS NULL BEGIN SELECT CAST(0 AS INT) AS Filas; RETURN; END
    UPDATE dbo.Componentes_C SET Act = 0 WHERE IdComponente = @IdComp;
    UPDATE dbo.Componentes_C SET Act = 1 WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

/* Semilla inicial: una versión 'ALTA' vigente por cada producto existente */
IF NOT EXISTS (SELECT 1 FROM dbo.Componentes_C)
INSERT INTO dbo.Componentes_C
    (IdComponente, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo,
     ActivoProducto, TipoCambio, FechaInicio, FechaFin, Act)
SELECT Id, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo,
       Activo, 'ALTA', GETDATE(), NULL, 1
FROM dbo.Componentes;
GO

/* Patente del nuevo item de menú (Maestros -> Productos - C) */
SET IDENTITY_INSERT [dbo].[Patente] ON
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Patente WHERE DataKey = 'Maestros.ProductosCambios')
    INSERT [dbo].[Patente] ([Id], [Nombre], [DataKey]) VALUES (39, N'Maestros - Bitácora de cambios de productos', N'Maestros.ProductosCambios')
GO
SET IDENTITY_INSERT [dbo].[Patente] OFF
GO
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 1 AND IdPatente = 39) INSERT [dbo].[RolPatente] ([IdRol],[IdPatente]) VALUES (1, 39)
GO

PRINT 'Migración Bitácora de cambios de productos aplicada correctamente.';
GO
