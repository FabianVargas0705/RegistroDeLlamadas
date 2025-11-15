namespace RegistroLlamadas.UI.Models
{
    public class RequestObtenerLlamada
    {
        public int IdLlamada { get; set; } = 0;
        public DateTime? Fecha { get; set; }

        public int? UsuarioId { get; set; }
    }
}
