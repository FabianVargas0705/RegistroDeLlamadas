namespace RegistroLlamadas.Api.Models
{
    public class RegistrarVisitaModel
    {
        public int IdLlamada { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaVisita { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
    }
}
