using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RegistroLlamadas.UI.Controllers
{
    public class CentroController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public CentroController(IHttpClientFactory http, IConfiguration configuration)
        {
            _configuration = configuration;
            _http = http;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> ConsultarCentros()
        {
            var centros = await ObtenerCentrosAPI();
            return View(centros ?? new List<CentroModel>());
        }

        public ActionResult AgregarCentros()
        {
            return View(new CentroModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarCentros(CentroModel centro)
        {
            if (!ModelState.IsValid)
                return View(centro);

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Centro/RegistrarCentro";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PostAsJsonAsync(urlApi, centro);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Centro registrado correctamente";
                        return RedirectToAction(nameof(ConsultarCentros));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al registrar el centro: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al registrar el centro: " + ex.Message;
            }

            return View(centro);
        }

        public async Task<ActionResult> ActualizarCentros(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Centro/ObtenerCentroPorId/{id}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var centro = await respuesta.Content.ReadFromJsonAsync<CentroModel>(options);
                        if (centro != null)
                            return View(centro);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el centro: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarCentros));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCentros(CentroModel centro)
        {
            if (!ModelState.IsValid)
                return View(centro);

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Centro/ActualizarCentro";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.PutAsJsonAsync(urlApi, centro);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Centro actualizado correctamente";
                        return RedirectToAction(nameof(ConsultarCentros));
                    }

                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error al actualizar el centro: " + errorContent;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el centro: " + ex.Message;
            }

            return View(centro);
        }

        public async Task<ActionResult> EliminarCentros(int id)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Centro/ObtenerCentroPorId/{id}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.GetAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var centro = await respuesta.Content.ReadFromJsonAsync<CentroModel>(options);
                        if (centro != null)
                            return View(centro);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el centro: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarCentros));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCentros(CentroModel centro)
        {
            if (centro == null || centro.IdCentro == 0)
            {
                TempData["ErrorMessage"] = "ID de centro inválido";
                return RedirectToAction(nameof(ConsultarCentros));
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + $"Centro/EliminarCentro/{centro.IdCentro}";

                    var token = HttpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var respuesta = await client.DeleteAsync(urlApi);

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Centro eliminado correctamente";
                    }
                    else
                    {
                        var errorContent = await respuesta.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al eliminar el centro: " + errorContent;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el centro: " + ex.Message;
            }

            return RedirectToAction(nameof(ConsultarCentros));
        }

        private async Task<List<CentroModel>> ObtenerCentrosAPI()
        {
            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Centro/ObtenerCentros";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await client.GetAsync(urlApi);

                if (respuesta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var centros = await respuesta.Content.ReadFromJsonAsync<List<CentroModel>>(options);
                    return centros ?? new List<CentroModel>();
                }
            }
            return null;
        }
    }
}