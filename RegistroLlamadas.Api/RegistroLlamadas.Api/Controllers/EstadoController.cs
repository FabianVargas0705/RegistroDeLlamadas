using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using System.Data;

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EstadoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerEstados")]
        public async Task<IActionResult> ObtenerEstados()
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var estados = await context.QueryAsync<EstadoModel>(
                        "sp_ObtenerEstados",
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(estados);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener los estados: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("ObtenerEstadoPorId/{idEstado}")]
        public async Task<IActionResult> ObtenerEstadoPorId(int idEstado)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdEstado", idEstado);

                    var estado = await context.QueryFirstOrDefaultAsync<EstadoModel>(
                        "sp_ObtenerEstadoPorId",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    if (estado == null)
                        return NotFound(new { success = false, mensaje = "Estado no encontrado" });

                    return Ok(estado);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener el estado: " + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("RegistrarEstado")]
        public async Task<IActionResult> RegistrarEstado([FromBody] EstadoModel estado)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Descripcion", estado.Descripcion);
                    parametros.Add("@TipoEstado", estado.TipoEstado);
                    parametros.Add("@IdEstadoGenerado", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await context.ExecuteAsync(
                        "sp_RegistrarEstado",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    var idGenerado = parametros.Get<int>("@IdEstadoGenerado");

                    return Ok(new
                    {
                        success = true,
                        idEstado = idGenerado,
                        mensaje = "Estado registrado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al registrar el estado: " + ex.Message
                });
            }
        }

        [HttpPut]
        [Route("ActualizarEstado")]
        public async Task<IActionResult> ActualizarEstado([FromBody] EstadoModel estado)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdEstado", estado.IdEstado);
                    parametros.Add("@Descripcion", estado.Descripcion);
                    parametros.Add("@TipoEstado", estado.TipoEstado);

                    await context.ExecuteAsync(
                        "sp_ActualizarEstado",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Estado actualizado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al actualizar el estado: " + ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("EliminarEstado/{idEstado}")]
        public async Task<IActionResult> EliminarEstado(int idEstado)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdEstado", idEstado);

                    await context.ExecuteAsync(
                        "sp_EliminarEstado",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Estado eliminado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al eliminar el estado: " + ex.Message
                });
            }
        }
    }
}