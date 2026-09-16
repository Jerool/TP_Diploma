/* ================================================================
   EsquemaNegocio_GO44.sql
   Extensión de negocio para "Gestion Usuario" (TP Diploma - GO44)
   Cubre:
     - CU01 Cargar Carrito
     - CU02 Registrar Cliente
     - CU03 Seleccionar Componente
   Autor: GO44
   ================================================================ */

USE [Gestion Usuario]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ================================================================
   SECCION 1 · TABLAS
   ================================================================ */

/* ---------- CLIENTES ---------- */
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        DNI          VARCHAR(15)  NOT NULL,
        Apellido     VARCHAR(80)  NOT NULL,
        Nombre       VARCHAR(80)  NOT NULL,
        Email        VARCHAR(255) NOT NULL,       -- se guarda cifrado (EncriptadorReversible)
        Telefono     VARCHAR(30)  NULL,
        FechaAlta    DATETIME     NOT NULL CONSTRAINT DF_Clientes_FechaAlta DEFAULT (GETDATE()),
        Activo       BIT          NOT NULL CONSTRAINT DF_Clientes_Activo    DEFAULT (1),
        CONSTRAINT PK_Clientes PRIMARY KEY (DNI)
    );
END
GO

/* ---------- COMPONENTES ---------- */
IF OBJECT_ID('dbo.Componentes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Componentes
    (
        Id            INT           IDENTITY(1,1) NOT NULL,
        Codigo        VARCHAR(30)   NOT NULL,     -- SKU único
        Nombre        VARCHAR(120)  NOT NULL,
        Descripcion   VARCHAR(500)  NULL,
        Categoria     VARCHAR(80)   NULL,
        Precio        DECIMAL(12,2) NOT NULL CONSTRAINT CK_Componentes_Precio       CHECK (Precio      >= 0),
        StockActual   INT           NOT NULL CONSTRAINT DF_Componentes_StockActual  DEFAULT (0)
                                             CONSTRAINT CK_Componentes_StockActual  CHECK (StockActual >= 0),
        StockMinimo   INT           NOT NULL CONSTRAINT DF_Componentes_StockMinimo  DEFAULT (0)
                                             CONSTRAINT CK_Componentes_StockMinimo  CHECK (StockMinimo >= 0),
        Activo        BIT           NOT NULL CONSTRAINT DF_Componentes_Activo       DEFAULT (1),
        CONSTRAINT PK_Componentes         PRIMARY KEY (Id),
        CONSTRAINT UQ_Componentes_Codigo  UNIQUE      (Codigo)
    );
END
GO

/* ---------- CARRITO ---------- */
IF OBJECT_ID('dbo.Carrito', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carrito
    (
        Id             INT          IDENTITY(1,1) NOT NULL,
        DniCliente     VARCHAR(15)  NOT NULL,
        LoginVendedor  NVARCHAR(250) NOT NULL,    -- UserName del vendedor logueado (debe coincidir con dbo.Usuario.UserName)
        FechaCreacion  DATETIME     NOT NULL CONSTRAINT DF_Carrito_FechaCreacion DEFAULT (GETDATE()),
        Estado         VARCHAR(20)  NOT NULL CONSTRAINT DF_Carrito_Estado       DEFAULT ('Confirmado')
                                             CONSTRAINT CK_Carrito_Estado
                                             CHECK (Estado IN ('Abierto','Confirmado','Cancelado')),
        Total          DECIMAL(12,2) NOT NULL CONSTRAINT DF_Carrito_Total       DEFAULT (0),
        CONSTRAINT PK_Carrito              PRIMARY KEY (Id),
        CONSTRAINT FK_Carrito_Cliente      FOREIGN KEY (DniCliente)    REFERENCES dbo.Clientes(DNI),
        CONSTRAINT FK_Carrito_Vendedor     FOREIGN KEY (LoginVendedor) REFERENCES dbo.Usuario(UserName)
    );
END
GO

/* ---------- LINEA_CARRITO ---------- */
IF OBJECT_ID('dbo.LineaCarrito', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LineaCarrito
    (
        Id              INT           IDENTITY(1,1) NOT NULL,
        IdCarrito       INT           NOT NULL,
        IdComponente    INT           NOT NULL,
        Cantidad        INT           NOT NULL CONSTRAINT CK_LineaCarrito_Cantidad CHECK (Cantidad > 0),
        PrecioUnitario  DECIMAL(12,2) NOT NULL CONSTRAINT CK_LineaCarrito_Precio   CHECK (PrecioUnitario >= 0),
        CONSTRAINT PK_LineaCarrito             PRIMARY KEY (Id),
        CONSTRAINT FK_LineaCarrito_Carrito     FOREIGN KEY (IdCarrito)    REFERENCES dbo.Carrito(Id)      ON DELETE CASCADE,
        CONSTRAINT FK_LineaCarrito_Componente  FOREIGN KEY (IdComponente) REFERENCES dbo.Componentes(Id)
    );
END
GO

/* ================================================================
   SECCION 2 · MODULO / TIPO EVENTO (para bitácora)
   ================================================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.Modulo WHERE Nombre = 'Cliente')
    INSERT INTO dbo.Modulo (Nombre) VALUES ('Cliente');

IF NOT EXISTS (SELECT 1 FROM dbo.Modulo WHERE Nombre = 'Carrito')
    INSERT INTO dbo.Modulo (Nombre) VALUES ('Carrito');

IF NOT EXISTS (SELECT 1 FROM dbo.Modulo WHERE Nombre = 'Componente')
    INSERT INTO dbo.Modulo (Nombre) VALUES ('Componente');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Cliente registrado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Cliente registrado');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Cliente modificado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Cliente modificado');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Carrito confirmado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Carrito confirmado');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Carrito cancelado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Carrito cancelado');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Componente vendido')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Componente vendido');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'Stock insuficiente')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('Stock insuficiente');

IF NOT EXISTS (SELECT 1 FROM dbo.TipoEvento WHERE Nombre = 'DNI duplicado')
    INSERT INTO dbo.TipoEvento (Nombre) VALUES ('DNI duplicado');
GO

/* ================================================================
   SECCION 3 · STORED PROCEDURES
   ================================================================ */

/* ---------- CLIENTES ---------- */

IF OBJECT_ID('dbo.sp_Cliente_ExisteDNI_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cliente_ExisteDNI_GO44;
GO
CREATE PROCEDURE dbo.sp_Cliente_ExisteDNI_GO44
    @DNI VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(COUNT(1) AS INT) AS Existe
    FROM dbo.Clientes
    WHERE DNI = @DNI;
END
GO

IF OBJECT_ID('dbo.sp_Cliente_BuscarPorDNI_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cliente_BuscarPorDNI_GO44;
GO
CREATE PROCEDURE dbo.sp_Cliente_BuscarPorDNI_GO44
    @DNI VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DNI, Apellido, Nombre, Email, Telefono, FechaAlta, Activo
    FROM dbo.Clientes
    WHERE DNI = @DNI;
END
GO

IF OBJECT_ID('dbo.sp_Cliente_Listar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cliente_Listar_GO44;
GO
CREATE PROCEDURE dbo.sp_Cliente_Listar_GO44
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DNI, Apellido, Nombre, Email, Telefono, FechaAlta, Activo
    FROM dbo.Clientes
    ORDER BY Apellido, Nombre;
END
GO

IF OBJECT_ID('dbo.sp_Cliente_Insertar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cliente_Insertar_GO44;
GO
CREATE PROCEDURE dbo.sp_Cliente_Insertar_GO44
    @DNI      VARCHAR(15),
    @Apellido VARCHAR(80),
    @Nombre   VARCHAR(80),
    @Email    VARCHAR(255),   -- ya viene cifrado desde BLL
    @Telefono VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Clientes (DNI, Apellido, Nombre, Email, Telefono, FechaAlta, Activo)
    VALUES                   (@DNI, @Apellido, @Nombre, @Email, @Telefono, GETDATE(), 1);
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

IF OBJECT_ID('dbo.sp_Cliente_ModificarEmail_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cliente_ModificarEmail_GO44;
GO
CREATE PROCEDURE dbo.sp_Cliente_ModificarEmail_GO44
    @DNI   VARCHAR(15),
    @Email VARCHAR(255)  -- ya cifrado
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Clientes SET Email = @Email WHERE DNI = @DNI;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

IF OBJECT_ID('dbo.sp_Cliente_ActivarDesactivar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Cliente_ActivarDesactivar_GO44;
GO
CREATE PROCEDURE dbo.sp_Cliente_ActivarDesactivar_GO44
    @DNI    VARCHAR(15),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Clientes SET Activo = @Activo WHERE DNI = @DNI;
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

/* ---------- COMPONENTES ---------- */

IF OBJECT_ID('dbo.sp_Componente_ListarActivos_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_ListarActivos_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_ListarActivos_GO44
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo, Activo
    FROM dbo.Componentes
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

IF OBJECT_ID('dbo.sp_Componente_BuscarPorId_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_BuscarPorId_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_BuscarPorId_GO44
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo, Activo
    FROM dbo.Componentes
    WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.sp_Componente_BuscarPorCodigo_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_BuscarPorCodigo_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_BuscarPorCodigo_GO44
    @Codigo VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo, Activo
    FROM dbo.Componentes
    WHERE Codigo = @Codigo;
END
GO

IF OBJECT_ID('dbo.sp_Componente_VerificarStock_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_VerificarStock_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_VerificarStock_GO44
    @Id       INT,
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        CASE WHEN StockActual >= @Cantidad THEN 1 ELSE 0 END AS TieneStock,
        StockActual
    FROM dbo.Componentes
    WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.sp_Componente_DescontarStock_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Componente_DescontarStock_GO44;
GO
CREATE PROCEDURE dbo.sp_Componente_DescontarStock_GO44
    @Id       INT,
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Componentes
    SET    StockActual = StockActual - @Cantidad
    WHERE  Id = @Id
       AND StockActual >= @Cantidad;    -- protege contra stock negativo
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

/* ---------- CARRITO ---------- */

IF OBJECT_ID('dbo.sp_Carrito_Insertar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Carrito_Insertar_GO44;
GO
CREATE PROCEDURE dbo.sp_Carrito_Insertar_GO44
    @DniCliente    VARCHAR(15),
    @LoginVendedor NVARCHAR(250),
    @Total         DECIMAL(12,2),
    @Estado        VARCHAR(20) = 'Confirmado'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Carrito (DniCliente, LoginVendedor, FechaCreacion, Estado, Total)
    VALUES                  (@DniCliente, @LoginVendedor, GETDATE(), @Estado, @Total);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdCarrito;
END
GO

IF OBJECT_ID('dbo.sp_Carrito_ObtenerPorId_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Carrito_ObtenerPorId_GO44;
GO
CREATE PROCEDURE dbo.sp_Carrito_ObtenerPorId_GO44
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, DniCliente, LoginVendedor, FechaCreacion, Estado, Total
    FROM dbo.Carrito
    WHERE Id = @Id;
END
GO

/* ---------- LINEA CARRITO ---------- */

IF OBJECT_ID('dbo.sp_LineaCarrito_Insertar_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LineaCarrito_Insertar_GO44;
GO
CREATE PROCEDURE dbo.sp_LineaCarrito_Insertar_GO44
    @IdCarrito      INT,
    @IdComponente   INT,
    @Cantidad       INT,
    @PrecioUnitario DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LineaCarrito (IdCarrito, IdComponente, Cantidad, PrecioUnitario)
    VALUES                       (@IdCarrito, @IdComponente, @Cantidad, @PrecioUnitario);
    SELECT CAST(@@ROWCOUNT AS INT) AS Filas;
END
GO

IF OBJECT_ID('dbo.sp_LineaCarrito_ListarPorCarrito_GO44', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LineaCarrito_ListarPorCarrito_GO44;
GO
CREATE PROCEDURE dbo.sp_LineaCarrito_ListarPorCarrito_GO44
    @IdCarrito INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        LC.Id,
        LC.IdCarrito,
        LC.IdComponente,
        LC.Cantidad,
        LC.PrecioUnitario,
        C.Codigo,
        C.Nombre AS ComponenteNombre
    FROM   dbo.LineaCarrito LC
    INNER JOIN dbo.Componentes C ON C.Id = LC.IdComponente
    WHERE  LC.IdCarrito = @IdCarrito
    ORDER  BY LC.Id;
END
GO

/* ================================================================
   SECCION 4 · DATOS DE PRUEBA (opcional — comentar en producción)
   ================================================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.Componentes)
BEGIN
    INSERT INTO dbo.Componentes (Codigo, Nombre, Descripcion, Categoria, Precio, StockActual, StockMinimo)
    VALUES
        ('CPU-001', 'Intel Core i5-13600K',    'Procesador 14 nucleos LGA1700',     'Procesador',      285000.00, 15,  3),
        ('CPU-002', 'AMD Ryzen 7 7800X3D',     'Procesador 8 nucleos AM5',          'Procesador',      520000.00, 10,  3),
        ('GPU-001', 'NVIDIA RTX 4070 Super',   'Placa de video 12GB GDDR6X',        'Placa de video',  850000.00,  8,  2),
        ('GPU-002', 'AMD Radeon RX 7800 XT',   'Placa de video 16GB GDDR6',         'Placa de video',  720000.00,  6,  2),
        ('RAM-001', 'Corsair Vengeance 32GB',  'DDR5 6000MHz 2x16GB',               'Memoria RAM',     125000.00, 25,  5),
        ('SSD-001', 'Samsung 990 Pro 1TB',     'NVMe M.2 PCIe 4.0',                 'Almacenamiento',  145000.00, 20,  5),
        ('MB-001',  'ASUS ROG Strix B650-E',   'Motherboard AM5',                   'Motherboard',     380000.00, 12,  3),
        ('PSU-001', 'Corsair RM850x',          'Fuente 850W 80+ Gold Modular',      'Fuente',          185000.00, 18,  4);
END
GO

PRINT '✅ EsquemaNegocio_GO44 aplicado correctamente';
GO
