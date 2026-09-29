/* ================================================================
   Fix_Rol_Admin_Patentes_GO44.sql
   Asigna la familia "Admin Total" al rol "Admin" existente
   para que el usuario admin vea TODOS los módulos del menú.
   ================================================================ */

USE [Gestion Usuario]
GO

-- 1) Ver qué familias tiene actualmente el rol Admin (diagnóstico)
PRINT '=== Familias actuales del rol Admin ===';
SELECT R.Nombre AS Rol, F.Nombre AS Familia
FROM dbo.RolFamilia RF
INNER JOIN dbo.Roles R    ON R.Id = RF.IdRol
INNER JOIN dbo.Familia F  ON F.Id = RF.IdFamilia
WHERE R.Nombre = 'Admin';

-- 2) Vincular el rol Admin a la familia "Admin Total" (todas las patentes)
INSERT INTO dbo.RolFamilia (IdRol, IdFamilia)
SELECT R.Id, F.Id
FROM dbo.Roles R, dbo.Familia F
WHERE R.Nombre = 'Admin'
  AND F.Nombre = 'Admin Total'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolFamilia WHERE IdRol = R.Id AND IdFamilia = F.Id);

PRINT '=== Después del fix ===';
SELECT R.Nombre AS Rol, F.Nombre AS Familia
FROM dbo.RolFamilia RF
INNER JOIN dbo.Roles R    ON R.Id = RF.IdRol
INNER JOIN dbo.Familia F  ON F.Id = RF.IdFamilia
WHERE R.Nombre IN ('Admin','Administrador');

PRINT '✅ Rol Admin ahora tiene todas las patentes GO44';
GO
