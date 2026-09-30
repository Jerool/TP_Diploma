/* ================================================================
   Fix_TipoEvento_Duplicados_GO44.sql
   Agrega tipos de evento faltantes para las validaciones de duplicados
   (email duplicado, teléfono duplicado, etc.)
   ================================================================ */
USE [Gestion Usuario]
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Email duplicado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Email duplicado');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Teléfono duplicado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Teléfono duplicado');

-- por si tampoco existían aún:
IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'DNI duplicado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('DNI duplicado');
GO

PRINT '✅ Tipos de evento de duplicados agregados al catálogo';
GO
