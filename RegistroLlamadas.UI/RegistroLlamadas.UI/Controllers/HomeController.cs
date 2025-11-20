using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistroLlamadas.UI.Models;
using System.Diagnostics;
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
                        HttpContext.Session.SetString("Token", datosApi.Token);
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
