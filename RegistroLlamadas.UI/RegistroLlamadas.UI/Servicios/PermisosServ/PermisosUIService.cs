using Microsoft.AspNetCore.Http;

namespace RegistroLlamadas.UI.Servicios.PermisosServ
{
    public class PermisosUIService : IPermisosUIService
    {
        private readonly PermisosService _apiPermisos;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HashSet<string> _paginas = new();


        public PermisosUIService(PermisosService apiPermisos, IHttpContextAccessor httpContextAccessor)
        {
            _apiPermisos = apiPermisos;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CargarPermisosAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            if (session == null)
                return;

            int? rolId = session.GetInt32("IdPerfil");

            if (rolId == null)
                return;

            _paginas = await _apiPermisos.ObtenerPaginasPermitidas(rolId.Value);
        }

        public bool PuedeVer(string ruta)
        {
            return _paginas.Contains(ruta);
        }

        public bool PuedeVerGrupo(IEnumerable<string> rutas)
        {
            return rutas.Any(r => _paginas.Contains(r));
        }
    }
}
