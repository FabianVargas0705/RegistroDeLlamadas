using Dapper;
using Microsoft.Data.SqlClient;
using RegistroLlamadas.Api.Models;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace RegistroLlamadas.Api.Servicios.Correo
{
    public class ProcesadorColaCorreos : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ProcesadorColaCorreos(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcesarCorreosPendientes();
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        private async Task ProcesarCorreosPendientes()
        {
            using var scope = _serviceProvider.CreateScope();

            using var connection = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var pendientes = await connection.QueryAsync<CorreoPendiente>(
                "SELECT TOP 5 * FROM ColaCorreos WHERE Estado = 'Pendiente' ORDER BY FechaRegistro");

            foreach (var correo in pendientes)
            {
                try
                {
                    await EnviarCorreoSMTP(correo);

                    await connection.ExecuteAsync(@"
                    UPDATE ColaCorreos
                    SET Estado = 'Enviado', FechaEnvio = GETDATE()
                    WHERE Id = @Id", new { correo.Id });
                }
                catch (Exception ex)
                {
                    await connection.ExecuteAsync(@"
                    UPDATE ColaCorreos
                    SET Intentos = Intentos + 1,
                        Error = @Error
                    WHERE Id = @Id",
                        new { correo.Id, Error = ex.Message });
                }
            }
        }

        private async Task EnviarCorreoSMTP(CorreoPendiente correo)
        {
            var smtpUser = _configuration["Valores:CorreoSMTP"];
            var smtpPass = _configuration["Valores:ContrasennaSMTP"];

            var mensaje = new MailMessage
            {
                From = new MailAddress(smtpUser!, "Capris Médica – Soporte Técnico"),
                Subject = correo.Asunto,
                Body = correo.Cuerpo,
                IsBodyHtml = true
            };

            mensaje.To.Add(correo.Destinatario);

            using var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mensaje);
        }

  
    }

}
