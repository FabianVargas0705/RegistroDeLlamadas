namespace RegistroLlamadas.Api.Models
{
    public class RequestObtenerLlamada
    {
        public int? IdLlamada { get; set; }
        public DateTime? Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Estado { get; set; }
        public string? Buscar { get; set; }
    }
}
