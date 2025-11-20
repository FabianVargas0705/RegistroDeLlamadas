using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RegistroLlamadas.UI.Controllers
{
    public class LlamadaController : Controller
    {
        // GET: LlamadaController
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public LlamadaController(IHttpClientFactory http, IConfiguration configuration)
        {
            _configuration = configuration;
            _http = http;
        }

        public async Task<ActionResult> Llamadas(DateTime? fecha = null)
        {
            var catalogos = ObtenerCatalogos();

            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View();
            }
            var llamadas = await ObtenerLlamadasAPI(0, fecha);
            if (llamadas == null)
            {
                ViewBag.Error = "No se pudieron cargar las llamadas.";
                llamadas = new List<LlamadaModel>();
            }

            var modelo = new LlamadasViewModel
            {
                Llamadas = llamadas,
                Equipos = catalogos.Equipos,
                Centros = catalogos.Centros,
                Usuarios = catalogos.Usuarios,
                Estados = catalogos.Estados.Where(e => e.TipoEstado == 2).ToList(),
                Clientes = catalogos.Clientes
            };
            ViewBag.UsuarioIdActual = HttpContext.Session.GetInt32("ConsecutivoUsuario");

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

        [HttpPost]
        public async Task<IActionResult> RegistrarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/registrarLlamada";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                    var respuesta = await client.PostAsJsonAsync(urlApi, llamada);

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
                return BadRequest(new { success = false, mensaje = "Error al registrar la llamada: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ActualizarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/actualizarLlamada";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, llamada);

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
                return BadRequest(new { success = false, mensaje = "Error al actualizar la llamada: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerLlamadaPorId(int idLlamada)
        {
            try
            {
                var llamadas = await ObtenerLlamadasAPI(idLlamada);

                if (llamadas == null)
                {
                    return BadRequest(new { success = false, mensaje = "Error al obtener la llamada" });
                }

                var llamada = llamadas.FirstOrDefault();

                if (llamada != null)
                {
                    return Ok(new { success = true, data = llamada });
                }

                return NotFound(new { success = false, mensaje = "Llamada no encontrada" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error: " + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarLlamada(int idLlamada)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Llamada/eliminarLlamada/{idLlamada}";

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
                return BadRequest(new { success = false, mensaje = "Error al eliminar la llamada: " + ex.Message });
            }
        }

        public async Task<ActionResult> MisLlamadas(DateTime? fecha = null)
        {
            var catalogos =  ObtenerCatalogos();
            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View();
            }

            // Obtener el ID del usuario actual desde la sesión
            var usuarioId = HttpContext.Session.GetInt32("ConsecutivoUsuario");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var llamadas = await ObtenerMisLlamadasAPI(usuarioId.Value, fecha);

            if (llamadas == null)
            {
                ViewBag.Error = "No se pudieron cargar las llamadas.";
                llamadas = new List<LlamadaModel>();
            }

            var modelo = new LlamadasViewModel
            {
                Llamadas = llamadas,
                Equipos = catalogos.Equipos,
                Centros = catalogos.Centros,
                Usuarios = catalogos.Usuarios,
                Estados = catalogos.Estados,
                Clientes = catalogos.Clientes
            };

            ViewBag.Titulo = "Mis Llamadas Asignadas";
            ViewBag.UsuarioIdActual = HttpContext.Session.GetInt32("ConsecutivoUsuario");
            return View("Llamadas", modelo); // Reutiliza la misma vista
        }

        private async Task<List<LlamadaModel>> ObtenerMisLlamadasAPI(int usuarioId, DateTime? fecha = null)
        {
            using (var client = _http.CreateClient())
            {
                var request = new RequestObtenerLlamada
                {
                    IdLlamada = 0,
                    Fecha = fecha ?? DateTime.Now,
                    UsuarioId = usuarioId // Agregar este campo al RequestObtenerLlamada
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

        public async Task<ActionResult> LlamadasPendientes(DateTime? fecha = null)
        {
            var catalogos =  ObtenerCatalogos();
            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View();
            }

            var llamadas = await ObtenerLlamadasAPI(0, fecha);

            if (llamadas != null)
            {
                llamadas = llamadas.Where(l => l.EstadoId == 3).ToList();
            }
            else
            {
                llamadas = new List<LlamadaModel>();
            }

            var modelo = new LlamadasViewModel
            {
                Llamadas = llamadas,
                Equipos = catalogos.Equipos,
                Centros = catalogos.Centros,
                Usuarios = catalogos.Usuarios,
                Estados = catalogos.Estados,
                Clientes = catalogos.Clientes
            };

            ViewBag.Titulo = "Llamadas Pendientes";
            return View("Llamadas", modelo);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarLlamada([FromBody] FinalizarLlamadaModel modelo)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/finalizarLlamada";

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
                return BadRequest(new { success = false, mensaje = "Error al finalizar la llamada: " + ex.Message });
            }
        }
    }
}
