using Dapper;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models.Middleware;
using System.Data;

namespace RegistroLlamadas.Api.Servicios.ServBitacora
{
    public class BitacoraService : IBitacoraService
    {
        private readonly IConfiguration _configuration;


        public BitacoraService(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public async Task GuardarBitacora(Bitacora bitacora)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UsuarioId", bitacora.UsuarioId);
                    parameters.Add("@Accion", bitacora.Accion);
                    parameters.Add("@Fecha", bitacora.Fecha);
                    parameters.Add("@TablaAfectada", bitacora.TablaAfectada);
                    parameters.Add("@Metodo", bitacora.Metodo);
                    parameters.Add("@Ruta", bitacora.Ruta);
                    parameters.Add("@QueryString", bitacora.QueryString);
                    parameters.Add("@Ip", bitacora.Ip);
                    parameters.Add("@RequestBody", bitacora.RequestBody);
                    parameters.Add("@ResponseBody", bitacora.ResponseBody);
                    parameters.Add("@StatusCode", bitacora.StatusCode);
                    parameters.Add("@TiempoMs", bitacora.TiempoMs);
                    parameters.Add("@Detalles", bitacora.Detalles);
                    parameters.Add("@NombreUsuario", bitacora.NombreUsuario);
                    parameters.Add("@Rol", bitacora.RolUsuario);
                    await connection.ExecuteAsync(
                        "sp_InsertarBitacora",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando bitácora: {ex.Message}");
            }
        }

    }
}
