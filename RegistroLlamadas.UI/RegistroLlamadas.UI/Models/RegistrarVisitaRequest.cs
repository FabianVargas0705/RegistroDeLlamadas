namespace RegistroLlamadas.UI.Models
{
    public class RegistrarVisitaRequest
    {
        public int IdLlamada { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaVisita { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public string Comentario { get; set; }
    }
}
