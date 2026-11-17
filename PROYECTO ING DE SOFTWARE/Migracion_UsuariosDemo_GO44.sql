/* ============================================================================
   MIGRACIÓN — Usuarios de demo para evaluación (uno por rol de negocio).
   Aplica SOLO sobre una base YA EXISTENTE, sin borrar nada. Idempotente.
   Ejecutar contra [Gestion Usuario].

   En TODOS: la contraseña es igual al nombre de usuario (minúscula), SHA-256.

     Usuario        Contraseña     Rol                     Accede a
     -----------    -----------    --------------------    -------------------------------
     admin          admin          Admin                   TODO (rol con "admin" ve todo)
     cajero         cajero         Cajero                  Ventas (carrito, facturar, cobrar) + Clientes + Reportes
     vendedor       vendedor       Vendedor                Ventas (cargar carrito) + Clientes
     almacenista    almacenista    Almacenista             Compras > Recepción de mercadería
     compras        compras        Encargado de Compras    Compras (proveedor, SC, OC, pago)
   ============================================================================ */
USE [Gestion Usuario]
GO

/* 1) Permisos (RolPatente) de los roles de negocio no-admin. */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 24 AND IdPatente = 30) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (24, 30) /* Cajero: Ventas.CargarCarrito */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 24 AND IdPatente = 31) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (24, 31) /* Cajero: Ventas.Facturar */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 24 AND IdPatente = 32) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (24, 32) /* Cajero: Ventas.Cobrar */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 24 AND IdPatente = 24) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (24, 24) /* Cajero: Maestros.Clientes */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 25 AND IdPatente = 30) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (25, 30) /* Vendedor: Ventas.CargarCarrito */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 25 AND IdPatente = 24) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (25, 24) /* Vendedor: Maestros.Clientes */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 26 AND IdPatente = 38) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (26, 38) /* Almacenista: Almacen.RecibirMercaderia */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 28 AND IdPatente = 34) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (28, 34) /* Compras: RegistrarProveedor */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 28 AND IdPatente = 35) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (28, 35) /* Compras: GenerarSC */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 28 AND IdPatente = 36) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (28, 36) /* Compras: GenerarOC */
IF NOT EXISTS (SELECT 1 FROM dbo.RolPatente WHERE IdRol = 28 AND IdPatente = 37) INSERT dbo.RolPatente (IdRol, IdPatente) VALUES (28, 37) /* Compras: PagarOC */
GO

/* 2) Usuarios de demo (contraseña = usuario, SHA-256). DebeCambiarContrasena = 0 para no forzar el cambio. */
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE UserName = N'admin')
    INSERT dbo.Usuario (DNI, Apellido, Nombre, UserName, Contrasena, Email, Bloqueo, Activo, IdRol, IntentosFallidos, UltimoIntentoFallido, DebeCambiarContrasena, Idioma)
    VALUES (N'90000001', N'Demo', N'Admin', N'admin', N'8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', N'admin@demo.local', 0, 1, 1, 0, NULL, 0, N'es')
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE UserName = N'cajero')
    INSERT dbo.Usuario (DNI, Apellido, Nombre, UserName, Contrasena, Email, Bloqueo, Activo, IdRol, IntentosFallidos, UltimoIntentoFallido, DebeCambiarContrasena, Idioma)
    VALUES (N'90000002', N'Demo', N'Cajero', N'cajero', N'fea740101dbb727886b6908e7bc196a55054374c6827b41a60081c2525975b4d', N'cajero@demo.local', 0, 1, 24, 0, NULL, 0, N'es')
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE UserName = N'vendedor')
    INSERT dbo.Usuario (DNI, Apellido, Nombre, UserName, Contrasena, Email, Bloqueo, Activo, IdRol, IntentosFallidos, UltimoIntentoFallido, DebeCambiarContrasena, Idioma)
    VALUES (N'90000003', N'Demo', N'Vendedor', N'vendedor', N'e8827f3c0bcc90509b7d6841d446b163a671cac807a5f1bf41218667546ce80b', N'vendedor@demo.local', 0, 1, 25, 0, NULL, 0, N'es')
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE UserName = N'almacenista')
    INSERT dbo.Usuario (DNI, Apellido, Nombre, UserName, Contrasena, Email, Bloqueo, Activo, IdRol, IntentosFallidos, UltimoIntentoFallido, DebeCambiarContrasena, Idioma)
    VALUES (N'90000004', N'Demo', N'Almacenista', N'almacenista', N'0a0615d961e894b3b8568a6b356cc156a19406563cb231c51b369d85245931bb', N'almacenista@demo.local', 0, 1, 26, 0, NULL, 0, N'es')
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE UserName = N'compras')
    INSERT dbo.Usuario (DNI, Apellido, Nombre, UserName, Contrasena, Email, Bloqueo, Activo, IdRol, IntentosFallidos, UltimoIntentoFallido, DebeCambiarContrasena, Idioma)
    VALUES (N'90000005', N'Demo', N'Compras', N'compras', N'f74d0c877bba1b76db96b606bca17cd26b5103d0fa97e86b53a9f346211bb0fe', N'compras@demo.local', 0, 1, 28, 0, NULL, 0, N'es')
GO

PRINT 'Usuarios de demo creados: admin, cajero, vendedor, almacenista, compras (contraseña = usuario).';
GO
