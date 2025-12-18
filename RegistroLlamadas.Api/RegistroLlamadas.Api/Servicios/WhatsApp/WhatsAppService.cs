using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace RegistroLlamadas.Api.Servicios.WhatsApp
{
    public class WhatsAppService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public WhatsAppService(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient();
        }

        public async Task<string> EnviarMensaje(string numeroDestino,
    string nombreUsuario,
    string idLlamada,
    string asunto,
    string nombreCliente,
    string atendidoPor)
        {
            string token = _config["WhatsApp:Token"];
            string phoneId = _config["WhatsApp:PhoneNumberId"];
            string url = $"{_config["WhatsApp:ApiUrl"]}/messages";

            var data = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = "notificacion_llamada_asignada",
                    language = new { code = "es" },
                    components = new[]
            {
                new {
                    type = "body",
                    parameters = new object[]
                    {
                        new { type = "text", text = nombreUsuario },
                        new { type = "text", text = idLlamada },
                        new { type = "text", text = asunto },
                        new { type = "text", text = nombreCliente },
                        new { type = "text", text = atendidoPor }
                    }
                }
            }
                }
            };

            string json = JsonConvert.SerializeObject(data);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }
    }
}
