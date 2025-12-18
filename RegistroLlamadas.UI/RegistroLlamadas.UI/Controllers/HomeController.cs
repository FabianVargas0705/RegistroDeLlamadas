using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistroLlamadas.UI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using Utiles;
using static System.Net.WebRequestMethods;

namespace RegistroLlamadas.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;
       
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory http, IConfiguration configuration)
        {
            _logger = logger;
            _http = http;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string mensaje)
        {
            if (!string.IsNullOrEmpty(mensaje))
            {
                ViewBag.Mensaje = Uri.UnescapeDataString(mensaje);
            }
            return View();
        }
        [HttpPost]
        public IActionResult Login(UsuarioModel usuario)
        {
            var helper = new Helper();
            usuario.Contrasenna = helper.Encrypt(usuario.Contrasenna);
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Auth/ValidarSesion";
                var respuesta = context.PostAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                    if (datosApi != null)
                    {
                        HttpContext.Session.SetInt32("ConsecutivoUsuario", datosApi.ConsecutivoUsuario);
                        HttpContext.Session.SetString("NombreUsuario", datosApi.Nombre);
                        HttpContext.Session.SetString("NombrePerfil", datosApi.NombrePerfil);
                        HttpContext.Session.SetInt32("IdPerfil", datosApi.ConsecutivoPerfil);
                        HttpContext.Session.SetString("Token", datosApi.Token);
                     
                        if (datosApi.Contemporal)
                        {
                            return RedirectToAction("CambiarContrasena", "Home");
                        }

                        // ✅ Login normal
                        return RedirectToAction("DashboardView", "Dashboard");
                    }
                }

                ViewBag.Mensaje = "No se ha validado la información";
                return View();
            }
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContrasena(string NuevaContrasena,string ConfirmarContrasena)
        {
            if (NuevaContrasena != ConfirmarContrasena)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }

            int idUsuario = HttpContext.Session.GetInt32("ConsecutivoUsuario") ?? 0;

            var helper = new Helper();
            string passwordEncrypt = helper.Encrypt(NuevaContrasena);

            using (var client = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/actualizarContrasena";

                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                var dto = new
                {
                    IdUsuario = idUsuario,
                    NuevaContrasena = passwordEncrypt
                };

                var respuesta = await client.PutAsJsonAsync(urlApi, dto);

                if (respuesta.IsSuccessStatusCode)
                {
                    return RedirectToAction("DashboardView", "Dashboard");
                }

                ViewBag.Mensaje = "Error al actualizar la contraseña";
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
