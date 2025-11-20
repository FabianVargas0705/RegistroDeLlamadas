using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using RegistroLlamadas.Api.Models;

namespace RegistroLlamadas.Api.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IConfiguration _config;

        public ClienteController1(IConfiguration config)
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
        public async Task<IActionResult> ObtenerClientes()
        {
            using var db = Connection;

            var clientes = await db.QueryAsync<ClienteModel>(
                "sp_obtener_clientes",
                commandType: CommandType.StoredProcedure
            );

            return Ok(clientes);
        }

        [Route("insertar")]
        public async Task<IActionResult> InsertarCliente([FromBody] ClienteModel cliente)
        {
            using var db = Connection;

            var result = await db.ExecuteAsync(
                "sp_insertar_cliente",
                new
                {
                    cliente.Nombre,
                    cliente.PrimerApellido,
                    cliente.SegundoApellido,
                    cliente.Telefono,
                    cliente.Correo
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "Cliente insertado correctamente" });
        }

        [Route("actualizar")]
        public async Task<IActionResult> ActualizarCliente([FromBody] ClienteModel cliente)
        {
            using var db = Connection;

            await db.ExecuteAsync(
                "sp_actualizar_cliente",
                new
                {
                    cliente.IdCliente,
                    cliente.Nombre,
                    cliente.PrimerApellido,
                    cliente.SegundoApellido,
                    cliente.Telefono,
                    cliente.Correo
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "Cliente actualizado correctamente" });
        }

        [Route("eliminar/{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            using var db = Connection;

            await db.ExecuteAsync(
                "sp_eliminar_cliente",
                new { IdCliente = id },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "Cliente eliminado correctamente" });
        }

        [Route("api/[controller]")]
        public class ClienteController : ControllerBase
        {
        }
    }
}
