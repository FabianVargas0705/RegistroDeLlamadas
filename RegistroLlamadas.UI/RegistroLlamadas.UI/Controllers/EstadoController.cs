using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RegistroLlamadas.UI.Controllers
{
    public class EstadoController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public EstadoController(IHttpClientFactory http, IConfiguration configuration)
        {
            _configuration = configuration;
            _http = http;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> ConsultarEstados()
        {
            var estados = await ObtenerEstadosAPI();
            return View(estados ?? new List<EstadoModel>());
        }

        public ActionResult AgregarEstados()
        {
            return View(new EstadoModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarEstados(EstadoModel estado)
        {
            if (!ModelState.IsValid)
                return View(estado);

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Estado/RegistrarEstado";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, estado);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Estado registrado correctamente";
                        return RedirectToAction(nameof(ConsultarEstados));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al registrar el estado: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al registrar el estado: " + ex.Message;
            }

            return View(estado);
        }

        public async Task<ActionResult> ActualizarEstados(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Estado/ObtenerEstadoPorId/{id}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var estado = await respuesta.Content.ReadFromJsonAsync<EstadoModel>(options);
                        if (estado != null)
                            return View(estado);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el estado: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarEstados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstados(EstadoModel estado)
        {
            if (!ModelState.IsValid)
                return View(estado);

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Estado/ActualizarEstado";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PutAsJsonAsync(urlApi, estado);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Estado actualizado correctamente";
                        return RedirectToAction(nameof(ConsultarEstados));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al actualizar el estado: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el estado: " + ex.Message;
            }

            return View(estado);
        }

        public async Task<ActionResult> EliminarEstados(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Estado/ObtenerEstadoPorId/{id}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var estado = await respuesta.Content.ReadFromJsonAsync<EstadoModel>(options);
                        if (estado != null)
                            return View(estado);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el estado: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarEstados));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEstados(EstadoModel estado)
        {
            if (estado == null || estado.IdEstado == 0)
            {
                TempData["ErrorMessage"] = "ID de estado inválido";
                return RedirectToAction(nameof(ConsultarEstados));
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Estado/EliminarEstado/{estado.IdEstado}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.DeleteAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Estado eliminado correctamente";
                    }
                    else
                    {
                        var errorContent = await respuesta.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al eliminar el estado: " + errorContent;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el estado: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarEstados));
        }

        private async Task<List<EstadoModel>> ObtenerEstadosAPI()
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Estado/ObtenerEstados";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var estados = await respuesta.Content.ReadFromJsonAsync<List<EstadoModel>>(options);
                    return estados ?? new List<EstadoModel>();
                }
            }
            return null;
        }
    }
}