
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using RegistroLlamadas.Api.Servicios.Correo;
using System.Data;
using Utiles;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860


namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EnvioCorreo _emailService;

        public UsuarioController(IConfiguration configuration, EnvioCorreo emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }

        #region Obtener Usuarios
        [HttpGet]
        [Route("obtenerUsuarios")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var usuarios = await context.QueryAsync<UsuarioDTO>(
                        "sp_ObtenerUsuarios",
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(usuarios);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener usuarios: " + ex.Message
                });
            }
        }
        #endregion

        #region Obtener Usuario Por Id
        [HttpGet]
        [Route("obtenerUsuarioPorId/{idUsuario}")]
        public async Task<IActionResult> ObtenerUsuarioPorId(int idUsuario)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var usuario = await context.QueryFirstOrDefaultAsync<UsuarioDTO>(
                        "sp_ObtenerUsuarioPorId",
                        new { IdUsuario = idUsuario },
                        commandType: CommandType.StoredProcedure
                    );

                    if (usuario == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            mensaje = "Usuario no encontrado"
                        });
                    }

                    return Ok(usuario);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al obtener el usuario: " + ex.Message
                });
            }
        }
        #endregion

        #region Registrar Usuario
        [HttpPost]
        [Route("registrarUsuario")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioDTO usuario)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@Usuario", usuario.Usuario);
                    parametros.Add("@Nombre", usuario.Nombre);
                    parametros.Add("@PrimerApellido", usuario.PrimerApellido);
                    parametros.Add("@SegundoApellido", usuario.SegundoApellido);
                    parametros.Add("@Correo", usuario.Correo);
                    parametros.Add("@Telefono", usuario.Telefono);
                    parametros.Add("@EstadoId", usuario.EstadoId);
                    parametros.Add("@RolId", usuario.RolId);
                    parametros.Add("@ConTemp", 1);

                    string passwordPlano = GenerarContrasenaAleatoria(10);
                    string passwordEncrypt = EncriptarContrasena(passwordPlano);

                    parametros.Add("@Contrasena", passwordEncrypt);
                    parametros.Add("@IdUsuarioGenerado", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await context.ExecuteAsync(
                        "sp_RegistrarUsuario",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    var idGenerado = parametros.Get<int>("@IdUsuarioGenerado");

                    EnviarCorreoNotificacion(passwordPlano, usuario.Usuario, passwordPlano, usuario.Correo);

                    return Ok(new
                    {
                        success = true,
                        idUsuario = idGenerado,
                        mensaje = "Usuario registrado correctamente"
                    });
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("usuario"))
                {
                    return StatusCode(400, new
                    {
                        success = false,
                        code = "USUARIO_DUPLICADO",
                        mensaje = "El nombre de usuario ya existe."
                    });
                }

                if (ex.Message.Contains("correo"))
                {
                    return StatusCode(400, new
                    {
                        success = false,
                        code = "CORREO_DUPLICADO",
                        mensaje = "El correo electrónico ya está registrado."
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    code = "ERROR_SQL",
                    mensaje = "Error en la base de datos."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    code = "ERROR_GENERAL",
                    mensaje = "Ocurrió un error inesperado al registrar el usuario."
                });
            }
        }
        #endregion


        #region actualizarContrasena


        [HttpPut]
        [Route("actualizarContrasena")]
        public async Task<IActionResult> ActualizarContrasena([FromBody] ActualizarContrasenaDTO actualizarContrasena)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
              

                    var parametros = new DynamicParameters();
                    parametros.Add("@IdUsuario", actualizarContrasena.IdUsuario);
                    parametros.Add("@Contrasena", actualizarContrasena.NuevaContrasena);

                    await context.ExecuteAsync(
                        "sp_ActualizarContrasenaUsuario",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Contraseña actualizada correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al actualizar la contraseña: " + ex.Message
                });
            }
        }


        #endregion

        private void EnviarCorreoNotificacion(string NombreUsuario,string Usuario,string contrasenaTemporal,string correo)
        {
            string cuerpo = LeerPlantillaHtml("Plantillas/PlantillaContrasenaTemporal.html");

            cuerpo = cuerpo
                .Replace("{{NombreUsuario}}", NombreUsuario)
                .Replace("{{Usuario}}", Usuario)
                .Replace("{{ContrasenaTemporal}}", contrasenaTemporal)
                .Replace("{{Anio}}", DateTime.Now.Year.ToString());

            _emailService.EnviarCorreo(
                "Credenciales de acceso al sistema",
                cuerpo,
                correo
            );
        }

        private string LeerPlantillaHtml(string ruta)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), ruta);
            return System.IO.File.ReadAllText(fullPath);
        }
        #region Actualizar Usuario
        [HttpPut]
        [Route("actualizarUsuario")]
        public async Task<IActionResult> ActualizarUsuario([FromBody] UsuarioDTO usuario)
        {
            try
            {
                using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

                var parametros = new DynamicParameters();
                parametros.Add("@IdUsuario", usuario.IdUsuario);
                parametros.Add("@Usuario", usuario.Usuario);
                parametros.Add("@Nombre", usuario.Nombre);
                parametros.Add("@PrimerApellido", usuario.PrimerApellido);
                parametros.Add("@SegundoApellido", usuario.SegundoApellido);
                parametros.Add("@Correo", usuario.Correo);
                parametros.Add("@Telefono", usuario.Telefono);
                parametros.Add("@EstadoId", usuario.EstadoId);
                parametros.Add("@RolId", usuario.RolId);

                await context.ExecuteAsync(
                    "sp_ActualizarUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    success = true,
                    mensaje = "Usuario actualizado correctamente"
                });
            }
            catch (SqlException ex)
            {
               
                if (ex.Message.Contains("CORREO_DUPLICADO"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        code = "CORREO_DUPLICADO",
                        mensaje = "El correo ya pertenece a otro usuario."
                    });
                }

                if (ex.Message.Contains("USUARIO_DUPLICADO"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        code = "USUARIO_DUPLICADO",
                        mensaje = "El usuario ya pertenece a otro usuario."
                    });
                }
                if (ex.Message.Contains("USUARIO_NO_EXISTE"))
                {
                    return NotFound(new
                    {
                        success = false,
                        mensaje = "El usuario no existe."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    code = "ERROR_SQL",
                    mensaje = "Error al actualizar el usuario."
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

        #region Eliminar Usuario
        [HttpDelete]
        [Route("eliminarUsuario/{idUsuario}")]
        public async Task<IActionResult> EliminarUsuario(int idUsuario)
        {
            try
            {
                using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    await context.ExecuteAsync(
                        "sp_EliminarUsuario",
                        new { IdUsuario = idUsuario },
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        mensaje = "Usuario eliminado correctamente"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Error al eliminar el usuario: " + ex.Message
                });
            }
        }
        #endregion


        #region Generar contrseña aleatoria y mandar por correo

        private string GenerarContrasenaAleatoria(int longitud)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            var contrasena = new char[longitud];
            for (int i = 0; i < longitud; i++)
            {
                contrasena[i] = caracteres[random.Next(caracteres.Length)];
            }
            return new string(contrasena);
        }

        #endregion
    

    #region encriptar contraseña
    private string EncriptarContrasena(string contrasena)
        {
            var encryption = new Helper();
            return encryption.Encrypt(contrasena);
        }
    }

        #endregion
    }

