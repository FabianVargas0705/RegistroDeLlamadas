using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using RegistroLlamadas.Api.Models;

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UsuarioController(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [Route("obtener")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            using var db = Connection;

            var result = await db.QueryAsync<UsuarioModel>(
                "sp_obtener_usuarios",
                commandType: CommandType.StoredProcedure
            );

            return Ok(result);
        }

        [Route("insertar")]
        public async Task<IActionResult> InsertarUsuario([FromBody] UsuarioModel usuario)
        {
            using var db = Connection;

            await db.ExecuteAsync(
                "sp_insertar_usuario",
                new
                {
                    usuario.Nombre,
                    usuario.PrimerApellido,
                    usuario.SegundoApellido,
                    usuario.Correo,
                    usuario.Telefono,
                    usuario.NombreUsuario,
                    usuario.Contrasenna,
                    usuario.IdRol
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "Usuario insertado correctamente" });
        }

        [Route("actualizar")]
        public async Task<IActionResult> ActualizarUsuario([FromBody] UsuarioModel usuario)
        {
            using var db = Connection;

            await db.ExecuteAsync(
                "sp_actualizar_usuario",
                new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.PrimerApellido,
                    usuario.SegundoApellido,
                    usuario.Correo,
                    usuario.Telefono,
                    usuario.NombreUsuario,
                    usuario.IdRol
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "Usuario actualizado correctamente" });
        }

        [Route("eliminar/{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            using var db = Connection;

            await db.ExecuteAsync(
                "sp_eliminar_usuario",
                new { IdUsuario = id },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }

        [Route("api/[controller]")]
        public class UsuarioController : ControllerBase
        {
        }
    }
}
