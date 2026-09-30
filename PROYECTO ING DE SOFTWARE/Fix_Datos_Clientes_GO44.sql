/* ================================================================
   Fix_Datos_Clientes_GO44.sql
   Inserta clientes de prueba (los mismos que tenés vos).
   Idempotente: si ya existen los saltea.
   ================================================================ */
USE [Gestion Usuario]
GO

-- Cliente 1
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE DNI = '11111111')
INSERT [dbo].[Clientes] ([DNI], [Apellido], [Nombre], [Email], [Telefono], [FechaAlta], [Activo])
VALUES (N'11111111', N'Aguila', N'Benjamin', N'fY8CIaC/HVREaYTkBoBbUjgRqC+VWfLNkbKXUMznAPk=', N'1159024801', GETDATE(), 1);

-- Cliente 2
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE DNI = '11111112')
INSERT [dbo].[Clientes] ([DNI], [Apellido], [Nombre], [Email], [Telefono], [FechaAlta], [Activo])
VALUES (N'11111112', N'gomez', N'facundo', N'MG8vU0kFL5SuOrGqF1sFg8up/oWsLMT2CMYFnZPpJKI=', N'1159024802', GETDATE(), 1);

-- Cliente 3
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE DNI = '46947544')
INSERT [dbo].[Clientes] ([DNI], [Apellido], [Nombre], [Email], [Telefono], [FechaAlta], [Activo])
VALUES (N'46947544', N'Gomez', N'Facundo', N'lL0mLuvz+LnFmf8aQXx1SeViOh23+tshp125YsVMLUU=', N'1159024801', GETDATE(), 1);

-- Cliente 4
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE DNI = '46948668')
INSERT [dbo].[Clientes] ([DNI], [Apellido], [Nombre], [Email], [Telefono], [FechaAlta], [Activo])
VALUES (N'46948668', N'kirichuk', N'Maximo', N'E5yu/HPCQeflwDGR0mAKBVTNaJrUrGZowcairGq7q/w=', N'1166607667', GETDATE(), 1);

PRINT '✅ Clientes insertados/verificados';
SELECT DNI, Apellido, Nombre, Telefono, Activo FROM dbo.Clientes ORDER BY Apellido;
GO
