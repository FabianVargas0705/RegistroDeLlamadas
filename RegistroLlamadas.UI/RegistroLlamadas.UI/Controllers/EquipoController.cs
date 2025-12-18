using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RegistroLlamadas.UI.Controllers
{
    public class EquipoController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public EquipoController(IHttpClientFactory http, IConfiguration configuration)
        {
            _configuration = configuration;
            _http = http;
        }

        // GET: Equipo/Index - Página principal con menú de opciones
        public ActionResult Index()
        {
            return View();
        }

        // GET: Equipo/ConsultarEquipos - Vista para consultar todos los equipos
        public async Task<ActionResult> ConsultarEquipos()
        {
            var equipos = await ObtenerEquiposAPI();
            return View(equipos ?? new List<EquipoModel>());
        }

        // GET: Equipo/AgregarEquipos - Vista para agregar un nuevo equipo
        public ActionResult AgregarEquipos()
        {
            var catalogos = ObtenerCatalogos();
            ViewBag.Centros = catalogos?.Centros ?? new List<CatalogoItem>();
            ViewBag.Estados = catalogos?.Estados
                    .Where(e => e.TipoEstado == 1)
                    .ToList()
                    ?? new List<CatalogoItem>(); ViewBag.Roles = catalogos?.Roles ?? new List<RolItem>();
            return View(new EquipoModel());
        }

        // POST: Equipo/AgregarEquipos - Procesar el formulario de agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarEquipos(EquipoModel equipo)
        {
            if (!ModelState.IsValid)
            {
                var catalogos = ObtenerCatalogos();
                ViewBag.Centros = catalogos?.Centros ?? new List<CatalogoItem>();
                ViewBag.Estados = catalogos?.Estados ?? new List<CatalogoItem>();
                return View(equipo);
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Equipo/RegistrarEquipo";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, equipo);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Equipo registrado correctamente";
                        return RedirectToAction(nameof(ConsultarEquipos));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al registrar el equipo: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al registrar el equipo: " + ex.Message;
            }

            var catalogosError = ObtenerCatalogos();
            ViewBag.Centros = catalogosError?.Centros ?? new List<CatalogoItem>();
            ViewBag.Estados = catalogosError?.Estados ?? new List<CatalogoItem>();
            return View(equipo);
        }

        // GET: Equipo/ActualizarEquipos/5 - Vista para actualizar un equipo
        public async Task<ActionResult> ActualizarEquipos(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Equipo/ObtenerEquipoPorId/{id}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var equipo = await respuesta.Content.ReadFromJsonAsync<EquipoModel>(options);
                        if (equipo != null)
                        {
                            var catalogos = ObtenerCatalogos();
                            ViewBag.Centros = catalogos?.Centros ?? new List<CatalogoItem>();
                            ViewBag.Estados = catalogos?.Estados ?? new List<CatalogoItem>();
                            
                            return View(equipo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el equipo: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarEquipos));
        }

        // POST: Equipo/ActualizarEquipos - Procesar el formulario de actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEquipos(EquipoModel equipo)
        {
            if (!ModelState.IsValid)
            {
                var catalogos = ObtenerCatalogos();
                ViewBag.Centros = catalogos?.Centros ?? new List<CatalogoItem>();
                ViewBag.Estados = catalogos?.Estados ?? new List<CatalogoItem>();
                return View(equipo);
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Equipo/ActualizarEquipo";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PutAsJsonAsync(urlApi, equipo);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Equipo actualizado correctamente";
                        return RedirectToAction(nameof(ConsultarEquipos));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al actualizar el equipo: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el equipo: " + ex.Message;
            }

            var catalogosError = ObtenerCatalogos();
            ViewBag.Centros = catalogosError?.Centros ?? new List<CatalogoItem>();
            ViewBag.Estados = catalogosError?.Estados ?? new List<CatalogoItem>();
            return View(equipo);
        }

        // GET: Equipo/EliminarEquipos/5 - Vista para confirmar eliminación
        public async Task<ActionResult> EliminarEquipos(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Equipo/ObtenerEquipoPorId/{id}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var equipo = await respuesta.Content.ReadFromJsonAsync<EquipoModel>(options);
                        if (equipo != null)
                        {
                            return View(equipo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el equipo: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarEquipos));
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEquiposPorCentro(int idCentro)
        {
            try
            {
                var equipos = await ObtenerEquiposPorCentroAPI(idCentro);

                return Ok(equipos);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener los equipos del centro: " + ex.Message
                });
            }
        }

        #region Obtener equipos por centro 
        private async Task<List<EquipoModel>> ObtenerEquiposPorCentroAPI(int idCentro)
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + $"Equipo/ObtenerEquiposPorCentro/{idCentro}";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var equipos = await respuesta.Content
                        .ReadFromJsonAsync<List<EquipoModel>>(options);

                    return equipos ?? new List<EquipoModel>();
                }

                return new List<EquipoModel>();
            }
        }
        #endregion


        // POST: Equipo/EliminarEquipos - Procesar la eliminación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEquipos(EquipoModel equipo)
        {
            if (equipo == null || equipo.IdEquipo == 0)
            {
                TempData["ErrorMessage"] = "ID de equipo inválido";
                return RedirectToAction(nameof(ConsultarEquipos));
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Equipo/EliminarEquipo/{equipo.IdEquipo}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.DeleteAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Equipo eliminado correctamente";
                    }
                    else
                    {
                        var errorContent = await respuesta.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al eliminar el equipo: " + errorContent;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el equipo: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarEquipos));
        }

        // Métodos auxiliares
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

        private async Task<List<EquipoModel>> ObtenerEquiposAPI()
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Equipo/ObtenerEquipos";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var equipos = await respuesta.Content.ReadFromJsonAsync<List<EquipoModel>>(options);
                    return equipos ?? new List<EquipoModel>();
                }

                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEquipoPorId(int idEquipo)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Equipo/ObtenerEquipoPorId/{idEquipo}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var equipo = await respuesta.Content.ReadFromJsonAsync<EquipoModel>(options);
                        return Ok(new { success = true, data = equipo });
                    }

                    return BadRequest(new { success = false, mensaje = "Error al obtener el equipo" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarEquipo([FromBody] EquipoModel equipo)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Equipo/RegistrarEquipo";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, equipo);

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
                return BadRequest(new { success = false, mensaje = "Error al registrar el equipo: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEquipo([FromBody] EquipoModel equipo)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Equipo/ActualizarEquipo";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PutAsJsonAsync(urlApi, equipo);

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
                return BadRequest(new { success = false, mensaje = "Error al actualizar el equipo: " + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarEquipo(int idEquipo)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Equipo/EliminarEquipo/{idEquipo}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.DeleteAsync(urlApi);

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
                return BadRequest(new { success = false, mensaje = "Error al eliminar el equipo: " + ex.Message });
            }
        }
    }
}
