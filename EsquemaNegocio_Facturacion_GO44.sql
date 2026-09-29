/* ================================================================
   EsquemaNegocio_Facturacion_GO44.sql
   CU04 Generar Factura + CU05 Cobrar Venta
   ================================================================ */

USE [Gestion Usuario]
GO

/* ============ 1. TABLAS ============ */

IF OBJECT_ID('dbo.Facturas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Facturas
    (
        Id             INT           IDENTITY(1,1) NOT NULL,
        NumeroFactura  VARCHAR(20)   NOT NULL,
        IdCarrito      INT           NOT NULL,
        DniCliente     VARCHAR(15)   NOT NULL,
        LoginCajero    NVARCHAR(250) NOT NULL,
        FechaEmision   DATETIME      NOT NULL CONSTRAINT DF_Facturas_FechaEmision DEFAULT (GETDATE()),
        Subtotal       DECIMAL(12,2) NOT NULL,
        IVA            DECIMAL(12,2) NOT NULL,
        Total          DECIMAL(12,2) NOT NULL,
        Estado         VARCHAR(20)   NOT NULL CONSTRAINT DF_Facturas_Estado DEFAULT ('Pendiente')
                                              CONSTRAINT CK_Facturas_Estado
                                              CHECK (Estado IN ('Pendiente','Cobrada','Anulada')),
        CONSTRAINT PK_Facturas               PRIMARY KEY (Id),
        CONSTRAINT UQ_Facturas_Numero        UNIQUE (NumeroFactura),
        CONSTRAINT FK_Facturas_Carrito       FOREIGN KEY (IdCarrito)   REFERENCES dbo.Carrito(Id),
        CONSTRAINT FK_Facturas_Cliente       FOREIGN KEY (DniCliente)  REFERENCES dbo.Clientes(DNI),
        CONSTRAINT FK_Facturas_Cajero        FOREIGN KEY (LoginCajero) REFERENCES dbo.Usuario(UserName)
    );
END
GO

IF OBJECT_ID('dbo.LineaFactura', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LineaFactura
    (
        Id              INT           IDENTITY(1,1) NOT NULL,
        IdFactura       INT           NOT NULL,
        IdComponente    INT           NOT NULL,
        Cantidad        INT           NOT NULL CHECK (Cantidad > 0),
        PrecioUnitario  DECIMAL(12,2) NOT NULL CHECK (PrecioUnitario >= 0),
        Subtotal        DECIMAL(12,2) NOT NULL,
        CONSTRAINT PK_LineaFactura               PRIMARY KEY (Id),
        CONSTRAINT FK_LineaFactura_Factura       FOREIGN KEY (IdFactura)    REFERENCES dbo.Facturas(Id) ON DELETE CASCADE,
        CONSTRAINT FK_LineaFactura_Componente    FOREIGN KEY (IdComponente) REFERENCES dbo.Componentes(Id)
    );
END
GO

IF OBJECT_ID('dbo.CobrosFactura', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CobrosFactura
    (
        Id               INT           IDENTITY(1,1) NOT NULL,
        IdFactura        INT           NOT NULL,
        MetodoPago       VARCHAR(20)   NOT NULL CHECK (MetodoPago IN ('Efectivo','Tarjeta')),
        Monto            DECIMAL(12,2) NOT NULL,
        FechaCobro       DATETIME      NOT NULL CONSTRAINT DF_CobrosFactura_Fecha DEFAULT (GETDATE()),
        -- Datos tarjeta (solo si MetodoPago = 'Tarjeta'); nunca guardar CVV real (solo confirmación)
        NroTarjetaEnmasc VARCHAR(20)   NULL,     -- ej: ****-****-****-1234
        Banco            VARCHAR(80)   NULL,
        TitularNombre    VARCHAR(80)   NULL,
        TitularApellido  VARCHAR(80)   NULL,
        CodigoAutoriz    VARCHAR(30)   NULL,     -- código devuelto por el banco
        CONSTRAINT PK_CobrosFactura         PRIMARY KEY (Id),
        CONSTRAINT FK_CobrosFactura_Factura FOREIGN KEY (IdFactura) REFERENCES dbo.Facturas(Id) ON DELETE CASCADE
    );
END
GO

/* ============ 2. INSERTS EN MODULO / TIPO EVENTO ============ */

IF NOT EXISTS (SELECT 1 FROM dbo.Modulo WHERE Nombre = 'Factura')
    INSERT INTO dbo.Modulo (Nombre) VALUES ('Factura');

IF NOT EXISTS (SELECT 1 FROM dbo.Modulo WHERE Nombre = 'Cobro')
    INSERT INTO dbo.Modulo (Nombre) VALUES ('Cobro');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Factura generada')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Factura generada');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Factura anulada')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Factura anulada');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Cobro registrado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Cobro registrado');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Cobro rechazado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Cobro rechazado');
GO

/* ============ 3. STORED PROCEDURES FACTURAS ============ */

IF OBJECT_ID('dbo.sp_Factura_Insertar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Factura_Insertar_GO44;
GO
CREATE PROCEDURE dbo.sp_Factura_Insertar_GO44
    @NumeroFactura VARCHAR(20),
    @IdCarrito     INT,
    @DniCliente    VARCHAR(15),
    @LoginCajero   NVARCHAR(250),
    @Subtotal      DECIMAL(12,2),
    @IVA           DECIMAL(12,2),
    @Total         DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Facturas (NumeroFactura, IdCarrito, DniCliente, LoginCajero,
                              FechaEmision, Subtotal, IVA, Total, Estado)
    VALUES (@NumeroFactura, @IdCarrito, @DniCliente, @LoginCajero,
            GETDATE(), @Subtotal, @IVA, @Total, 'Pendiente');
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdFactura;
END
GO

IF OBJECT_ID('dbo.sp_LineaFactura_Insertar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LineaFactura_Insertar_GO44;
GO
CREATE PROCEDURE dbo.sp_LineaFactura_Insertar_GO44
    @IdFactura      INT,
    @IdComponente   INT,
    @Cantidad       INT,
    @PrecioUnitario DECIMAL(12,2),
    @Subtotal       DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LineaFactura (IdFactura, IdComponente, Cantidad, PrecioUnitario, Subtotal)
    VALUES (@IdFactura, @IdComponente, @Cantidad, @PrecioUnitario, @Subtotal);
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

IF OBJECT_ID('dbo.sp_Factura_MarcarCobrada_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Factura_MarcarCobrada_GO44;
GO
CREATE PROCEDURE dbo.sp_Factura_MarcarCobrada_GO44
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Facturas SET Estado = 'Cobrada' WHERE Id = @Id AND Estado = 'Pendiente';
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

IF OBJECT_ID('dbo.sp_Factura_BuscarPorId_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Factura_BuscarPorId_GO44;
GO
CREATE PROCEDURE dbo.sp_Factura_BuscarPorId_GO44
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, NumeroFactura, IdCarrito, DniCliente, LoginCajero,
           FechaEmision, Subtotal, IVA, Total, Estado
    FROM dbo.Facturas
    WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.sp_LineaFactura_ListarPorFactura_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LineaFactura_ListarPorFactura_GO44;
GO
CREATE PROCEDURE dbo.sp_LineaFactura_ListarPorFactura_GO44
    @IdFactura INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT LF.Id, LF.IdFactura, LF.IdComponente, LF.Cantidad,
           LF.PrecioUnitario, LF.Subtotal,
           C.Codigo, C.Nombre AS ComponenteNombre
    FROM dbo.LineaFactura LF
    INNER JOIN dbo.Componentes C ON C.Id = LF.IdComponente
    WHERE LF.IdFactura = @IdFactura
    ORDER BY LF.Id;
END
GO

IF OBJECT_ID('dbo.sp_Cobro_Registrar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cobro_Registrar_GO44;
GO
CREATE PROCEDURE dbo.sp_Cobro_Registrar_GO44
    @IdFactura         INT,
    @MetodoPago        VARCHAR(20),
    @Monto             DECIMAL(12,2),
    @NroTarjetaEnmasc  VARCHAR(20) = NULL,
    @Banco             VARCHAR(80) = NULL,
    @TitularNombre     VARCHAR(80) = NULL,
    @TitularApellido   VARCHAR(80) = NULL,
    @CodigoAutoriz     VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.CobrosFactura (IdFactura, MetodoPago, Monto, FechaCobro,
                                    NroTarjetaEnmasc, Banco, TitularNombre, TitularApellido, CodigoAutoriz)
    VALUES (@IdFactura, @MetodoPago, @Monto, GETDATE(),
            @NroTarjetaEnmasc, @Banco, @TitularNombre, @TitularApellido, @CodigoAutoriz);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdCobro;
END
GO

/* Buscar carrito confirmado pendiente de facturar por DNI */
IF OBJECT_ID('dbo.sp_Carrito_BuscarPendientePorDNI_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Carrito_BuscarPendientePorDNI_GO44;
GO
CREATE PROCEDURE dbo.sp_Carrito_BuscarPendientePorDNI_GO44
    @DNI VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    -- Trae el ÚLTIMO carrito confirmado del cliente que aún no tenga factura asociada
    SELECT TOP 1 C.Id, C.DniCliente, C.LoginVendedor, C.FechaCreacion, C.Estado, C.Total
    FROM dbo.Carrito C
    LEFT JOIN dbo.Facturas F ON F.IdCarrito = C.Id
    WHERE C.DniCliente = @DNI
      AND C.Estado = 'Confirmado'
      AND F.Id IS NULL
    ORDER BY C.FechaCreacion DESC;
END
GO

/* ================================================================
   4. NUEVOS ROLES + FAMILIAS + PATENTES
   Reutiliza el modelo Composite existente (Rol → Familia → Patente)
   ================================================================ */

/* --- Patentes por módulo (DataKey usado en TienePermiso) --- */

DECLARE @patentes TABLE (Nombre NVARCHAR(80), DataKey NVARCHAR(100));

INSERT INTO @patentes VALUES
    -- ADMIN
    ('Gestionar usuarios',        'Admin.Usuarios'),
    ('Gestionar permisos',        'Admin.Permisos'),
    ('Ver bitácora',              'Admin.Bitacora'),
    ('Backup y restore',          'Admin.Backup'),
    ('Verificar integridad',      'Admin.Integridad'),
    -- MAESTROS
    ('Gestionar clientes',        'Maestros.Clientes'),
    ('Exportar/Importar clientes','Maestros.Clientes.Serializar'),
    ('Gestionar productos',       'Maestros.Productos'),
    ('Alta de productos',         'Maestros.Productos.Alta'),
    ('Modificar precio producto', 'Maestros.Productos.ModificarPrecio'),
    ('Baja de productos',         'Maestros.Productos.Baja'),
    -- VENTAS
    ('Cargar carrito',            'Ventas.CargarCarrito'),
    ('Generar factura',           'Ventas.Facturar'),
    ('Cobrar venta',              'Ventas.Cobrar'),
    -- USUARIO
    ('Cambiar clave',             'Usuario.CambiarClave');

/* Inserta patentes que no existan (usa Nombre como key) */
INSERT INTO dbo.Patente (Nombre, DataKey)
SELECT p.Nombre, p.DataKey
FROM @patentes p
WHERE NOT EXISTS (SELECT 1 FROM dbo.Patente WHERE Nombre = p.Nombre);
GO

/* --- Familias (grupos de patentes) --- */

IF NOT EXISTS (SELECT 1 FROM dbo.Familia WHERE Nombre = 'Cajero Base')
    INSERT INTO dbo.Familia (Nombre) VALUES ('Cajero Base');

IF NOT EXISTS (SELECT 1 FROM dbo.Familia WHERE Nombre = 'Vendedor Base')
    INSERT INTO dbo.Familia (Nombre) VALUES ('Vendedor Base');

IF NOT EXISTS (SELECT 1 FROM dbo.Familia WHERE Nombre = 'Almacenista Base')
    INSERT INTO dbo.Familia (Nombre) VALUES ('Almacenista Base');

IF NOT EXISTS (SELECT 1 FROM dbo.Familia WHERE Nombre = 'Admin Total')
    INSERT INTO dbo.Familia (Nombre) VALUES ('Admin Total');
GO

/* --- Asociar patentes a familias --- */

-- Familia Cajero Base: facturar, cobrar, ver/registrar clientes, cambiar clave
INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente)
SELECT F.Id, P.Id
FROM dbo.Familia F, dbo.Patente P
WHERE F.Nombre = 'Cajero Base'
  AND P.DataKey IN ('Ventas.Facturar','Ventas.Cobrar','Maestros.Clientes','Usuario.CambiarClave')
  AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente WHERE IdFamilia = F.Id AND IdPatente = P.Id);

-- Familia Vendedor Base: cargar carrito, ver productos y clientes, cambiar clave
INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente)
SELECT F.Id, P.Id
FROM dbo.Familia F, dbo.Patente P
WHERE F.Nombre = 'Vendedor Base'
  AND P.DataKey IN ('Ventas.CargarCarrito','Maestros.Productos','Maestros.Clientes','Usuario.CambiarClave')
  AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente WHERE IdFamilia = F.Id AND IdPatente = P.Id);

-- Familia Almacenista Base: ABM completo productos, cambiar clave
INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente)
SELECT F.Id, P.Id
FROM dbo.Familia F, dbo.Patente P
WHERE F.Nombre = 'Almacenista Base'
  AND P.DataKey IN ('Maestros.Productos','Maestros.Productos.Alta','Maestros.Productos.ModificarPrecio',
                    'Maestros.Productos.Baja','Usuario.CambiarClave')
  AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente WHERE IdFamilia = F.Id AND IdPatente = P.Id);

-- Familia Admin Total: TODAS las patentes
INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente)
SELECT F.Id, P.Id
FROM dbo.Familia F, dbo.Patente P
WHERE F.Nombre = 'Admin Total'
  AND NOT EXISTS (SELECT 1 FROM dbo.FamiliaPatente WHERE IdFamilia = F.Id AND IdPatente = P.Id);
GO

/* --- Roles nuevos --- */

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Cajero')
    INSERT INTO dbo.Roles (Nombre) VALUES ('Cajero');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Vendedor')
    INSERT INTO dbo.Roles (Nombre) VALUES ('Vendedor');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Almacenista')
    INSERT INTO dbo.Roles (Nombre) VALUES ('Almacenista');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Administrador')
    INSERT INTO dbo.Roles (Nombre) VALUES ('Administrador');
GO

/* --- Asignar familias a roles --- */

INSERT INTO dbo.RolFamilia (IdRol, IdFamilia)
SELECT R.Id, F.Id FROM dbo.Roles R, dbo.Familia F
WHERE (R.Nombre = 'Cajero'        AND F.Nombre = 'Cajero Base')
   OR (R.Nombre = 'Vendedor'      AND F.Nombre = 'Vendedor Base')
   OR (R.Nombre = 'Almacenista'   AND F.Nombre = 'Almacenista Base')
   OR (R.Nombre = 'Administrador' AND F.Nombre = 'Admin Total')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolFamilia WHERE IdRol = R.Id AND IdFamilia = F.Id);
GO

PRINT '✅ Facturación GO44 + Roles/Patentes aplicados';
GO
