using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using System.Data;

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentroController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CentroController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerCentros")]
        public async Task<IActionResult> ObtenerCentros()
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var centros = await context.QueryAsync<CentroModel>(
                        "sp_ObtenerCentros",
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(centros);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener los centros: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("ObtenerCentroPorId/{idCentro}")]
        public async Task<IActionResult> ObtenerCentroPorId(int idCentro)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdCentro", idCentro);

                    var centro = await context.QueryFirstOrDefaultAsync<CentroModel>(
                        "sp_ObtenerCentroPorId",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    if (centro == null)
                        return NotFound(new { success = false, mensaje = "Centro no encontrado" });

                    return Ok(centro);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener el centro: " + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("RegistrarCentro")]
        public async Task<IActionResult> RegistrarCentro([FromBody] CentroModel centro)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Nombre", centro.Nombre);
                    parametros.Add("@EstadoId", centro.EstadoId);
                    parametros.Add("@IdCentroGenerado", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await context.ExecuteAsync(
                        "sp_RegistrarCentro",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    var idGenerado = parametros.Get<int>("@IdCentroGenerado");

                    return Ok(new
                    {
                        success = true,
                        idCentro = idGenerado,
                        mensaje = "Centro registrado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al registrar el centro: " + ex.Message
                });
            }
        }

        [HttpPut]
        [Route("ActualizarCentro")]
        public async Task<IActionResult> ActualizarCentro([FromBody] CentroModel centro)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdCentro", centro.IdCentro);
                    parametros.Add("@Nombre", centro.Nombre);
                    parametros.Add("@EstadoId", centro.EstadoId);

                    await context.ExecuteAsync(
                        "sp_ActualizarCentro",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Centro actualizado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al actualizar el centro: " + ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("EliminarCentro/{idCentro}")]
        public async Task<IActionResult> EliminarCentro(int idCentro)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdCentro", idCentro);

                    await context.ExecuteAsync(
                        "sp_EliminarCentro",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Centro eliminado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al eliminar el centro: " + ex.Message
                });
            }
        }
    }
}