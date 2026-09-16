using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{

    public class BLLPermisos_GO44
    {
        private readonly DALPatente_GO44 _dalPatente;
        private readonly DALFamilia_GO44 _dalFamilia;
        private readonly DALRol_GO44 _dalRol;
        private readonly BLLIntegridad_GO44 _bllIntegridad;

        public BLLPermisos_GO44()
        {
            _dalPatente = new DALPatente_GO44();
            _dalFamilia = new DALFamilia_GO44();
            _dalRol = new DALRol_GO44();
            _bllIntegridad = new BLLIntegridad_GO44();
        }

        private void RecalcularFamilias()
        {
            if (BLLIntegridad_GO44.IntegridadConocidamenteRota) return;
            try
            {
                _bllIntegridad.RecalcularTabla("Familia");
                _bllIntegridad.RecalcularTabla("FamiliaPatente");
                _bllIntegridad.RecalcularTabla("FamiliaIntegrada");
            } catch { }
        }

        private void RecalcularRoles()
        {
            if (BLLIntegridad_GO44.IntegridadConocidamenteRota) return;
            try
            {
                _bllIntegridad.RecalcularTabla("Roles");
                _bllIntegridad.RecalcularTabla("RolPatente");
                _bllIntegridad.RecalcularTabla("RolFamilia");
            } catch { }
        }

        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                var actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = actual != null ? actual.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Admin", tipoEvento, detalle, criticidad);
            }
            catch { }
        }

        public List<Patente_GO44> ListarPatentes() => _dalPatente.ListarTodas();
        public List<Familia_GO44> ListarFamilias() => _dalFamilia.ListarTodasPlanas();
        public List<Rol_GO44> ListarRoles()      => _dalRol.ListarTodos();

        public Familia_GO44 ObtenerArbolFamilia(int idFamilia) => _dalFamilia.ObtenerArbol(idFamilia);
        public Rol_GO44     ObtenerArbolRol(int idRol)         => _dalRol.ObtenerArbol(idRol);

        public int CrearFamilia(string nombre, List<int> idsPatentes, List<int> idsSubfamilias)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception(IdiomaManager_GO44.T("err.nombreFamiliaObligatorio"));

            if (_dalFamilia.ListarTodasPlanas().Any(f => f.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.familiaNombreDuplicado"), nombre));

            idsPatentes    = (idsPatentes ?? new List<int>()).Distinct().OrderBy(i => i).ToList();
            idsSubfamilias = (idsSubfamilias ?? new List<int>()).Distinct().OrderBy(i => i).ToList();

            if (idsPatentes.Count == 0 && idsSubfamilias.Count == 0)
                throw new Exception(IdiomaManager_GO44.T("err.familiaSinContenido"));

            ValidarRedundanciaPatentesYSubfamilias(idsPatentes, idsSubfamilias);

            HashSet<int> patentesEfectivasNueva = ConstruirPatentesEfectivas(idsPatentes, idsSubfamilias);
            Familia_GO44 equivalente = BuscarFamiliaConMismasPatentesEfectivas(patentesEfectivasNueva);
            if (equivalente != null)
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.familiaComposicionDuplicada"), equivalente.Nombre));

            int idCreada = _dalFamilia.Crear(nombre, idsPatentes, idsSubfamilias);
            Auditar("Familia creada",
                    $"Nombre: '{nombre}', Patentes: {idsPatentes.Count}, Subfamilias: {idsSubfamilias.Count}",
                    "Media");
            RecalcularFamilias();
            return idCreada;
        }

        private void ValidarRedundanciaPatentesYSubfamilias(List<int> idsPatentes, List<int> idsSubfamilias)
        {
            // ── (a) Patente directa que ya viene por alguna subfamilia ──
            if (idsPatentes.Count > 0 && idsSubfamilias.Count > 0)
            {
                foreach (int idSub in idsSubfamilias)
                {
                    Familia_GO44 arbol = _dalFamilia.ObtenerArbol(idSub);
                    if (arbol == null) continue;

                    HashSet<int> patentesDeLaSub = new HashSet<int>(arbol.ObtenerPatentes().Select(p => p.Id));

                    foreach (int idPat in idsPatentes)
                    {
                        if (patentesDeLaSub.Contains(idPat))
                        {
                            Patente_GO44 pat = _dalPatente.BuscarPorId(idPat);
                            string nombrePat = pat != null ? pat.Nombre : $"Id {idPat}";
                            throw new Exception(string.Format(
                                IdiomaManager_GO44.T("err.redundanciaPatenteEnSub"), nombrePat, arbol.Nombre));
                        }
                    }
                }
            }

            // ── (b) Dos subfamilias hermanas comparten alguna patente ──
            if (idsSubfamilias.Count >= 2)
            {
                var efectivasPorSub = new Dictionary<int, HashSet<int>>();
                var nombresPorSub = new Dictionary<int, string>();
                foreach (int idSub in idsSubfamilias)
                {
                    Familia_GO44 arbol = _dalFamilia.ObtenerArbol(idSub);
                    if (arbol == null) continue;
                    efectivasPorSub[idSub] = new HashSet<int>(arbol.ObtenerPatentes().Select(p => p.Id));
                    nombresPorSub[idSub] = arbol.Nombre;
                }

                var subs = efectivasPorSub.Keys.ToList();
                for (int i = 0; i < subs.Count; i++)
                {
                    for (int j = i + 1; j < subs.Count; j++)
                    {
                        var solapadas = efectivasPorSub[subs[i]].Intersect(efectivasPorSub[subs[j]]).ToList();
                        if (solapadas.Any())
                        {
                            Patente_GO44 pat = _dalPatente.BuscarPorId(solapadas.First());
                            string nombrePat = pat != null ? pat.Nombre : $"Id {solapadas.First()}";
                            throw new Exception(string.Format(
                                IdiomaManager_GO44.T("err.redundanciaSubsCompartenPatente"),
                                nombrePat, nombresPorSub[subs[i]], nombresPorSub[subs[j]]));
                        }
                    }
                }
            }
        }

        private void ValidarRedundanciaEnAscendentesFamilia(int idFamiliaModificada, HashSet<int> nuevasEfectivas)
        {
            foreach (var famAncestro in _dalFamilia.ListarTodasPlanas())
            {
                if (famAncestro.Id == idFamiliaModificada) continue;

                Familia_GO44 arbolAncestro = _dalFamilia.ObtenerArbol(famAncestro.Id);
                if (arbolAncestro == null) continue;

                if (!ContieneFamiliaRec(arbolAncestro, idFamiliaModificada)) continue;

                // (a) Patentes directas del ancestro que se solapan con las nuevas efectivas.
                var patentesDirectas = _dalFamilia.IdsPatentesDirectas(famAncestro.Id);
                var solapDirectas = patentesDirectas.Intersect(nuevasEfectivas).ToList();
                if (solapDirectas.Any())
                {
                    Patente_GO44 pat = _dalPatente.BuscarPorId(solapDirectas.First());
                    string nombrePat = pat != null ? pat.Nombre : $"Id {solapDirectas.First()}";
                    throw new Exception(string.Format(
                        IdiomaManager_GO44.T("err.redundanciaAscendenteDirecta"), nombrePat, famAncestro.Nombre));
                }

                // (b) Patentes que aporta otra subfamilia hermana del ancestro, solapadas con las nuevas efectivas.
                var subsDelAncestro = _dalFamilia.IdsSubfamiliasDirectas(famAncestro.Id);
                foreach (int idHermana in subsDelAncestro)
                {
                    if (idHermana == idFamiliaModificada) continue;
                    Familia_GO44 arbolHermana = _dalFamilia.ObtenerArbol(idHermana);
                    if (arbolHermana == null) continue;
                    if (ContieneFamiliaRec(arbolHermana, idFamiliaModificada)) continue;

                    HashSet<int> efectivasHermana = new HashSet<int>(arbolHermana.ObtenerPatentes().Select(p => p.Id));
                    var solapHermana = efectivasHermana.Intersect(nuevasEfectivas).ToList();
                    if (solapHermana.Any())
                    {
                        Patente_GO44 pat = _dalPatente.BuscarPorId(solapHermana.First());
                        string nombrePat = pat != null ? pat.Nombre : $"Id {solapHermana.First()}";
                        throw new Exception(string.Format(
                            IdiomaManager_GO44.T("err.redundanciaAscendenteHermana"),
                            nombrePat, famAncestro.Nombre, arbolHermana.Nombre));
                    }
                }
            }

            // ── Mismas dos validaciones pero para roles que contengan la familia ──
            foreach (var rol in _dalRol.ListarTodos())
            {
                Rol_GO44 arbolRol = _dalRol.ObtenerArbol(rol.Id);
                if (arbolRol == null) continue;
                if (!RolContieneFamiliaRec(arbolRol, idFamiliaModificada)) continue;

                var patentesDirectasRol = _dalRol.IdsPatentesDirectas(rol.Id);
                var solapDirectas = patentesDirectasRol.Intersect(nuevasEfectivas).ToList();
                if (solapDirectas.Any())
                {
                    Patente_GO44 pat = _dalPatente.BuscarPorId(solapDirectas.First());
                    string nombrePat = pat != null ? pat.Nombre : $"Id {solapDirectas.First()}";
                    throw new Exception(string.Format(
                        IdiomaManager_GO44.T("err.redundanciaAscendenteRolDirecta"), nombrePat, rol.Nombre));
                }

                var familiasDelRol = _dalRol.IdsFamiliasDirectas(rol.Id);
                foreach (int idFamHermana in familiasDelRol)
                {
                    if (idFamHermana == idFamiliaModificada) continue;
                    Familia_GO44 arbolHermana = _dalFamilia.ObtenerArbol(idFamHermana);
                    if (arbolHermana == null) continue;
                    if (ContieneFamiliaRec(arbolHermana, idFamiliaModificada)) continue;

                    HashSet<int> efectivasHermana = new HashSet<int>(arbolHermana.ObtenerPatentes().Select(p => p.Id));
                    var solapHermana = efectivasHermana.Intersect(nuevasEfectivas).ToList();
                    if (solapHermana.Any())
                    {
                        Patente_GO44 pat = _dalPatente.BuscarPorId(solapHermana.First());
                        string nombrePat = pat != null ? pat.Nombre : $"Id {solapHermana.First()}";
                        throw new Exception(string.Format(
                            IdiomaManager_GO44.T("err.redundanciaAscendenteRolFamilia"),
                            nombrePat, rol.Nombre, arbolHermana.Nombre));
                    }
                }
            }
        }

        private bool RolContieneFamiliaRec(Rol_GO44 rol, int idFamiliaBuscada)
        {
            if (rol == null) return false;
            foreach (var hijo in rol.Hijos)
            {
                if (hijo is Familia_GO44 fam)
                {
                    if (fam.Id == idFamiliaBuscada) return true;
                    if (ContieneFamiliaRec(fam, idFamiliaBuscada)) return true;
                }
            }
            return false;
        }

        private HashSet<int> ConstruirPatentesEfectivas(List<int> idsPatentes, List<int> idsSubfamilias)
        {
            HashSet<int> efectivas = new HashSet<int>(idsPatentes);
            foreach (int idSub in idsSubfamilias)
            {
                Familia_GO44 arbol = _dalFamilia.ObtenerArbol(idSub);
                if (arbol == null) continue;
                foreach (var p in arbol.ObtenerPatentes())
                    efectivas.Add(p.Id);
            }
            return efectivas;
        }

        private Familia_GO44 BuscarFamiliaConMismasPatentesEfectivas(HashSet<int> patentesEfectivas, int excluirId = 0)
        {
            foreach (var fam in _dalFamilia.ListarTodasPlanas())
            {
                if (fam.Id == excluirId) continue;
                Familia_GO44 arbol = _dalFamilia.ObtenerArbol(fam.Id);
                if (arbol == null) continue;
                HashSet<int> efectivas = new HashSet<int>(arbol.ObtenerPatentes().Select(p => p.Id));
                if (efectivas.SetEquals(patentesEfectivas))
                    return fam;
            }
            return null;
        }

        public void EliminarFamilia(int idFamilia)
        {
            List<string> rolesQueLaUsan = _dalFamilia.NombresRolesQueUsan(idFamilia);
            if (rolesQueLaUsan.Count > 0)
            {
                throw new Exception(string.Format(
                    IdiomaManager_GO44.T("err.familiaEnUsoRoles"),
                    rolesQueLaUsan.Count, string.Join(", ", rolesQueLaUsan)));
            }

            List<string> familiasQueLaContienen = _dalFamilia.NombresFamiliasQueLaContienen(idFamilia);
            if (familiasQueLaContienen.Count > 0)
            {
                throw new Exception(string.Format(
                    IdiomaManager_GO44.T("err.familiaEnUsoFamilias"),
                    familiasQueLaContienen.Count, string.Join(", ", familiasQueLaContienen)));
            }

            _dalFamilia.Eliminar(idFamilia);
            Auditar("Familia eliminada", $"IdFamilia: {idFamilia}", "Alta");
            RecalcularFamilias();
        }

        public int CrearRol(string nombre, List<int> idsPatentes, List<int> idsFamilias)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception(IdiomaManager_GO44.T("err.nombreRolObligatorio"));

            if (_dalRol.ListarTodos().Any(r => r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.rolNombreDuplicado"), nombre));

            idsPatentes = (idsPatentes ?? new List<int>()).Distinct().ToList();
            idsFamilias = (idsFamilias ?? new List<int>()).Distinct().ToList();

            if (idsPatentes.Count == 0 && idsFamilias.Count == 0)
                throw new Exception(IdiomaManager_GO44.T("err.rolSinContenido"));

            ValidarRedundanciaPatentesYFamilias(idsPatentes, idsFamilias);

            HashSet<int> efectivasNuevo = ConstruirPatentesEfectivas(idsPatentes, idsFamilias);
            Rol_GO44 equivalente = BuscarRolConMismasPatentesEfectivas(efectivasNuevo);
            if (equivalente != null)
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.rolComposicionDuplicada"), equivalente.Nombre));

            int idCreado = _dalRol.Crear(nombre, idsPatentes, idsFamilias);
            Auditar("Rol creado",
                    $"Nombre: '{nombre}', Patentes: {idsPatentes.Count}, Familias: {idsFamilias.Count}",
                    "Media");
            RecalcularRoles();
            return idCreado;
        }

        private void ValidarRedundanciaPatentesYFamilias(List<int> idsPatentes, List<int> idsFamilias)
        {
            // ── (a) Patente directa que ya viene por alguna familia ──
            if (idsPatentes.Count > 0 && idsFamilias.Count > 0)
            {
                foreach (int idFam in idsFamilias)
                {
                    Familia_GO44 arbol = _dalFamilia.ObtenerArbol(idFam);
                    if (arbol == null) continue;

                    HashSet<int> patentesDeLaFam = new HashSet<int>(arbol.ObtenerPatentes().Select(p => p.Id));

                    foreach (int idPat in idsPatentes)
                    {
                        if (patentesDeLaFam.Contains(idPat))
                        {
                            Patente_GO44 pat = _dalPatente.BuscarPorId(idPat);
                            string nombrePat = pat != null ? pat.Nombre : $"Id {idPat}";
                            throw new Exception(string.Format(
                                IdiomaManager_GO44.T("err.redundanciaPatenteEnFam"), nombrePat, arbol.Nombre));
                        }
                    }
                }
            }

            // ── (b) Dos familias hermanas comparten alguna patente ──
            if (idsFamilias.Count >= 2)
            {
                var efectivasPorFam = new Dictionary<int, HashSet<int>>();
                var nombresPorFam = new Dictionary<int, string>();
                foreach (int idFam in idsFamilias)
                {
                    Familia_GO44 arbol = _dalFamilia.ObtenerArbol(idFam);
                    if (arbol == null) continue;
                    efectivasPorFam[idFam] = new HashSet<int>(arbol.ObtenerPatentes().Select(p => p.Id));
                    nombresPorFam[idFam] = arbol.Nombre;
                }

                var fams = efectivasPorFam.Keys.ToList();
                for (int i = 0; i < fams.Count; i++)
                {
                    for (int j = i + 1; j < fams.Count; j++)
                    {
                        var solapadas = efectivasPorFam[fams[i]].Intersect(efectivasPorFam[fams[j]]).ToList();
                        if (solapadas.Any())
                        {
                            Patente_GO44 pat = _dalPatente.BuscarPorId(solapadas.First());
                            string nombrePat = pat != null ? pat.Nombre : $"Id {solapadas.First()}";
                            throw new Exception(string.Format(
                                IdiomaManager_GO44.T("err.redundanciaFamsCompartenPatente"),
                                nombrePat, nombresPorFam[fams[i]], nombresPorFam[fams[j]]));
                        }
                    }
                }
            }
        }

        private Rol_GO44 BuscarRolConMismasPatentesEfectivas(HashSet<int> patentesEfectivas, int excluirId = 0)
        {
            foreach (var rol in _dalRol.ListarTodos())
            {
                if (rol.Id == excluirId) continue;
                Rol_GO44 arbol = _dalRol.ObtenerArbol(rol.Id);
                if (arbol == null) continue;
                HashSet<int> efectivas = new HashSet<int>(arbol.ObtenerPatentes().Select(p => p.Id));
                if (efectivas.SetEquals(patentesEfectivas))
                    return rol;
            }
            return null;
        }

        public void EliminarRol(int idRol)
        {
            int cantUsuarios = _dalRol.CantidadUsuariosConRol(idRol);
            if (cantUsuarios > 0)
            {
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.rolEnUso"), cantUsuarios));
            }

            _dalRol.Eliminar(idRol);
            Auditar("Rol eliminado", $"IdRol: {idRol}", "Alta");
            RecalcularRoles();
        }

        public bool TienePermiso(int idRol, string dataKey)
        {
            Rol_GO44 rol = _dalRol.ObtenerArbol(idRol);
            return rol != null && rol.TienePermiso(dataKey);
        }

        public void ModificarFamilia(int idFamilia, string nombre, List<int> idsPatentes, List<int> idsSubfamilias)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception(IdiomaManager_GO44.T("err.nombreFamiliaObligatorio"));

            if (_dalFamilia.ListarTodasPlanas()
                .Any(f => f.Id != idFamilia && f.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.familiaOtroNombreDuplicado"), nombre));

            idsPatentes    = (idsPatentes ?? new List<int>()).Distinct().OrderBy(i => i).ToList();
            idsSubfamilias = (idsSubfamilias ?? new List<int>()).Distinct().OrderBy(i => i).ToList();

            if (idsPatentes.Count == 0 && idsSubfamilias.Count == 0)
                throw new Exception(IdiomaManager_GO44.T("err.familiaSinContenido"));

            if (idsSubfamilias.Contains(idFamilia))
                throw new Exception(IdiomaManager_GO44.T("err.familiaCiclo"));

            foreach (int idSub in idsSubfamilias)
            {
                Familia_GO44 arbolSub = _dalFamilia.ObtenerArbol(idSub);
                if (arbolSub == null) continue;
                if (ContieneFamiliaRec(arbolSub, idFamilia))
                    throw new Exception(string.Format(
                        IdiomaManager_GO44.T("err.familiaCicloTransitivo"), arbolSub.Nombre));
            }

            ValidarRedundanciaPatentesYSubfamilias(idsPatentes, idsSubfamilias);

            HashSet<int> patentesEfectivasNueva = ConstruirPatentesEfectivas(idsPatentes, idsSubfamilias);

            ValidarRedundanciaEnAscendentesFamilia(idFamilia, patentesEfectivasNueva);

            Familia_GO44 equivalente = BuscarFamiliaConMismasPatentesEfectivas(patentesEfectivasNueva, excluirId: idFamilia);
            if (equivalente != null)
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.familiaOtraComposicionDuplicada"), equivalente.Nombre));


            if (equivalente != null)
                throw new Exception(
                    $"Ya existe otra familia con la misma composición efectiva de patentes: '{equivalente.Nombre}'.");

            _dalFamilia.Modificar(idFamilia, nombre, idsPatentes, idsSubfamilias);
            Auditar("Familia modificada",
                    $"IdFamilia: {idFamilia}, Nombre: '{nombre}', Patentes: {idsPatentes.Count}, Subfamilias: {idsSubfamilias.Count}",
                    "Media");
            RecalcularFamilias();
        }

        public void ModificarRol(int idRol, string nombre, List<int> idsPatentes, List<int> idsFamilias)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception(IdiomaManager_GO44.T("err.nombreRolObligatorio"));

            if (_dalRol.ListarTodos()
                .Any(r => r.Id != idRol && r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.rolOtroNombreDuplicado"), nombre));

            idsPatentes = (idsPatentes ?? new List<int>()).Distinct().ToList();
            idsFamilias = (idsFamilias ?? new List<int>()).Distinct().ToList();

            if (idsPatentes.Count == 0 && idsFamilias.Count == 0)
                throw new Exception(IdiomaManager_GO44.T("err.rolSinContenido"));

            ValidarRedundanciaPatentesYFamilias(idsPatentes, idsFamilias);

            HashSet<int> efectivasNuevo = ConstruirPatentesEfectivas(idsPatentes, idsFamilias);
            Rol_GO44 equivalente = BuscarRolConMismasPatentesEfectivas(efectivasNuevo, excluirId: idRol);
            if (equivalente != null)
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.rolOtraComposicionDuplicada"), equivalente.Nombre));

            _dalRol.Modificar(idRol, nombre, idsPatentes, idsFamilias);
            Auditar("Rol modificado",
                    $"IdRol: {idRol}, Nombre: '{nombre}', Patentes: {idsPatentes.Count}, Familias: {idsFamilias.Count}",
                    "Media");
            RecalcularRoles();
        }

        private bool ContieneFamiliaRec(Familia_GO44 nodo, int idBuscado)
        {
            if (nodo == null) return false;
            foreach (var hijo in nodo.Hijos)
            {
                if (hijo is Familia_GO44 sub)
                {
                    if (sub.Id == idBuscado) return true;
                    if (ContieneFamiliaRec(sub, idBuscado)) return true;
                }
            }
            return false;
        }
    }
}
