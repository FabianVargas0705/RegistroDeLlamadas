using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RegistroLlamadas.UI.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public ClienteController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        #region Listar clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await ObtenerClientesAPI();
            var catalogos = ObtenerCatalogos();

            ViewBag.Estados = catalogos?.Estados
                .Where(e => e.TipoEstado == 1)
                .ToList();

            return View(clientes);
        }
        #endregion

        private CatalogosDTO ObtenerCatalogos()
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Catalogo/GetCatalogos";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                var respuesta = client.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var catalogos = respuesta.Content
                        .ReadFromJsonAsync<CatalogosDTO>(options).Result;

                    return catalogos!;
                }

                return null;
            }
        }
        private async Task<List<ClienteModel>> ObtenerClientesAPI(int idCliente = 0)
        {
            using var client = _http.CreateClient();

            var urlApi = idCliente > 0
                ? _configuration["Valores:UrlAPI"] + $"Cliente/obtenerClientePorId/{idCliente}"
                : _configuration["Valores:UrlAPI"] + "Cliente/obtenerClientes";

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(urlApi);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (idCliente > 0)
            {
                var c = await resp.Content.ReadFromJsonAsync<ClienteModel>(options);
                return c != null ? new List<ClienteModel> { c } : new();
            }

            return await resp.Content.ReadFromJsonAsync<List<ClienteModel>>(options) ?? new();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCliente([FromBody] ClienteModel cliente)
        {
            return await EjecutarGuardar(cliente, "Cliente/registrarCliente", HttpMethod.Post);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCliente([FromBody] ClienteModel cliente)
        {
            return await EjecutarGuardar(cliente, "Cliente/actualizarCliente", HttpMethod.Put);
        }

        private async Task<IActionResult> EjecutarGuardar(
      ClienteModel cliente,
      string endpoint,
      HttpMethod metodo)
        {
            using var client = _http.CreateClient();
            var urlApi = _configuration["Valores:UrlAPI"] + endpoint;

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage resp;

            if (metodo == HttpMethod.Post)
            {
                resp = await client.PostAsJsonAsync(urlApi, cliente);
            }
            else if (metodo == HttpMethod.Put)
            {
                resp = await client.PutAsJsonAsync(urlApi, cliente);
            }
            else
            {
                return BadRequest("Método HTTP no soportado");
            }

            var content = await resp.Content.ReadAsStringAsync();
            return StatusCode((int)resp.StatusCode, content);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerClientePorId(int idCliente)
        {
            var cliente = (await ObtenerClientesAPI(idCliente)).FirstOrDefault();

            return Ok(new { success = true, data = cliente });
        }
    }
}
