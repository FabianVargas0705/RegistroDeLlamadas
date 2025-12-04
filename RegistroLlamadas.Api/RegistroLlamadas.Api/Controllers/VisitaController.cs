using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using RegistroLlamadas.Api.Servicios.Correo;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public VisitaController(IConfiguration configuration)
        {
            _configuration = configuration;
      
        }

        [HttpPost]
        [Route("ObtenerVisitas")]
        public async Task<IEnumerable<VisitaModel>> ObtenerVisitas([FromBody] RequestObtenerVisita filtro)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new
                {
                    IdVisita = (filtro.IdVisita == 0 ? null : filtro.IdVisita),
                    Fecha = filtro.Fecha,
                    UsuarioId = (filtro.UsuarioId == 0 ? null : filtro.UsuarioId)
                };

                var resultado = await context.QueryAsync<VisitaModel>(
                    "sp_obtener_visitas",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return resultado;
            }
        }

        [HttpPost("finalizarVisita")]
        public async Task<IActionResult> FinalizarVisita([FromBody] FinalizarVisitaRequest modelo)
        {
            try
            {
                if (modelo == null || modelo.IdVisita <= 0)
                    return BadRequest(new { success = false, mensaje = "Datos inválidos." });

                using (var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new
                    {
                        IdVisita = modelo.IdVisita,
                        Descripcion = modelo.Descripcion,
                        HoraFinal = modelo.HoraFinal
                    };

                    var result = await connection.ExecuteAsync(
                        "sp_finalizar_visita",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new { success = true, mensaje = "Visita finalizada correctamente." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al finalizar la visita: " + ex.Message
                });
            }
        }

        // GET api/<VisitaController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<VisitaController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<VisitaController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<VisitaController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
