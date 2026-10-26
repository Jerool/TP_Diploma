/* ============================================================================
   MIGRACIÓN — Rep. 3 (INT.) "Estrategia de Ventas".
   Agrega el SP de métricas de rotación sobre una base existente. Idempotente.
   Ejecutar contra [Gestion Usuario].
   ============================================================================ */
USE [Gestion Usuario]
GO

IF OBJECT_ID('dbo.sp_ReporteInteligente_GO44', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_ReporteInteligente_GO44]
GO
CREATE PROCEDURE [dbo].[sp_ReporteInteligente_GO44]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.Id, c.Codigo, c.Nombre, c.Categoria, c.StockActual, c.StockMinimo, c.Precio,
           ISNULL(v30.Vendidas30, 0) AS Vendidas30,
           vall.UltimaVenta
    FROM dbo.Componentes c
    LEFT JOIN (
        SELECT lf.IdComponente, SUM(lf.Cantidad) AS Vendidas30
        FROM dbo.LineaFactura lf
        INNER JOIN dbo.Facturas f ON f.Id = lf.IdFactura
        WHERE f.FechaEmision >= DATEADD(DAY, -30, GETDATE())
        GROUP BY lf.IdComponente
    ) v30 ON v30.IdComponente = c.Id
    LEFT JOIN (
        SELECT lf.IdComponente, MAX(f.FechaEmision) AS UltimaVenta
        FROM dbo.LineaFactura lf
        INNER JOIN dbo.Facturas f ON f.Id = lf.IdFactura
        GROUP BY lf.IdComponente
    ) vall ON vall.IdComponente = c.Id
    WHERE c.Activo = 1
    ORDER BY c.Nombre;
END
GO

PRINT 'Migración Reporte Inteligente aplicada correctamente.';
GO
