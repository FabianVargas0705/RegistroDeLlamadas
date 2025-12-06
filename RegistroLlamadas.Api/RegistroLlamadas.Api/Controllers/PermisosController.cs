using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models.AdministacionPermisosPagina;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PermisosController(IConfiguration configuration)
        {
            _config = configuration;
        }


        [HttpGet("obtener/{roleId}")]
        public async Task<IActionResult> ObtenerPermisos(int roleId)
        {
            using var connection = new SqlConnection(_config["ConnectionStrings:BDConnection"]);

            var datos = await connection.QueryAsync<PaginaPermisoModel>(
                "sp_obtener_permisos_por_rol",
                new { RoleId = roleId },
                commandType: CommandType.StoredProcedure);

            return Ok(datos);
        }


        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarPermisos([FromBody] RequestGuardarPermisos req)
        {
            using var connection = new SqlConnection(_config["ConnectionStrings:BDConnection"]);

            string paginasCsv = string.Join(",", req.PaginasSeleccionadas);

            await connection.ExecuteAsync(
                "sp_permisos_guardar",
                new { RoleId = req.RoleId, PaginasCSV = paginasCsv },
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { success = true, mensaje = "Permisos actualizados correctamente" });
        }
        // GET: api/<PermisosController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PermisosController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PermisosController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PermisosController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PermisosController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
