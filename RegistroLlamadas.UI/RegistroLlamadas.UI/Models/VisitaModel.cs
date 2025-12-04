namespace RegistroLlamadas.UI.Models
{
    public class VisitaModel
    {
        public int IdVisita { get; set; }
        public int IdLlamada { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaVisita { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan? HoraFinal { get; set; }
        public string Comentario { get; set; }
        public int EstadoId { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? RegistradoPor { get; set; }

       
        public string UsuarioNombre { get; set; }
        public string EstadoNombre { get; set; }
        public string ClienteNombre { get; set; }   
        public string CentroNombre { get; set; }     
        public string AsuntoLlamada { get; set; }    
    }
}
