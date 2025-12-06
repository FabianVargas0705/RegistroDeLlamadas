namespace RegistroLlamadas.UI.Models
{
    public class RequestObtenerLlamada
    {
        public int IdLlamada { get; set; } = 0;
        public DateTime? Fecha { get; set; }

        public int? UsuarioId { get; set; }

        public string Buscar { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

}
