namespace RegistroLlamadas.Api.Models
{
    public class CorreoPendiente
    {
        public int Id { get; set; }
        public string Destinatario { get; set; }
        public string Asunto { get; set; }
        public string Cuerpo { get; set; }
    }
}
