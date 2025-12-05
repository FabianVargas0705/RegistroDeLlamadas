using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using RegistroLlamadas.UI.Servicios.PermisosServ;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RegistroLlamadas.UI.Controllers
{
    public class DashboardController : Controller
    {
        // GET: DashboardController
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;
        private readonly IPermisosUIService _permisosUI;

        public DashboardController(IHttpClientFactory http, IConfiguration configuration, IPermisosUIService permisosUIService)
        {
            _http = http;
            _configuration = configuration;
            _permisosUI = permisosUIService;
        }
        public async Task<ActionResult> DashboardView(DateTime? fecha = null)
        {
            var catalogos = ObtenerCatalogos();

            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View();
            }
            var llamadas = await ObtenerLlamadasAPI(0, fecha);
            var visitas = await ObtenerVisitasAPI(fecha);
            if (llamadas == null)
            {
                ViewBag.Error = "No se pudieron cargar las llamadas.";
                llamadas = new List<LlamadaModel>();
            }

            var modelo = new LlamadasViewModel
            {
                Llamadas = llamadas,
                Visitas = visitas,
                Equipos = catalogos.Equipos,
                Centros = catalogos.Centros,
                Usuarios = catalogos.Usuarios,
                Estados = catalogos.Estados,
                Clientes = catalogos.Clientes
            };
            ViewBag.UsuarioIdActual = HttpContext.Session.GetInt32("ConsecutivoUsuario");
            await _permisosUI.CargarPermisosAsync();

            return View(modelo);
        }

        private CatalogosDTO ObtenerCatalogos()
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Catalogo/GetCatalogos";


                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = client.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var catalogos = respuesta.Content.ReadFromJsonAsync<CatalogosDTO>(options).Result;
                    return catalogos!;
                }

                return null;
            }
        }
        private async Task<List<LlamadaModel>> ObtenerLlamadasAPI(int idLlamada = 0, DateTime? fecha = null)
        {
            using (var client = _http.CreateClient())
            {
                var request = new RequestObtenerLlamada
                {
                    IdLlamada = idLlamada,
                    Fecha = fecha ?? DateTime.Now
                };

                var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/obtenerLlamada";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.PostAsJsonAsync(urlApi, request);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var llamadas = await respuesta.Content.ReadFromJsonAsync<List<LlamadaModel>>(options);
                    return llamadas ?? new List<LlamadaModel>();
                }

                return null;
            }
        }


        private async Task<List<VisitaModel>> ObtenerVisitasAPI(DateTime? fecha = null)
        {
            using (var client = _http.CreateClient())
            {
                var request = new RequestObtenerVisita
                {
                    Fecha = fecha ?? DateTime.Now
                };

                var urlApi = _configuration["Valores:UrlAPI"] + "Visita/ObtenerVisitas";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.PostAsJsonAsync(urlApi, request);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var visitas = await respuesta.Content.ReadFromJsonAsync<List<VisitaModel>>(options);
                    return visitas ?? new List<VisitaModel>();
                }

                return new List<VisitaModel>();
            }
        }

        // GET: DashboardController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DashboardController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DashboardController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DashboardController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DashboardController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DashboardController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DashboardController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
