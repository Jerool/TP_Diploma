/* ================================================================
   DER_GestionUsuario_EA.js
   Script para Enterprise Architect que genera el DER completo
   de la base "Gestion Usuario" (23 tablas + 25 FKs).

   CÓMO USARLO:
   1. Abrí Enterprise Architect con tu proyecto.
   2. En el Project Browser (izquierda), hacé click sobre el package
      DONDE QUERÉS QUE SE CREE EL DER (ej: "DER" o un package nuevo).
   3. Menú superior: Specialize → Tools → Scripting
      (en versiones nuevas: Script → Scripting)
   4. En el panel Scripting, botón derecho → New Group → nombre: "DER"
   5. Sobre el grupo "DER", botón derecho → New Script → Normal Script
      → tipo: JavaScript o JScript → nombre: "Generar DER BD"
   6. Pegá TODO este archivo en el editor.
   7. Botón "Run Script" (flecha verde arriba).
   8. Esperá ~10 segundos. Al terminar, abrí el diagrama "DER - Gestion Usuario".
   ================================================================ */

var DIAGRAM_NAME = "DER - Gestion Usuario";

!INC Local Scripts.EAConstants-JScript

function main()
{
    Repository.EnsureOutputVisible("Script");
    Session.Output("=== Generando DER Gestion Usuario ===");

    var selectedPackage = Repository.GetTreeSelectedPackage();
    if (selectedPackage == null)
    {
        Session.Prompt("Primero seleccioná un package en el Project Browser.", promptOK);
        return;
    }
    Session.Output("Package destino: " + selectedPackage.Name);

    // 1. Crear diagrama
    var diagram = selectedPackage.Diagrams.AddNew(DIAGRAM_NAME, "Class");
    diagram.Update();
    selectedPackage.Diagrams.Refresh();
    Session.Output("Diagrama creado: " + diagram.Name);

    // 2. Crear elementos (tablas)
    var tablas = definirTablas();
    var elementosPorTabla = {};
    var x = 50;
    var y = 50;
    var colIndex = 0;
    var maxCols = 4;
    var dx = 240;
    var dy = 220;

    for (var i = 0; i < tablas.length; i++)
    {
        var t = tablas[i];
        var elem = selectedPackage.Elements.AddNew(t.nombre, "Class");
        elem.Stereotype = "table";
        elem.Update();

        // Agregar atributos
        for (var j = 0; j < t.columnas.length; j++)
        {
            var col = t.columnas[j];
            var attr = elem.Attributes.AddNew(col.nombre, col.tipo);
            attr.IsStatic = false;
            attr.Visibility = "Public";
            if (col.esPK)
            {
                attr.Stereotype = "PK";
                attr.Notes = "Primary Key";
            }
            else if (col.esFK)
            {
                attr.Stereotype = "FK";
                attr.Notes = "Foreign Key -> " + col.fkTarget;
            }
            attr.Update();
        }
        elem.Attributes.Refresh();

        elementosPorTabla[t.nombre] = elem;

        // Agregar al diagrama
        var diagObj = diagram.DiagramObjects.AddNew("", "");
        diagObj.ElementID = elem.ElementID;
        diagObj.left = x;
        diagObj.top = -y;
        diagObj.right = x + 200;
        diagObj.bottom = -(y + 170);
        diagObj.Update();

        colIndex++;
        if (colIndex >= maxCols)
        {
            colIndex = 0;
            x = 50;
            y += dy;
        }
        else
        {
            x += dx;
        }

        Session.Output("  [" + (i+1) + "/" + tablas.length + "] Tabla: " + t.nombre + " (" + t.columnas.length + " columnas)");
    }

    // 3. Crear FKs (relaciones)
    var fks = definirFKs();
    for (var k = 0; k < fks.length; k++)
    {
        var fk = fks[k];
        var elemHija = elementosPorTabla[fk.tablaHija];
        var elemPadre = elementosPorTabla[fk.tablaPadre];
        if (elemHija == null || elemPadre == null)
        {
            Session.Output("  [SKIP] FK " + fk.nombre + ": tabla no encontrada");
            continue;
        }
        var conn = elemHija.Connectors.AddNew(fk.nombre, "Association");
        conn.SupplierID = elemPadre.ElementID;
        conn.Direction = "Source -> Destination";
        conn.ClientEnd.Cardinality = "0..*";
        conn.SupplierEnd.Cardinality = "1";
        conn.ClientEnd.Role = fk.columnaFK;
        conn.Update();
        Session.Output("  FK: " + fk.tablaHija + "." + fk.columnaFK + " -> " + fk.tablaPadre);
    }

    diagram.DiagramObjects.Refresh();
    diagram.DiagramLinks.Refresh();
    Repository.ReloadDiagram(diagram.DiagramID);
    Repository.OpenDiagram(diagram.DiagramID);

    Session.Output("=== DER generado OK ===");
    Session.Output("Total tablas: " + tablas.length);
    Session.Output("Total FKs: " + fks.length);
}

// ============================================================
// DEFINICIÓN DE TABLAS Y COLUMNAS
// ============================================================
function definirTablas()
{
    return [
        // --- Seguridad / Auditoría ---
        { nombre: "Usuario", columnas: [
            { nombre: "DNI",                 tipo: "nvarchar(250)", esPK: true  },
            { nombre: "Apellido",            tipo: "nvarchar(250)" },
            { nombre: "Nombre",              tipo: "nvarchar(250)" },
            { nombre: "UserName",            tipo: "nvarchar(250)" },
            { nombre: "Contrasena",          tipo: "nvarchar(250)" },
            { nombre: "Email",               tipo: "nvarchar(250)" },
            { nombre: "Bloqueo",             tipo: "bit" },
            { nombre: "Activo",              tipo: "bit" },
            { nombre: "IdRol",               tipo: "int",           esFK: true, fkTarget: "Roles.Id" },
            { nombre: "IntentosFallidos",    tipo: "int" },
            { nombre: "DebeCambiarContrasena", tipo: "bit" },
            { nombre: "Idioma",              tipo: "nvarchar(5)" }
        ]},
        { nombre: "Roles", columnas: [
            { nombre: "Id",     tipo: "int",            esPK: true },
            { nombre: "Nombre", tipo: "nvarchar(50)" }
        ]},
        { nombre: "Familia", columnas: [
            { nombre: "Id",     tipo: "int",            esPK: true },
            { nombre: "Nombre", tipo: "nvarchar(100)" }
        ]},
        { nombre: "Patente", columnas: [
            { nombre: "Id",      tipo: "int",            esPK: true },
            { nombre: "Nombre",  tipo: "nvarchar(100)" },
            { nombre: "DataKey", tipo: "nvarchar(100)" }
        ]},
        { nombre: "RolFamilia", columnas: [
            { nombre: "IdRol",     tipo: "int", esPK: true, esFK: true, fkTarget: "Roles.Id" },
            { nombre: "IdFamilia", tipo: "int", esPK: true, esFK: true, fkTarget: "Familia.Id" }
        ]},
        { nombre: "RolPatente", columnas: [
            { nombre: "IdRol",     tipo: "int", esPK: true, esFK: true, fkTarget: "Roles.Id" },
            { nombre: "IdPatente", tipo: "int", esPK: true, esFK: true, fkTarget: "Patente.Id" }
        ]},
        { nombre: "FamiliaPatente", columnas: [
            { nombre: "IdFamilia", tipo: "int", esPK: true, esFK: true, fkTarget: "Familia.Id" },
            { nombre: "IdPatente", tipo: "int", esPK: true, esFK: true, fkTarget: "Patente.Id" }
        ]},
        { nombre: "FamiliaIntegrada", columnas: [
            { nombre: "IdFamiliaPadre", tipo: "int", esPK: true, esFK: true, fkTarget: "Familia.Id" },
            { nombre: "IdFamiliaHija",  tipo: "int", esPK: true, esFK: true, fkTarget: "Familia.Id" }
        ]},
        { nombre: "Modulo", columnas: [
            { nombre: "Id",     tipo: "int",           esPK: true },
            { nombre: "Nombre", tipo: "nvarchar(50)" }
        ]},
        { nombre: "TipoEvento", columnas: [
            { nombre: "Id",     tipo: "int",            esPK: true },
            { nombre: "Nombre", tipo: "nvarchar(100)" }
        ]},
        { nombre: "EVENTOS", columnas: [
            { nombre: "Id_Evento",    tipo: "int",            esPK: true },
            { nombre: "UserName",     tipo: "nvarchar(250)",  esFK: true, fkTarget: "Usuario.UserName" },
            { nombre: "Criticidad",   tipo: "nvarchar(50)" },
            { nombre: "FechaHora",    tipo: "datetime" },
            { nombre: "IdModulo",     tipo: "int",            esFK: true, fkTarget: "Modulo.Id" },
            { nombre: "IdTipoEvento", tipo: "int",            esFK: true, fkTarget: "TipoEvento.Id" },
            { nombre: "Detalle",      tipo: "nvarchar(max)" }
        ]},
        { nombre: "IntegridadDVH", columnas: [
            { nombre: "NombreTabla",  tipo: "nvarchar(50)", esPK: true },
            { nombre: "IdRegistro",   tipo: "nvarchar(50)", esPK: true },
            { nombre: "DVH",          tipo: "nvarchar(64)" },
            { nombre: "FechaCalculo", tipo: "datetime" }
        ]},
        { nombre: "IntegridadDVV", columnas: [
            { nombre: "NombreTabla",  tipo: "nvarchar(50)", esPK: true },
            { nombre: "DVV",          tipo: "nvarchar(64)" },
            { nombre: "FechaCalculo", tipo: "datetime" }
        ]},

        // --- Negocio ---
        { nombre: "Clientes", columnas: [
            { nombre: "DNI",       tipo: "varchar(15)",  esPK: true },
            { nombre: "Apellido",  tipo: "varchar(80)" },
            { nombre: "Nombre",    tipo: "varchar(80)" },
            { nombre: "Email",     tipo: "varchar(255)" },
            { nombre: "Telefono",  tipo: "varchar(30)" },
            { nombre: "FechaAlta", tipo: "datetime" },
            { nombre: "Activo",    tipo: "bit" }
        ]},
        { nombre: "Componentes", columnas: [
            { nombre: "Id",          tipo: "int",            esPK: true },
            { nombre: "Codigo",      tipo: "varchar(30)" },
            { nombre: "Nombre",      tipo: "varchar(120)" },
            { nombre: "Descripcion", tipo: "varchar(500)" },
            { nombre: "Categoria",   tipo: "varchar(80)" },
            { nombre: "Precio",      tipo: "decimal(12,2)" },
            { nombre: "StockActual", tipo: "int" },
            { nombre: "StockMinimo", tipo: "int" },
            { nombre: "Activo",      tipo: "bit" }
        ]},
        { nombre: "Carrito", columnas: [
            { nombre: "Id",            tipo: "int",            esPK: true },
            { nombre: "DniCliente",    tipo: "varchar(15)",    esFK: true, fkTarget: "Clientes.DNI" },
            { nombre: "LoginVendedor", tipo: "nvarchar(250)",  esFK: true, fkTarget: "Usuario.UserName" },
            { nombre: "FechaCreacion", tipo: "datetime" },
            { nombre: "Estado",        tipo: "varchar(20)" },
            { nombre: "Total",         tipo: "decimal(12,2)" }
        ]},
        { nombre: "LineaCarrito", columnas: [
            { nombre: "Id",             tipo: "int",            esPK: true },
            { nombre: "IdCarrito",      tipo: "int",            esFK: true, fkTarget: "Carrito.Id" },
            { nombre: "IdComponente",   tipo: "int",            esFK: true, fkTarget: "Componentes.Id" },
            { nombre: "Cantidad",       tipo: "int" },
            { nombre: "PrecioUnitario", tipo: "decimal(12,2)" }
        ]},

        // --- Facturación ---
        { nombre: "Facturas", columnas: [
            { nombre: "Id",            tipo: "int",            esPK: true },
            { nombre: "NumeroFactura", tipo: "varchar(20)" },
            { nombre: "IdCarrito",     tipo: "int",            esFK: true, fkTarget: "Carrito.Id" },
            { nombre: "DniCliente",    tipo: "varchar(15)",    esFK: true, fkTarget: "Clientes.DNI" },
            { nombre: "LoginCajero",   tipo: "nvarchar(250)",  esFK: true, fkTarget: "Usuario.UserName" },
            { nombre: "FechaEmision",  tipo: "datetime" },
            { nombre: "Subtotal",      tipo: "decimal(12,2)" },
            { nombre: "IVA",           tipo: "decimal(12,2)" },
            { nombre: "Total",         tipo: "decimal(12,2)" },
            { nombre: "Estado",        tipo: "varchar(20)" }
        ]},
        { nombre: "LineaFactura", columnas: [
            { nombre: "Id",             tipo: "int",           esPK: true },
            { nombre: "IdFactura",      tipo: "int",           esFK: true, fkTarget: "Facturas.Id" },
            { nombre: "IdComponente",   tipo: "int",           esFK: true, fkTarget: "Componentes.Id" },
            { nombre: "Cantidad",       tipo: "int" },
            { nombre: "PrecioUnitario", tipo: "decimal(12,2)" },
            { nombre: "Subtotal",       tipo: "decimal(12,2)" }
        ]},
        { nombre: "CobrosFactura", columnas: [
            { nombre: "Id",               tipo: "int",            esPK: true },
            { nombre: "IdFactura",        tipo: "int",            esFK: true, fkTarget: "Facturas.Id" },
            { nombre: "MetodoPago",       tipo: "varchar(20)" },
            { nombre: "Monto",            tipo: "decimal(12,2)" },
            { nombre: "FechaCobro",       tipo: "datetime" },
            { nombre: "NroTarjetaEnmasc", tipo: "varchar(20)" },
            { nombre: "Banco",            tipo: "varchar(80)" },
            { nombre: "TitularNombre",    tipo: "varchar(80)" },
            { nombre: "TitularApellido",  tipo: "varchar(80)" },
            { nombre: "CodigoAutoriz",    tipo: "varchar(30)" }
        ]},

        // --- Compras / Proveedores (RFN2) ---
        { nombre: "Proveedores", columnas: [
            { nombre: "CUIT",      tipo: "varchar(11)",  esPK: true },
            { nombre: "Nombre",    tipo: "varchar(80)" },
            { nombre: "Apellido",  tipo: "varchar(80)" },
            { nombre: "Empresa",   tipo: "varchar(120)" },
            { nombre: "Telefono",  tipo: "varchar(15)" },
            { nombre: "CVU",       tipo: "varchar(22)" },
            { nombre: "Banco",     tipo: "varchar(80)" },
            { nombre: "Titular",   tipo: "varchar(160)" },
            { nombre: "Activo",    tipo: "bit" },
            { nombre: "FechaAlta", tipo: "datetime" }
        ]},
        { nombre: "SolicitudesCotizacion", columnas: [
            { nombre: "Id",             tipo: "int",           esPK: true },
            { nombre: "NumeroSC",       tipo: "AS computed" },
            { nombre: "CuitProveedor",  tipo: "varchar(11)",   esFK: true, fkTarget: "Proveedores.CUIT" },
            { nombre: "LoginEncargado", tipo: "varchar(50)" },
            { nombre: "FechaEmision",   tipo: "datetime" },
            { nombre: "Estado",         tipo: "varchar(20)" }
        ]},
        { nombre: "LineasSolicitudCotizacion", columnas: [
            { nombre: "Id",           tipo: "int", esPK: true },
            { nombre: "IdSolicitud",  tipo: "int", esFK: true, fkTarget: "SolicitudesCotizacion.Id" },
            { nombre: "IdComponente", tipo: "int", esFK: true, fkTarget: "Componentes.Id" },
            { nombre: "Cantidad",     tipo: "int" }
        ]}
    ];
}

// ============================================================
// DEFINICIÓN DE FOREIGN KEYS
// ============================================================
function definirFKs()
{
    return [
        { nombre: "FK_Usuario_Roles",         tablaHija: "Usuario",                   columnaFK: "IdRol",         tablaPadre: "Roles" },
        { nombre: "FK_RolFamilia_Rol",        tablaHija: "RolFamilia",                columnaFK: "IdRol",         tablaPadre: "Roles" },
        { nombre: "FK_RolFamilia_Familia",    tablaHija: "RolFamilia",                columnaFK: "IdFamilia",     tablaPadre: "Familia" },
        { nombre: "FK_RolPatente_Rol",        tablaHija: "RolPatente",                columnaFK: "IdRol",         tablaPadre: "Roles" },
        { nombre: "FK_RolPatente_Patente",    tablaHija: "RolPatente",                columnaFK: "IdPatente",     tablaPadre: "Patente" },
        { nombre: "FK_FamiliaPatente_Familia",tablaHija: "FamiliaPatente",            columnaFK: "IdFamilia",     tablaPadre: "Familia" },
        { nombre: "FK_FamiliaPatente_Patente",tablaHija: "FamiliaPatente",            columnaFK: "IdPatente",     tablaPadre: "Patente" },
        { nombre: "FK_FamIntegrada_Padre",    tablaHija: "FamiliaIntegrada",          columnaFK: "IdFamiliaPadre",tablaPadre: "Familia" },
        { nombre: "FK_FamIntegrada_Hija",     tablaHija: "FamiliaIntegrada",          columnaFK: "IdFamiliaHija", tablaPadre: "Familia" },
        { nombre: "FK_Eventos_Usuario",       tablaHija: "EVENTOS",                   columnaFK: "UserName",      tablaPadre: "Usuario" },
        { nombre: "FK_Eventos_Modulo",        tablaHija: "EVENTOS",                   columnaFK: "IdModulo",      tablaPadre: "Modulo" },
        { nombre: "FK_Eventos_TipoEvento",    tablaHija: "EVENTOS",                   columnaFK: "IdTipoEvento",  tablaPadre: "TipoEvento" },
        { nombre: "FK_Carrito_Cliente",       tablaHija: "Carrito",                   columnaFK: "DniCliente",    tablaPadre: "Clientes" },
        { nombre: "FK_Carrito_Vendedor",      tablaHija: "Carrito",                   columnaFK: "LoginVendedor", tablaPadre: "Usuario" },
        { nombre: "FK_LineaCarrito_Carrito",  tablaHija: "LineaCarrito",              columnaFK: "IdCarrito",     tablaPadre: "Carrito" },
        { nombre: "FK_LineaCarrito_Componente",tablaHija: "LineaCarrito",             columnaFK: "IdComponente",  tablaPadre: "Componentes" },
        { nombre: "FK_Facturas_Carrito",      tablaHija: "Facturas",                  columnaFK: "IdCarrito",     tablaPadre: "Carrito" },
        { nombre: "FK_Facturas_Cliente",      tablaHija: "Facturas",                  columnaFK: "DniCliente",    tablaPadre: "Clientes" },
        { nombre: "FK_Facturas_Cajero",       tablaHija: "Facturas",                  columnaFK: "LoginCajero",   tablaPadre: "Usuario" },
        { nombre: "FK_LineaFactura_Factura",  tablaHija: "LineaFactura",              columnaFK: "IdFactura",     tablaPadre: "Facturas" },
        { nombre: "FK_LineaFactura_Componente",tablaHija: "LineaFactura",             columnaFK: "IdComponente",  tablaPadre: "Componentes" },
        { nombre: "FK_CobrosFactura_Factura", tablaHija: "CobrosFactura",             columnaFK: "IdFactura",     tablaPadre: "Facturas" },
        { nombre: "FK_SC_Proveedor",          tablaHija: "SolicitudesCotizacion",     columnaFK: "CuitProveedor", tablaPadre: "Proveedores" },
        { nombre: "FK_LSC_Solicitud",         tablaHija: "LineasSolicitudCotizacion", columnaFK: "IdSolicitud",   tablaPadre: "SolicitudesCotizacion" },
        { nombre: "FK_LSC_Componente",        tablaHija: "LineasSolicitudCotizacion", columnaFK: "IdComponente",  tablaPadre: "Componentes" }
    ];
}

main();
