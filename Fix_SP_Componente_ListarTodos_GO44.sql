/* ================================================================
   Fix_SP_Componente_ListarTodos_GO44.sql
   Crea el SP faltante sp_Componente_ListarTodos_GO44 que usa el
   ABM de Productos cuando se tilda el radio button "Todos".
   Corre esto una sola vez en la base de negocio.
   ================================================================ */
USE [Gestion Usuario]
GO

IF OBJECT_ID('dbo.sp_Componente_ListarTodos_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_ListarTodos_GO44;
GO

CREATE PROCEDURE dbo.sp_Componente_ListarTodos_GO44
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo, Activo
    FROM dbo.Componentes
    ORDER BY Nombre;
END
GO

PRINT '✅ sp_Componente_ListarTodos_GO44 creado';
GO
