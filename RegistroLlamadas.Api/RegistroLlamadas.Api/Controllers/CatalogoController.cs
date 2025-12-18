using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using RegistroLlamadas.Api.Models.AdministacionPermisosPagina;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CatalogoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // GET: api/<CatalogoController>
        [HttpGet]
        [Route("GetCatalogos")]
        public IActionResult GetCatalogos()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var multi = context.QueryMultiple("sp_ObtenerCatalogos", commandType: CommandType.StoredProcedure);

                var result = new CatalogosDTO
                {
                    Equipos = multi.Read<EquipoItem>().ToList(),
                    Centros = multi.Read<CatalogoItemModels>().ToList(),
                    Usuarios = multi.Read<CataogoUsuarioModel>().ToList(),
                    Estados = multi.Read<CatalogoItemModels>().ToList(),
                    Clientes = multi.Read<ClienteItemModels>().ToList(),
                    Roles = multi.Read<RolItem>().ToList()
                };

                return Ok(result);
            }
        }


        [HttpGet("GetPaginas")]
        public async Task<IActionResult> GetPaginas()
        {
            using var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var paginas = await connection.QueryAsync<PaginaModels>(
                "sp_obtener_paginas_activas",
                commandType: CommandType.StoredProcedure
            );

            return Ok(paginas);
        }

        // GET api/<CatalogoController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CatalogoController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CatalogoController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CatalogoController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
