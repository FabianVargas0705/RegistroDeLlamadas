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
    public class LlamadaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LlamadaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // GET: api/<LlamadaController>
        [HttpPost]
        [Route("obtenerLlamada")]
        public async Task<IEnumerable<LlamadaModel>> ObtenerLlamadas([FromBody] RequestObtenerLlamada llamada)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new
                {
                    IdLlamada = llamada.IdLlamada,
                    Fecha = llamada.Fecha,
                    UsuarioId = llamada.UsuarioId
                };

                var resultado = context.QueryAsync<LlamadaModel, UsuarioModel, ClienteModel, EquipoModel, CentroModel, EstadoModel, LlamadaModel>(
                    "sp_obtener_llamadas",
                    (llamadaModel, usuario, cliente, equipo, centro, estado) =>
                    {
                        llamadaModel.Usuario = usuario;
                        llamadaModel.Cliente = cliente;
                        llamadaModel.Equipo = equipo;
                        llamadaModel.Centro = centro;
                        llamadaModel.Estado = estado;
                        return llamadaModel;
                    },
                    parametros,
                    splitOn: "ConsecutivoUsuario,IdCliente,IdEquipo,IdCentro,IdEstado",
                    commandType: CommandType.StoredProcedure
                );

                return await resultado;
            }
        }

        [HttpPost]
        [Route("registrarLlamada")]
        public async Task<IActionResult> RegistrarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    
                    var parametros = new DynamicParameters();
                    parametros.Add("@Fecha", llamada.Fecha);
                    parametros.Add("@HoraInicio", llamada.HoraInicio);
                    parametros.Add("@HoraFinal", llamada.HoraFinal);
                    parametros.Add("@Promat", llamada.Promat);
                    parametros.Add("@Asunto", llamada.Asunto);
                    parametros.Add("@CorreoEnviado", llamada.CorreoEnviado);
                    parametros.Add("@UsuarioId", llamada.UsuarioId);
                    parametros.Add("@EquipoId", llamada.EquipoId);
                    parametros.Add("@ClienteId", llamada.ClienteId);
                    parametros.Add("@CentroId", llamada.CentroId);
                    parametros.Add("@Estado", await obtenerEstado(llamada));
                    parametros.Add("@IdLlamadaGenerado", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await context.ExecuteAsync(
                        "sp_registrar_llamada",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    var idGenerado = parametros.Get<int>("@IdLlamadaGenerado");

                    return Ok(new
                    {
                        success = true,
                        idLlamada = idGenerado,
                        mensaje = "Llamada registrada correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al registrar la llamada: " + ex.Message
                });
            }
        
        }

        private async Task<string> obtenerEstado(LlamadaModel llamada)
        {
            if (llamada.Estado == null)
                llamada.Estado = new EstadoModel();


            bool tieneHoraInicio = llamada.HoraInicio != TimeSpan.Zero;
            bool tieneHoraFinal = llamada.HoraFinal.HasValue;

            // 1. Si trae hora inicio y final → Finalizada
            if (tieneHoraInicio && tieneHoraFinal)
            {
                llamada.Estado.Descripcion = "Finalizado";
                return llamada.Estado.Descripcion;
            }

            // 2. Si NO trae hora final pero sí hora inicio
            if (tieneHoraInicio && !tieneHoraFinal)
            {
                DateTime horaInicio = DateTime.Today.Add(llamada.HoraInicio);
                double horasDiferencia = (DateTime.Now - horaInicio).TotalHours;

                if (horasDiferencia < 1)
                    llamada.Estado.Descripcion = "A tiempo";
                else if (horasDiferencia >= 1 && horasDiferencia < 2)
                    llamada.Estado.Descripcion = "En apoyo";
                else
                    llamada.Estado.Descripcion = "Atrasado";

                return llamada.Estado.Descripcion;
            }

            // Si no cae en ningún caso, devolver algo por defecto
            llamada.Estado.Descripcion = "Sin información";
            return llamada.Estado.Descripcion;
        }

        [HttpPost]
        [Route("actualizarLlamada")]
        public async Task<IActionResult> ActualizarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdLlamada", llamada.IdLlamada);
                    parametros.Add("@Fecha", llamada.Fecha);
                    parametros.Add("@HoraInicio", llamada.HoraInicio);
                    parametros.Add("@HoraFinal", llamada.HoraFinal);
                    parametros.Add("@Promat", llamada.Promat);
                    parametros.Add("@Asunto", llamada.Asunto);
                    parametros.Add("@CorreoEnviado", llamada.CorreoEnviado);
                    parametros.Add("@UsuarioId", llamada.UsuarioId);
                    parametros.Add("@EquipoId", llamada.EquipoId);
                    parametros.Add("@ClienteId", llamada.ClienteId);
                    parametros.Add("@CentroId", llamada.CentroId);
                    parametros.Add("@Estado", await obtenerEstado(llamada));

                    var resultado = await context.QueryFirstOrDefaultAsync<int>(
                        "sp_actualizar_llamada",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        idLlamada = resultado,
                        mensaje = "Llamada actualizada correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al actualizar la llamada: " + ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("eliminarLlamada/{idLlamada}")]
        public async Task<IActionResult> EliminarLlamada(int idLlamada)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdLlamada", idLlamada);

                    var resultado = await context.QueryFirstOrDefaultAsync<int>(
                        "sp_eliminar_llamada",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Llamada eliminada correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al eliminar la llamada: " + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("finalizarLlamada")]
        public async Task<IActionResult> FinalizarLlamada([FromBody] FinalizarLlamadaModel modelo)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdLlamada", modelo.IdLlamada);
                    parametros.Add("@DescripcionSolucion", modelo.DescripcionSolucion);
                    parametros.Add("@HoraFinal", modelo.HoraFinal);
                    parametros.Add("@CorreoEnviado", modelo.CorreoEnviado);

                    await context.ExecuteAsync(
                        "sp_finalizar_llamada",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Llamada finalizada correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al finalizar la llamada: " + ex.Message
                });
            }
        }
    }
}
