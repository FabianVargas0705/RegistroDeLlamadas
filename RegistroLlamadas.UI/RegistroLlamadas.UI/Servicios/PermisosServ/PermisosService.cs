using RegistroLlamadas.UI.Models.ServiciosM;
using System.Net.Http.Headers;

namespace RegistroLlamadas.UI.Servicios.PermisosServ
{
    public class PermisosService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public PermisosService(HttpClient http, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _http = http;
            _configuration = configuration;
            _httpContextAccessor = accessor;
        }

        public async Task<HashSet<string>> ObtenerPaginasPermitidas(int rolId)
        {
            var baseUrl = _configuration["Valores:UrlAPI"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("❌ La URL del API no está configurada.");


            var url = $"{baseUrl}auth/permisos/{rolId}";

            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            var paginas = await _http.GetFromJsonAsync<List<PaginaModels>>(url);

            return paginas?
                .Select(p => p.Ruta)
                .ToHashSet()
                ?? new HashSet<string>();
        }
    }
}
