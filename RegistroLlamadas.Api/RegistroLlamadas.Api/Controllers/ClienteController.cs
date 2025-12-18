using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ClienteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Obtener clientes
        [HttpGet("obtenerClientes")]
        public async Task<IActionResult> ObtenerClientes()
        {
            try
            {
                using var con = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var clientes = await con.QueryAsync<ClienteModel>(
                    "sp_ObtenerClientes",
                    commandType: CommandType.StoredProcedure
                );

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = ex.Message
                });
            }
        }
        #endregion

        #region Obtener cliente por ID
        [HttpGet("obtenerClientePorId/{idCliente}")]
        public async Task<IActionResult> ObtenerClientePorId(int idCliente)
        {
            try
            {
                using var con = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var cliente = await con.QueryFirstOrDefaultAsync<ClienteModel>(
                    "sp_ObtenerClientePorId",
                    new { IdCliente = idCliente },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = ex.Message
                });
            }
        }
        #endregion

        #region Registrar cliente
        [HttpPost("registrarCliente")]
        public async Task<IActionResult> RegistrarCliente([FromBody] ClienteModel cliente)
        {
            try
            {
                using var con = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var parametros = new DynamicParameters();
                parametros.Add("@Identificacion", cliente.Identificacion);
                parametros.Add("@Nombre", cliente.Nombre);
                parametros.Add("@PrimerApellido", cliente.PrimerApellido);
                parametros.Add("@SegundoApellido", cliente.SegundoApellido);
                parametros.Add("@Correo", cliente.Correo);
                parametros.Add("@Telefono", cliente.Telefono);
                parametros.Add("@EstadoId", cliente.EstadoId);

                await con.ExecuteAsync(
                    "sp_RegistrarCliente",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    success = true,
                    mensaje = "Cliente registrado correctamente"
                });
            }
            catch (SqlException ex)
            {
   
                if (ex.Message.Contains("IDENTIFICACION_DUPLICADA"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        code = "IDENTIFICACION_DUPLICADA",
                        mensaje = "La identificación ya está registrada."
                    });
                }

      
                if (ex.Message.Contains("CORREO_DUPLICADO"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        code = "CORREO_DUPLICADO",
                        mensaje = "El correo electrónico ya está registrado."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    code = "ERROR_SQL",
                    mensaje = "Error al registrar el cliente."
                });
            }
        }
        #endregion

        #region Actualizar cliente
        [HttpPut("actualizarCliente")]
        public async Task<IActionResult> ActualizarCliente([FromBody] ClienteModel cliente)
        {
            try
            {
                using var con = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var parametros = new DynamicParameters();
                parametros.Add("@IdCliente", cliente.IdCliente);
                parametros.Add("@Identificacion", cliente.Identificacion);
                parametros.Add("@Nombre", cliente.Nombre);
                parametros.Add("@PrimerApellido", cliente.PrimerApellido);
                parametros.Add("@SegundoApellido", cliente.SegundoApellido);
                parametros.Add("@Correo", cliente.Correo);
                parametros.Add("@Telefono", cliente.Telefono);
                parametros.Add("@EstadoId", cliente.EstadoId);

                await con.ExecuteAsync(
                    "sp_ActualizarCliente",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    success = true,
                    mensaje = "Cliente actualizado correctamente"
                });
            }
            catch (SqlException ex)
            {
               
                if (ex.Message.Contains("IDENTIFICACION_DUPLICADA"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        code = "IDENTIFICACION_DUPLICADA",
                        mensaje = "La identificación ya pertenece a otro cliente."
                    });
                }

        
                if (ex.Message.Contains("CORREO_DUPLICADO"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        code = "CORREO_DUPLICADO",
                        mensaje = "El correo ya pertenece a otro cliente."
                    });
                }

                if (ex.Message.Contains("CLIENTE_NO_EXISTE"))
                {
                    return NotFound(new
                    {
                        success = false,
                        mensaje = "El cliente no existe."
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    code = "ERROR_SQL",
                    mensaje = "Error al actualizar el cliente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    mensaje = "Error inesperado: " + ex.Message
                });
            }
        }
        #endregion


    }
}
