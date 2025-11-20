using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RegistroLlamadas.UI.Controllers
{
    public class RolController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public RolController(IHttpClientFactory http, IConfiguration configuration)
        {
            _configuration = configuration;
            _http = http;
        }
        // GET: Rol/Index - Página principal con menú de opciones
        public ActionResult Index()
        {
            return View();
        }

        // GET: Rol/ConsultarRoles - Vista para consultar todos los roles
        public async Task<ActionResult> ConsultarRoles()
        {
            var roles = await ObtenerRolesAPI();
            return View(roles ?? new List<RolModel>());
        }

        // GET: Rol/AgregarRoles - Vista para agregar un nuevo rol
        public ActionResult AgregarRoles()
        {
            return View(new RolModel());
        }

        // POST: Rol/AgregarRoles - Procesar el formulario de agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarRoles(RolModel rol)
        {
            if (!ModelState.IsValid)
            {
                return View(rol);
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Rol/RegistrarRol";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, rol);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Rol registrado correctamente";
                        return RedirectToAction(nameof(ConsultarRoles));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al registrar el rol: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al registrar el rol: " + ex.Message;
            }

            return View(rol);
        }

        // GET: Rol/ActualizarRoles/5 - Vista para actualizar un rol
        public async Task<ActionResult> ActualizarRoles(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Rol/ObtenerRolPorId/{id}";

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
                        var rol = await respuesta.Content.ReadFromJsonAsync<RolModel>(options);
                        if (rol != null)
                        {
                            return View(rol);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el rol: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarRoles));
        }

        // POST: Rol/ActualizarRoles - Procesar el formulario de actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarRoles(RolModel rol)
        {
            if (!ModelState.IsValid)
            {
                return View(rol);
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Rol/ActualizarRol";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PutAsJsonAsync(urlApi, rol);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Rol actualizado correctamente";
                        return RedirectToAction(nameof(ConsultarRoles));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al actualizar el rol: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el rol: " + ex.Message;
            }

            return View(rol);
        }

        // GET: Rol/EliminarRoles/5 - Vista para confirmar eliminación
        public async Task<ActionResult> EliminarRoles(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Rol/ObtenerRolPorId/{id}";

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
                        var rol = await respuesta.Content.ReadFromJsonAsync<RolModel>(options);
                        if (rol != null)
                        {
                            return View(rol);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el rol: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarRoles));
        }

        // POST: Rol/EliminarRoles - Procesar la eliminación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRoles(RolModel rol)
        {
            if (rol == null || rol.IdRol == 0)
            {
                TempData["ErrorMessage"] = "ID de rol inválido";
                return RedirectToAction(nameof(ConsultarRoles));
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Rol/EliminarRol/{rol.IdRol}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.DeleteAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Rol eliminado correctamente";
                    }
                    else
                    {
                        var errorContent = await respuesta.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al eliminar el rol: " + errorContent;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el rol: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarRoles));
        }

        // Método auxiliar para obtener roles desde la API
        private async Task<List<RolModel>> ObtenerRolesAPI()
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Rol/ObtenerRoles";

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
                    var roles = await respuesta.Content.ReadFromJsonAsync<List<RolModel>>(options);
                    return roles ?? new List<RolModel>();
                }
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerRolPorId(int idRol)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Rol/ObtenerRolPorId/{idRol}";

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
                        var rol = await respuesta.Content.ReadFromJsonAsync<RolModel>(options);
                        return Ok(new { success = true, data = rol });
                    }

                    return BadRequest(new { success = false, mensaje = "Error al obtener el rol" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarRol([FromBody] RolModel rol)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Rol/RegistrarRol";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, rol);

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
                return BadRequest(new { success = false, mensaje = "Error al registrar el rol: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarRol([FromBody] RolModel rol)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Rol/ActualizarRol";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PutAsJsonAsync(urlApi, rol);

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
                return BadRequest(new { success = false, mensaje = "Error al actualizar el rol: " + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarRol(int idRol)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Rol/EliminarRol/{idRol}";

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
                return BadRequest(new { success = false, mensaje = "Error al eliminar el rol: " + ex.Message });
            }
        }
    }

}
