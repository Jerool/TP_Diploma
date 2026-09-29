/* ================================================================
   Fix_EVENTOS_Detalle_GO44.sql
   Agranda la columna EVENTOS.Detalle de NVARCHAR(500) a NVARCHAR(MAX)
   para evitar "String or binary data would be truncated" cuando el
   RecalcularTabla del sistema de integridad detecta muchos cambios juntos.
   ================================================================ */

USE [Gestion Usuario]
GO

-- Cambio de tipo: 500 -> MAX (soporta hasta ~2GB de texto)
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'EVENTOS' AND COLUMN_NAME = 'Detalle'
      AND CHARACTER_MAXIMUM_LENGTH = 500
)
BEGIN
    ALTER TABLE dbo.EVENTOS
    ALTER COLUMN Detalle NVARCHAR(MAX) NULL;

    PRINT '✅ EVENTOS.Detalle ampliado a NVARCHAR(MAX)';
END
ELSE
BEGIN
    PRINT 'ℹ️ EVENTOS.Detalle ya no es NVARCHAR(500) — sin cambios';
END
GO
