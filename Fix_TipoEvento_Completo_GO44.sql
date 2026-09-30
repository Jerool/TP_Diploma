/* ================================================================
   Fix_TipoEvento_Completo_GO44.sql
   Agrega TODOS los TipoEvento que usa la app en la bitácora.
   Idempotente: solo inserta los que no existen.
   ================================================================ */
USE [Gestion Usuario]
GO

DECLARE @Eventos TABLE (Nombre VARCHAR(100));

INSERT INTO @Eventos (Nombre) VALUES
  /* --- Cliente --- */
  ('Cliente registrado'),
  ('Cliente modificado'),
  ('DNI duplicado'),
  ('Email duplicado'),
  ('Teléfono duplicado'),

  /* --- Componente (ABM Productos) --- */
  ('Componente registrado'),
  ('Componente modificado'),
  ('Componente rechazado'),
  ('Componente dado de baja'),
  ('Componente reactivado'),
  ('Stock insuficiente'),

  /* --- Carrito --- */
  ('Carrito confirmado'),
  ('Carrito cancelado'),

  /* --- Factura / Cobro --- */
  ('Factura generada'),
  ('Factura anulada'),
  ('Cobro registrado'),
  ('Factura cobrada'),

  /* --- Permisos / Admin --- */
  ('Familia creada'),
  ('Familia modificada'),
  ('Familia eliminada'),
  ('Rol creado'),
  ('Rol modificado'),
  ('Rol eliminado'),

  /* --- Usuario / Sesión --- */
  ('Login exitoso'),
  ('Logout realizado'),
  ('Usuario creado'),
  ('Usuario desbloqueado'),
  ('Usuario bloqueado'),
  ('Usuario bloqueado por intentos fallidos'),
  ('Usuario inexistente'),
  ('Usuario inactivo'),
  ('Contraseña incorrecta'),
  ('Contraseña cambiada exitosamente'),
  ('Intento de login con sesión ya activa'),
  ('Email modificado'),
  ('Rol modificado usuario'),
  ('Usuario activado'),
  ('Usuario desactivado'),
  ('Idioma cambiado'),

  /* --- Integridad / Backup --- */
  ('Backup automático'),
  ('Backup manual'),
  ('Restauración de backup'),
  ('Integridad recalculada'),
  ('Integridad rota'),
  ('Integridad reparada');

-- Insertar solo los que aún no existen
INSERT INTO dbo.TipoEvento (Nombre)
SELECT e.Nombre
FROM @Eventos e
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.TipoEvento t WHERE t.Nombre = e.Nombre
);

DECLARE @Agregados INT = @@ROWCOUNT;
PRINT CONCAT('✅ TipoEvento agregados: ', @Agregados);
PRINT '   (los que ya existían se ignoran)';
GO

-- Verificación: listar todos los TipoEvento actuales
SELECT Id, Nombre FROM dbo.TipoEvento ORDER BY Nombre;
GO
