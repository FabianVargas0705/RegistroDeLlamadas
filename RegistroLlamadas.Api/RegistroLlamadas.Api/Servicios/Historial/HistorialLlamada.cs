using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace RegistroLlamadas.Api.Servicios.Historial
{
    public class HistorialLlamada
    {

        private readonly IConfiguration _configuration;

        public HistorialLlamada(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void RegistrarHistorial(int idLlamada, string accion, string descripcion, int? usuarioAfectado, int? registradoPor)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@IdLlamada", idLlamada);
                parametros.Add("@TipoAccion", accion);
                parametros.Add("@Descripcion", descripcion);
                parametros.Add("@UsuarioAfectado", usuarioAfectado);
                parametros.Add("@RegistradoPor", registradoPor);

                context.Execute(
                    "sp_registrar_llamada_historial",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

    }
}
