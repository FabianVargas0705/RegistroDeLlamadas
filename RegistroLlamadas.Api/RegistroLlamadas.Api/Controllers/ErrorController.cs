using Dapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ErrorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("RegistrarError")]
        public IActionResult RegistrarError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

            int consecutivoUsuario = int.TryParse(HttpContext.User.FindFirst("id")?.Value, out var id)
             ? id
             : 0;

            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@ConsecutivoUsuario", consecutivoUsuario);
                parametros.Add("@MensajeError", exception?.Error.Message);
                parametros.Add("@OrigenError", exception?.Path);

                context.Execute("RegistrarError", parametros);
            }

            return StatusCode(500, "Se presentó una excepción en nuestro servicio");
        }
    }
}
