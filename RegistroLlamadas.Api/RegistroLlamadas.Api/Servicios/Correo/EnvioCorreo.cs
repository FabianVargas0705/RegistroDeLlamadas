using Dapper;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace RegistroLlamadas.Api.Servicios.Correo
{
    public class EnvioCorreo
    {
        private readonly IConfiguration _configuration;

        public EnvioCorreo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void EnviarCorreo(string subject, string body, string destinatario)
        {
            using var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var query = @"
                INSERT INTO ColaCorreos (Destinatario, Asunto, Cuerpo)
                VALUES (@Destinatario, @Asunto, @Cuerpo)";

            connection.Execute(query, new
            {
                Destinatario = destinatario,
                Asunto = subject,
                Cuerpo = body
            });
        }
    }
}
