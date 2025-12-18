using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RegistroLlamadas.UI.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public UsuarioController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }
        #region obtenerUsuariosApi (GET)
        private async Task<List<UsuarioDTO>> ObtenerUsuariosAPI(int idUsuario = 0)
        {
            using (var client = _http.CreateClient())
            {
                string urlApi;

                if (idUsuario > 0)
                    urlApi = _configuration["Valores:UrlAPI"] + $"Usuario/obtenerUsuarioPorId/{idUsuario}";
                else
                    urlApi = _configuration["Valores:UrlAPI"] + "Usuario/obtenerUsuarios";

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

                    if (idUsuario > 0)
                    {
                        var usuario = await respuesta.Content
                            .ReadFromJsonAsync<UsuarioDTO>(options);

                        return usuario != null
                            ? new List<UsuarioDTO> { usuario }
                            : new List<UsuarioDTO>();
                    }

                    var usuarios = await respuesta.Content
                        .ReadFromJsonAsync<List<UsuarioDTO>>(options);

                    return usuarios ?? new List<UsuarioDTO>();
                }

                return null;
            }
        }
        #endregion

        #region Registrar Usuario
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioDTO usuario)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/registrarUsuario";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }

                    var respuesta = await client.PostAsJsonAsync(urlApi, usuario);
                    var content = await respuesta.Content.ReadAsStringAsync();

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var contentOk = await respuesta.Content.ReadAsStringAsync();
                        return Content(contentOk, "application/json");
                    }

       
                    var errorContent = await respuesta.Content.ReadAsStringAsync();

                    return StatusCode(
              (int)respuesta.StatusCode,
              JsonSerializer.Deserialize<object>(content)
          );
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    mensaje = "Ocurrió un error inesperado al registrar el usuario."
                });
            }
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

        #region Obtener todos los usuarios 
        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            try
            {
                var usuarios = await ObtenerUsuariosAPI() ?? new List<UsuarioDTO>();

                var catalogos = ObtenerCatalogos();

                ViewBag.Estados = catalogos?.Estados
                    .Where(e => e.TipoEstado == 1)
                    .ToList()
                    ?? new List<CatalogoItem>(); ViewBag.Roles = catalogos?.Roles ?? new List<RolItem>();

                if (usuarios == null)
                {
                    TempData["ErrorMessage"] = "No se pudieron cargar los usuarios";
                    return View(new List<UsuarioDTO>());
                }

                return View(usuarios);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar usuarios: " + ex.Message;
                return View(new List<UsuarioDTO>());
            }
        }
        #endregion

        #region Actualizar Usuario
        [HttpPost]
        public async Task<IActionResult> ActualizarUsuario([FromBody] UsuarioDTO usuario)
        {
            try
            {
                using var client = _http.CreateClient();

                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/actualizarUsuario";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

        
                var respuesta = await client.PutAsJsonAsync(urlApi, usuario);

                var content = await respuesta.Content.ReadAsStringAsync();

       
                if (respuesta.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }

        
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
                    mensaje = "Error al actualizar el usuario: " + ex.Message
                });
            }
        }
        #endregion

        #region Obtener Usuario por ID
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuarioPorId(int idUsuario)
        {
            try
            {
                var usuarios = await ObtenerUsuariosAPI(idUsuario);

                if (usuarios == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        mensaje = "Error al obtener el usuario"
                    });
                }

                var usuario = usuarios.FirstOrDefault();

                if (usuario != null)
                {
                    return Ok(new
                    {
                        success = true,
                        data = usuario
                    });
                }

                return NotFound(new
                {
                    success = false,
                    mensaje = "Usuario no encontrado"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error: " + ex.Message
                });
            }
        }
        #endregion


    }
}
