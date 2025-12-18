namespace RegistroLlamadas.Api.Models
{
    public class WhatsAppRequest
    {
        public string Numero { get; set; }            // Número destino
        public string NombreUsuario { get; set; }     // {{1}}
        public string IdLlamada { get; set; }         // {{2}}
        public string Asunto { get; set; }            // {{3}}
        public string NombreCliente { get; set; }     // {{4}}
        public string AtendidoPor { get; set; }       // {{5}}
    }
}
