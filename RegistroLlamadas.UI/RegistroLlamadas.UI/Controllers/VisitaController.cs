using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RegistroLlamadas.UI.Controllers
{
    public class VisitaController : Controller
{

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;

        public VisitaController(IConfiguration configuration, IHttpClientFactory http)
        {
            _configuration = configuration;

            _http = http;
        }
        // GET: VisitaController
        public async Task<ActionResult> TodasLasVisitasAsync(DateTime? fecha = null)
        {
            var catalogos = ObtenerCatalogos();
            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View("Visitas");
            }

            var visitas = await ObtenerVisitasAPI(0, fecha,0);
            if (visitas == null)
            {
                ViewBag.Error = "No se pudieron cargar las visitas.";
                visitas = new List<VisitaModel>();
            }
            var modelo = new VisitasViewModel
            {
                Visitas = visitas,
                Usuarios = catalogos.Usuarios,
            };

            ViewBag.UsuarioIdActual = HttpContext.Session.GetInt32("ConsecutivoUsuario");
            ViewBag.RolActual = HttpContext.Session.GetInt32("IdPerfil");
            ViewBag.Titulo = "Todas las Visitas";
            return View("Visitas", modelo);
        }


        public async Task<ActionResult> MisVisitas(DateTime? fecha = null)
        {
            var catalogos = ObtenerCatalogos();
            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View("Visitas");
            }
            var usuarioId = HttpContext.Session.GetInt32("ConsecutivoUsuario");
            var visitas = await ObtenerVisitasAPI(0, fecha, usuarioId);
            if (visitas == null)
            {
                ViewBag.Error = "No se pudieron cargar las visitas.";
                visitas = new List<VisitaModel>();
            }
            var modelo = new VisitasViewModel
            {
                Visitas = visitas,
                Usuarios = catalogos.Usuarios,
            };

            ViewBag.UsuarioIdActual = HttpContext.Session.GetInt32("ConsecutivoUsuario");
            ViewBag.RolActual = HttpContext.Session.GetInt32("IdPerfil");
            ViewBag.Titulo = "Mis visitas";
            return View("Visitas", modelo);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarVisita([FromBody] FinalizarVisitaRequest modelo)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Visita/finalizarVisita";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, modelo);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var resultado = await respuesta.Content.ReadFromJsonAsync<dynamic>(options);
                        return Ok(resultado);
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, mensaje = errorContent });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error al finalizar la Visita: " + ex.Message });
            }
        }

        private async Task<List<VisitaModel>> ObtenerVisitasAPI(int idVisita = 0, DateTime? fecha = null,int? idUsuario = 0)
        {
            using (var client = _http.CreateClient())
            {
                var request = new RequestObtenerVisita
                {
                    IdVisita = idVisita,
                    Fecha = fecha ?? DateTime.Now,
                    UsuarioId = idUsuario
                };

                var urlApi = _configuration["Valores:UrlAPI"] + "Visita/ObtenerVisitas";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.PostAsJsonAsync(urlApi, request);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var visitas = await respuesta.Content.ReadFromJsonAsync<List<VisitaModel>>(options);
                    return visitas ?? new List<VisitaModel>();
                }

                return null;
            }
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
        // GET: VisitaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: VisitaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: VisitaController/Create
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

        // GET: VisitaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: VisitaController/Edit/5
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

        // GET: VisitaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: VisitaController/Delete/5
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
