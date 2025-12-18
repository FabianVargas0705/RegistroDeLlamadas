using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using System.Data;

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EquipoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerEquipos")]
        public async Task<IActionResult> ObtenerEquipos()
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var equipos = await context.QueryAsync<EquipoModel>(
                        "sp_ObtenerEquipos",
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(equipos);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener los equipos: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("ObtenerEquiposPorCentro/{idCentro}")]
        public async Task<IActionResult> ObtenerEquiposPorCentro(int idCentro)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdCentro", idCentro);
                    var equipos = await context.QueryAsync<EquipoModel>(
                        "sp_ObtenerEquiposPorCentro",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(equipos);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener los equipos del centro: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("ObtenerEquipoPorId/{idEquipo}")]
        public async Task<IActionResult> ObtenerEquipoPorId(int idEquipo)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdEquipo", idEquipo);

                    var equipo = await context.QueryFirstOrDefaultAsync<EquipoModel>(
                        "sp_ObtenerEquipoPorId",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    if (equipo == null)
                        return NotFound(new { success = false, mensaje = "Equipo no encontrado" });

                    return Ok(equipo);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener el equipo: " + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("RegistrarEquipo")]
        public async Task<IActionResult> RegistrarEquipo([FromBody] EquipoModel equipo)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Nombre", equipo.Nombre);
                    parametros.Add("@Modelo", equipo.Modelo);
                    parametros.Add("@Marca", equipo.Marca);
                    parametros.Add("@Condicion", equipo.Condicion);
                    parametros.Add("@Serie", equipo.Serie);
                    parametros.Add("@Activo", equipo.Activo);
                    parametros.Add("@CentroId", equipo.CentroId);
                    parametros.Add("@EstadoId", equipo.EstadoId);
                    parametros.Add("@IdEquipoGenerado", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await context.ExecuteAsync(
                        "sp_RegistrarEquipo",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    var idGenerado = parametros.Get<int>("@IdEquipoGenerado");

                    return Ok(new
                    {
                        success = true,
                        idEquipo = idGenerado,
                        mensaje = "Equipo registrado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al registrar el equipo: " + ex.Message
                });
            }
        }

        [HttpPut]
        [Route("ActualizarEquipo")]
        public async Task<IActionResult> ActualizarEquipo([FromBody] EquipoModel equipo)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdEquipo", equipo.IdEquipo);
                    parametros.Add("@Nombre", equipo.Nombre);
                    parametros.Add("@Modelo", equipo.Modelo);
                    parametros.Add("@Marca", equipo.Marca);
                    parametros.Add("@Condicion", equipo.Condicion);
                    parametros.Add("@Serie", equipo.Serie);
                    parametros.Add("@Activo", equipo.Activo);
                    parametros.Add("@CentroId", equipo.CentroId);
                    parametros.Add("@EstadoId", equipo.EstadoId);

                    await context.ExecuteAsync(
                        "sp_ActualizarEquipo",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Equipo actualizado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al actualizar el equipo: " + ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("EliminarEquipo/{idEquipo}")]
        public async Task<IActionResult> EliminarEquipo(int idEquipo)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdEquipo", idEquipo);

                    await context.ExecuteAsync(
                        "sp_EliminarEquipo",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Equipo eliminado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al eliminar el equipo: " + ex.Message
                });
            }
        }
    }
}

