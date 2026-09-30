/* ================================================================
   Fix_SP_Factura_ListarConFiltros_GO44.sql
   Crea el SP faltante sp_Factura_ListarConFiltros_GO44 que usa
   Reportes → Reporte de Facturas (con filtros de fecha, DNI y estado).
   Corre esto una vez en la base de negocio.
   ================================================================ */
USE [Gestion Usuario]
GO

IF OBJECT_ID('dbo.sp_Factura_ListarConFiltros_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Factura_ListarConFiltros_GO44;
GO

CREATE PROCEDURE dbo.sp_Factura_ListarConFiltros_GO44
    @FechaDesde  DATETIME       = NULL,
    @FechaHasta  DATETIME       = NULL,
    @DniCliente  VARCHAR(15)    = NULL,
    @Estado      VARCHAR(20)    = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        F.Id,
        F.NumeroFactura,
        F.IdCarrito,
        F.DniCliente,
        F.LoginCajero,
        F.FechaEmision,
        F.Subtotal,
        F.IVA,
        F.Total,
        F.Estado,
        ISNULL(C.Apellido + ', ' + C.Nombre, '(cliente eliminado)') AS ClienteNombreCompleto
    FROM dbo.Facturas F
    LEFT JOIN dbo.Clientes C ON C.DNI = F.DniCliente
    WHERE (@FechaDesde IS NULL OR F.FechaEmision >= @FechaDesde)
      AND (@FechaHasta IS NULL OR F.FechaEmision <= DATEADD(day, 1, @FechaHasta))
      AND (@DniCliente IS NULL OR @DniCliente = '' OR F.DniCliente = @DniCliente)
      AND (@Estado IS NULL OR @Estado = '' OR F.Estado = @Estado)
    ORDER BY F.FechaEmision DESC;
END
GO

PRINT '✅ sp_Factura_ListarConFiltros_GO44 creado';
GO
