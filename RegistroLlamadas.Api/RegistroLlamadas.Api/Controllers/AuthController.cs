using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RegistroLlamadas.Api.Models;
using RegistroLlamadas.Api.Models.AdministacionPermisosPagina;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RegistroLlamadas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IConfiguration configuration, IHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }   

        [HttpPost]
        [Route("ValidarSesion")]
        public IActionResult ValidarSesion(ValidarSesionRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {

                var spName = "ValidarSesion";

                var parametros = new DynamicParameters();
                parametros.Add("@CorreoElectronico", usuario.CorreoElectronico);
                parametros.Add("@Contrasenna", usuario.Contrasenna);

                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ValidarSesion", parametros);

                _httpContextAccessor.HttpContext.Items["SPName"] = spName;
                _httpContextAccessor.HttpContext.Items["SPParams"] = parametros;
                if (resultado != null)
                {
                    //JWT
                    resultado.Token = GenerarToken(resultado.ConsecutivoUsuario, resultado.Nombre, resultado.ConsecutivoPerfil);
                    return Ok(resultado);
                }

                return NotFound();
            }
        }
        [HttpGet("permisos/{roleId}")]
        public async Task<IActionResult> ObtenerPermisos(int roleId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parametros = new DynamicParameters();
                    parametros.Add("@RoleId", roleId);

                    var resultado = await connection.QueryAsync<PaginaModels>(
                        "sp_obtener_permisos_por_rol",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(resultado);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = ex.Message });
            }
        }

        private string GenerarToken(int usuarioId, string nombre, int rol)
        {
            var key = _configuration["Valores:KeyJWT"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", usuarioId.ToString()),
                new Claim("nombre", nombre),
                new Claim("rol", rol.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
