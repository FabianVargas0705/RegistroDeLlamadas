using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.Api.Models;
using RegistroLlamadas.Api.Servicios.WhatsApp;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppController : ControllerBase
    {
 
        private readonly WhatsAppService _ws;

        public WhatsAppController(WhatsAppService ws)
        {
            _ws = ws;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotificacion([FromBody] WhatsAppRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Numero))
                return BadRequest("Debe indicar el número.");

            var resultado = await _ws.EnviarMensaje(
                body.Numero,
                body.NombreUsuario,
                body.IdLlamada,
                body.Asunto,
                body.NombreCliente,
                body.AtendidoPor
            );

            return Ok(resultado);
        }
    }
}
