/* ================================================================
   Fix_SP_Componente_ABM_GO44.sql
   Crea los 4 SPs faltantes del ABM de Productos:
     · sp_Componente_Insertar_GO44        (Nuevo Producto)
     · sp_Componente_ActualizarPrecio_GO44 (Modificar Precio)
     · sp_Componente_DarDeBaja_GO44        (Baja lógica)
     · sp_Componente_Reactivar_GO44        (Reactivar)
   ================================================================ */
USE [Gestion Usuario]
GO

/* ---------- INSERTAR ---------- */
IF OBJECT_ID('dbo.sp_Componente_Insertar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_Insertar_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_Insertar_GO44
    @Codigo       VARCHAR(30),
    @Nombre       VARCHAR(120),
    @Descripcion  VARCHAR(500) = NULL,
    @Categoria    VARCHAR(80)  = NULL,
    @Precio       DECIMAL(12,2),
    @StockActual  INT = 0,
    @StockMinimo  INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Componentes (Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo, Activo)
    VALUES (@Codigo, @Nombre, @Descripcion, @Categoria, @Precio, @StockActual, @StockMinimo, 1);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO

/* ---------- ACTUALIZAR PRECIO ---------- */
IF OBJECT_ID('dbo.sp_Componente_ActualizarPrecio_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_ActualizarPrecio_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_ActualizarPrecio_GO44
    @Id     INT,
    @Precio DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Componentes SET Precio = @Precio WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

/* ---------- DAR DE BAJA (baja lógica: Activo = 0) ---------- */
IF OBJECT_ID('dbo.sp_Componente_DarDeBaja_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_DarDeBaja_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_DarDeBaja_GO44
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Componentes SET Activo = 0 WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

/* ---------- REACTIVAR (Activo = 1) ---------- */
IF OBJECT_ID('dbo.sp_Componente_Reactivar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_Reactivar_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_Reactivar_GO44
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Componentes SET Activo = 1 WHERE Id = @Id;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

PRINT '✅ SPs de ABM Componentes creados/actualizados';
GO
