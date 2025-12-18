using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using RegistroLlamadas.Api.Servicios.Correo;
using RegistroLlamadas.Api.Servicios.Historial;
using System.Data;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LlamadaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EnvioCorreo _enviarCorreo;
        private readonly HistorialLlamada _historialLlamada;

        public LlamadaController(IConfiguration configuration, EnvioCorreo envioCorreo, HistorialLlamada historialLlamada)
        {
            _configuration = configuration;
            _enviarCorreo = envioCorreo;
            _historialLlamada = historialLlamada;
        }
        // GET: api/<LlamadaController>
        #region Obtener llamadas
        [HttpPost]
        [Route("obtenerLlamada")]
        public async Task<IEnumerable<LlamadaModel>> ObtenerLlamadas([FromBody] RequestObtenerLlamada llamada)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();

                parametros.Add("@IdLlamada", llamada.IdLlamada ?? 0);
                parametros.Add("@Fecha", llamada.Fecha, DbType.Date);
                parametros.Add("@UsuarioId", llamada.UsuarioId, DbType.Int32);
                parametros.Add("@FechaDesde", llamada.FechaDesde, DbType.Date);
                parametros.Add("@FechaHasta", llamada.FechaHasta, DbType.Date);
                parametros.Add("@Estado", llamada.Estado, DbType.String);
                parametros.Add("@Buscar", llamada.Buscar, DbType.String);

                var resultado = await context.QueryAsync<
                    LlamadaModel, UsuarioModel, ClienteModel, EquipoModel, CentroModel, EstadoModel, LlamadaModel>(
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

                return resultado;
            }
        }


        #endregion

        #region registrar historial llamada
        private void RegistrarHistorialLlamada(int idLlamada, string accion, string descripcion, int? usuarioAfectado, int? registradoPor)
        {
            _historialLlamada.RegistrarHistorial(idLlamada, accion, descripcion, usuarioAfectado, registradoPor);
        }

        #endregion


        #region Agregar comentario
        [Authorize]
        [HttpPost("AgregarComentario")]
        public async Task<IActionResult> AgregarComentario(ComentarioModel model)
        {
            var nombreUser = User.FindFirst("nombre").Value;
            var registradoPor = int.Parse(User.FindFirst("id").Value);
            RegistrarHistorialLlamada(
                idLlamada: model.IdLlamada,
                accion: "Comentario Agregado",
                descripcion: $"Comentario agregado por {nombreUser}: {model.Comentario}",
                usuarioAfectado: null,
                registradoPor: registradoPor
            );
            return Ok();
        }

        #endregion


        #region Registrar Llamada
        [Authorize]
        [HttpPost]
        [Route("registrarLlamada")]
        public async Task<IActionResult> RegistrarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                var nombreUser = User.FindFirst("nombre").Value;

                var registradoPor = int.Parse(User.FindFirst("id").Value);

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

                    RegistrarHistorialLlamada(
                                    idLlamada: idGenerado,
                                    accion: "Creación",
                                    descripcion: $"Llamada registrada por el usuario {nombreUser}. Asunto: {llamada.Asunto}",
                                    usuarioAfectado: llamada.UsuarioId,   
                                    registradoPor: registradoPor
                                );
                    if(llamada.UsuarioId != registradoPor)
                    {
                        EnviarCorreoReasignacion(idGenerado, nombreUser);

                    }
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

        #endregion

        private CorreoAsignacionModel ObtenerDatosCorreoReasignacion(int idLlamada)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            return context.QueryFirstOrDefault<CorreoAsignacionModel>(
                "sp_obtener_datos_asignacion",
                new { IdLlamada = idLlamada },
                commandType: CommandType.StoredProcedure
            );
        }

        private void EnviarCorreoReasignacion(int idLlamada, string reasignadoPor)
        {
            var datos = ObtenerDatosCorreoReasignacion(idLlamada);

            if (datos == null || string.IsNullOrWhiteSpace(datos.CorreoUsuario))
                return;

            string cuerpo = $@"
        <h2>Se le ha asignado una nueva llamada</h2>
        <p><strong>Número de caso:</strong> {datos.IdLlamada}</p>
        <p><strong>Asunto:</strong> {datos.Asunto}</p>
        <p><strong>Asignado por:</strong> {reasignadoPor}</p>
        <p>Por favor ingrese al sistema para dar seguimiento.</p>
        <br>
        <small>Capris Médica – Sistema de soporte</small>
    ";

            _enviarCorreo.EnviarCorreo(
                "Nueva llamada asignada",
                cuerpo,
                datos.CorreoUsuario
            );
        }



        #region obtener estado llamada
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

        #endregion



        [HttpPost]
        [Route("actualizarLlamada")]
        public async Task<IActionResult> ActualizarLlamada([FromBody] LlamadaModel llamada)
        {
            try
            {
                var nombreUser = User.FindFirst("nombre").Value;

                var registradoPor = int.Parse(User.FindFirst("id").Value);

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

                    RegistrarHistorialLlamada(
                             idLlamada: llamada.IdLlamada,
                             accion: "Actualizacion",
                             descripcion: $"Llamada editada por el usuario {nombreUser}.",
                             usuarioAfectado: llamada.UsuarioId,
                             registradoPor: registradoPor
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
                var nombreUser = User.FindFirst("nombre").Value;

                var registradoPor = int.Parse(User.FindFirst("id").Value);
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@IdLlamada", idLlamada);

                    var resultado = await context.QueryFirstOrDefaultAsync<int>(
                        "sp_eliminar_llamada",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );


                    RegistrarHistorialLlamada(
                             idLlamada: idLlamada,
                             accion: "Eliminacion",
                             descripcion: $"Llamada eliminada por el usuario {nombreUser}.",
                             usuarioAfectado: registradoPor,
                             registradoPor: registradoPor
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


        [Authorize]
        [HttpPost]
        [Route("finalizarLlamada")]
        public async Task<IActionResult> FinalizarLlamada([FromBody] FinalizarLlamadaModel modelo)
        {
            try
            {
                
                var registradoPor = int.Parse(User.FindFirst("id").Value);
                var nombreUser = User.FindFirst("nombre").Value;
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
                    if(modelo.CorreoEnviado)
                    {
                        EnviarCorreoNotificacion("Encuesta de satisfacción", modelo.IdLlamada);
                    }

                    _historialLlamada.RegistrarHistorial(
                    modelo.IdLlamada,
                    "LLamada Finalizada",
                    $"El usuario {nombreUser} finalizo la llamada. Comentario: {modelo.DescripcionSolucion}",
                    registradoPor,
                    registradoPor
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
        [Authorize]
        [HttpPost]
        [Route("Visita/Registrar")]
        public async Task<IActionResult> RegistrarVisita([FromBody] RegistrarVisitaModel model)
        {
            try
            {
                var nombreUser = User.FindFirst("nombre").Value;

                var registradoPor = int.Parse(User.FindFirst("id").Value);
                using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var parametros = new DynamicParameters();
                parametros.Add("@IdLlamada", model.IdLlamada);
                parametros.Add("@UsuarioId", model.UsuarioId);
                parametros.Add("@FechaVisita", model.FechaVisita);
                parametros.Add("@HoraInicio", model.HoraInicio);
                parametros.Add("@Comentario", model.Comentario);

                await context.ExecuteAsync(
                    "sp_registrar_visita",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                        _historialLlamada.RegistrarHistorial(
                   model.IdLlamada,
                   "Visita registrada",
                   $"El usuario {nombreUser} registró una visita. Comentario: {model.Comentario}",
                   model.UsuarioId,
                   registradoPor
               );

                return Ok(new
                {
                    success = true,
                    mensaje = "Visita registrada correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al registrar visita: " + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("reasignar")]
        public async Task<IActionResult> Reasignar([FromBody] ReasignarLlamadaModel model)
        {
            try
            {
                using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var parametros = new DynamicParameters();
                parametros.Add("@IdLlamada", model.IdLlamada);
                parametros.Add("@NuevoUsuarioId", model.NuevoUsuarioId);
                parametros.Add("@RealizadoPor", model.RealizadoPor);
                parametros.Add("@Comentario", model.Comentario);

                await context.ExecuteAsync(
                    "sp_reasignar_llamada",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                string nombreUsuario = await obtenerNombreUsuario(model.NuevoUsuarioId);
                _historialLlamada.RegistrarHistorial(
                    model.IdLlamada,
                    "Reasignación",
                    $"Asignado al usuario {nombreUsuario}. Comentario: {model.Comentario}",
                    model.NuevoUsuarioId,
                    model.RealizadoPor       
                );
                EnviarCorreoReasignacion(model.IdLlamada, nombreUsuario);


                return Ok(new
                {
                    success = true,
                    mensaje = "Llamada reasignada correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al reasignar: " + ex.Message
                });
            }
        }

        private async Task<string> obtenerNombreUsuario(int id) {

            using var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            return await connection.QueryFirstOrDefaultAsync<string>(
                "sp_usuario_obtener_nombre",
                new { IdUsuario = id },
                commandType: CommandType.StoredProcedure
            ) ?? "Usuario desconocido";
        }


        private EncuetaModel ObtenerCorreoClientePorIdLlamada(int idLlamada)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new
                {
                    IdLlamada = idLlamada
                };

                var resultado = context.QueryFirstOrDefault<EncuetaModel>(
                    "sp_obtener_datos_correo",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return resultado;
            }
        }

        [HttpPost]
        [Route("detalles")]
        public async Task<IActionResult> ObtenerDetallesLlamada([FromBody] RequestIdLlamada request)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new { IdLlamada = request.IdLlamada };

                    using (var multi = await context.QueryMultipleAsync(
                        "sp_llamada_detalles",
                        parametros,
                        commandType: CommandType.StoredProcedure))
                    {
                        var llamada = await multi.ReadFirstOrDefaultAsync<LlamadaModel>();
                        var historial = (await multi.ReadAsync<LlamadaHistorialModel>()).ToList();
                        var visitas = (await multi.ReadAsync<VisitaModel>()).ToList();

                        return Ok(new
                        {
                            success = true,
                            llamada,
                            historial,
                            visitas
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = ex.Message });
            }
        }


        private void EnviarCorreoNotificacion(string asunto, int idLlamada)
        {
            EncuetaModel datos = ObtenerCorreoClientePorIdLlamada(idLlamada);

            if (datos == null || string.IsNullOrWhiteSpace(datos.Correo))
                return;

            // Leer plantilla de archivo HTML
            string cuerpo = LeerPlantillaHtml("Plantillas/PlantillaEncuentaRedes.html");
            string qrBase64 = LeerPlantillaHtml("Plantillas/qrRedes.txt");
            cuerpo = cuerpo.Replace("{{QRBASE64}}", qrBase64);

            // Reemplazar variables en la plantilla
            cuerpo = cuerpo.Replace("{{NombreCliente}}", datos.NombreCliente)
                           .Replace("{{AtendidoPor}}", datos.NombreUsuario)
                           .Replace("{{AsuntoLlamada}}", datos.Asunto)
                           .Replace("{{NumeroCaso}}", idLlamada.ToString());

            // Enviar correo
            _enviarCorreo.EnviarCorreo(
                asunto,       
                cuerpo,       
                datos.Correo   
            );
        }

        private string LeerPlantillaHtml(string ruta)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), ruta);
            return System.IO.File.ReadAllText(fullPath);
        }
    }
}
