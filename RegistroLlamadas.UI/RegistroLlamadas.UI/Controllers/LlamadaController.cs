using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Reflection;
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

        #region obtener llamadas
        public async Task<ActionResult> Llamadas(DateTime? fecha = null,
    string buscar = null,
    string estado = null,
    DateTime? fechaDesde = null,
    DateTime? fechaHasta = null)
        {
            var catalogos = ObtenerCatalogos();

            if (catalogos == null)
            {
                ViewBag.Error = "No se pudieron cargar los catálogos.";
                return View();
            }
            var llamadas = await ObtenerLlamadasAPI(0, fecha, buscar,estado,fechaDesde,fechaHasta);
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
            ViewBag.UsuarioIdActual = HttpContext.Session.GetInt32("ConsecutivoUsuario");
            ViewBag.RolActual = HttpContext.Session.GetInt32("IdPerfil");
            return View(modelo); 
        }

        #endregion


        #region obtener catalogo
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
        #endregion




        #region obtenerllamadasApi
        private async Task<List<LlamadaModel>> ObtenerLlamadasAPI(int idLlamada = 0,
    DateTime? fecha = null,
    string buscar = null,
    string estado = null,
    DateTime? fechaDesde = null,
    DateTime? fechaHasta = null,
    int? usuarioId = null)
        {
            using (var client = _http.CreateClient())
            {
                var request = new RequestObtenerLlamada
                {
                    IdLlamada = idLlamada,
                    Fecha = fecha,
                    UsuarioId = usuarioId,

                    Buscar = string.IsNullOrWhiteSpace(buscar) ? null : buscar,
                    Estado = string.IsNullOrWhiteSpace(estado) ? null : estado,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta
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
        #endregion



        #region registrar Llamada
        [HttpPost]
        public async Task<IActionResult> RegistrarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                using var client = _http.CreateClient();

                var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/registrarLlamada";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.PostAsJsonAsync(urlApi, llamada);
                var content = await respuesta.Content.ReadAsStringAsync();

        
                if (respuesta.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }

                // ❌ ERROR → devolver EXACTAMENTE lo que manda el API
                return StatusCode(
                    (int)respuesta.StatusCode,
                    JsonSerializer.Deserialize<object>(content)
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    code = "ERROR_UI",
                    mensaje = "Error al registrar la llamada: " + ex.Message
                });
            }
        }
        #endregion



        #region Actualizar llamada
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

        #endregion




        #region Obtener llamada por ID
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

        #endregion




        #region eliminar llamada
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

        #endregion




        #region obtener misLlamadas

        public async Task<ActionResult> MisLlamadas(DateTime? fecha = null,
    string buscar = null,
    string estado = null,
    DateTime? fechaDesde = null,
    DateTime? fechaHasta = null)
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

            var llamadas = await ObtenerLlamadasAPI(0, fecha, buscar, estado, fechaDesde, fechaHasta, usuarioId.Value);

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
            ViewBag.RolActual = HttpContext.Session.GetInt32("IdPerfil");
            return View("Llamadas", modelo); // Reutiliza la misma vista
        }

        #endregion



        #region obtener mis llamadas api
        private async Task<List<LlamadaModel>> ObtenerMisLlamadasAPI(int usuarioId, DateTime? fecha = null)
        {
            using (var client = _http.CreateClient())
            {
                var request = new RequestObtenerLlamada
                {
                    IdLlamada = 0,
                    Fecha = fecha ?? DateTime.Now,
                    UsuarioId = usuarioId
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

        #endregion



        #region finalizar llamada
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
        #endregion




        #region registrar la visita

        [HttpPost]
        public async Task<IActionResult> RegistrarVisita([FromBody] RegistrarVisitaRequest modelo)
        {
            try
            {
                if (modelo.IdLlamada <= 0)
                    return BadRequest(new { success = false, mensaje = "IdLlamada no válido" });

                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/Visita/Registrar";

                    // Token
                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }

                    // OBJETO que se enviará al API real
                    var enviarApi = new
                    {
                        IdLlamada = modelo.IdLlamada,
                        UsuarioId = modelo.UsuarioId,  // Puede ser null → API decide qué hacer
                        FechaVisita = DateTime.Now.Date,
                        HoraInicio = DateTime.Now.ToString("HH:mm"),
                        Comentario = "Visita asignada desde el panel"
                    };

                    var respuesta = await client.PostAsJsonAsync(urlApi, enviarApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var resultado = await respuesta.Content.ReadFromJsonAsync<dynamic>();
                        return Ok(new { success = true, data = resultado });
                    }

                    var errorApi = await respuesta.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, mensaje = errorApi });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error al registrar la visita: " + ex.Message });
            }
        }

        #endregion


        [HttpPost]
        public async Task<IActionResult> ObtenerDetalles([FromBody] RequestIdLlamada modelo)
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/detalles";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.PostAsJsonAsync(urlApi, modelo);

                if (!respuesta.IsSuccessStatusCode)
                    return BadRequest(new { success = false, mensaje = "Error al obtener detalles" });

                var data = await respuesta.Content.ReadFromJsonAsync<dynamic>();
                return Ok(data);
            }
        }

        #region reasignar visita
        [HttpPost]
        public async Task<IActionResult> Reasignar([FromBody] ReasignarLlamadaRequest model)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/Reasignar";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                    // Enviar la solicitud al API
                    var respuesta = await client.PostAsJsonAsync(urlApi, model);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var resultado = await respuesta.Content.ReadFromJsonAsync<dynamic>();
                        return Ok(new { success = true, data = resultado });
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, mensaje = errorContent });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error al reasignar la llamada: " + ex.Message });
            }
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> AgregarComentario([FromBody] ComentarioModel model)
        {
            using var client = _http.CreateClient();
            var urlApi = _configuration["Valores:UrlAPI"] + "Llamada/AgregarComentario";

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.PostAsJsonAsync(urlApi, model);

            if (resp.IsSuccessStatusCode)
                return Ok();

            return BadRequest();
        }

    }
}
