namespace RegistroLlamadas.UI.Models
{
    public class RequestObtenerVisita
    {
        public int? IdVisita { get; set; }      
        public DateTime? Fecha { get; set; }    
        public int? UsuarioId { get; set; }
    }
}
