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
            var correoSMTP = _configuration["Valores:CorreoSMTP"]!;
            var contrasennaSMTP = _configuration["Valores:ContrasennaSMTP"]!;

            if (string.IsNullOrEmpty(contrasennaSMTP))
                return;

            var mensaje = new MailMessage
            {
                From = new MailAddress(correoSMTP, "Capris Médica – Soporte Técnico"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoSMTP, contrasennaSMTP),
                EnableSsl = true
            };

            smtp.Send(mensaje);
        }
    }
}
