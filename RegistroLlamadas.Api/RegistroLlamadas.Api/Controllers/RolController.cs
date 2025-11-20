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
    public class RolController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RolController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerRoles")]
        public async Task<IActionResult> ObtenerRoles()
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var roles = await context.QueryAsync<RolModel>(
                        "sp_ObtenerRoles",
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(roles);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener los roles: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("ObtenerRolPorId/{idRol}")]
        public async Task<IActionResult> ObtenerRolPorId(int idRol)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdRol", idRol);

                    var rol = await context.QueryFirstOrDefaultAsync<RolModel>(
                        "sp_ObtenerRolPorId",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    if (rol == null)
                        return NotFound(new { success = false, mensaje = "Rol no encontrado" });

                    return Ok(rol);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener el rol: " + ex.Message
                });
            }
        }

        
        [HttpPost]
        [Route("RegistrarRol")]
        public async Task<IActionResult> RegistrarRol([FromBody] RolModel rol)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Nombre", rol.Nombre);
                    parametros.Add("@Descripcion", rol.Descripcion);
                    parametros.Add("@IdRolGenerado", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await context.ExecuteAsync(
                        "sp_RegistrarRol",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    var idGenerado = parametros.Get<int>("@IdRolGenerado");

                    return Ok(new
                    {
                        success = true,
                        idRol = idGenerado,
                        mensaje = "Rol registrado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al registrar el rol: " + ex.Message
                });
            }
        }

        [HttpPut]
        [Route("ActualizarRol")]
        public async Task<IActionResult> ActualizarRol([FromBody] RolModel rol)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdRol", rol.IdRol);
                    parametros.Add("@Nombre", rol.Nombre);
                    parametros.Add("@Descripcion", rol.Descripcion);

                    await context.ExecuteAsync(
                        "sp_ActualizarRol",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Rol actualizado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al actualizar el rol: " + ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("EliminarRol/{idRol}")]
        public async Task<IActionResult> EliminarRol(int idRol)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdRol", idRol);

                    await context.ExecuteAsync(
                        "sp_EliminarRol",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Rol eliminado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al eliminar el rol: " + ex.Message
                });
            }
        }
    }
}

    

