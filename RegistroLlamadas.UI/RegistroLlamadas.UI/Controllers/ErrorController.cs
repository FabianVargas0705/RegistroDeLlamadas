using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RegistroLlamadas.UI.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult MostrarError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return View();
        }
    }
}
