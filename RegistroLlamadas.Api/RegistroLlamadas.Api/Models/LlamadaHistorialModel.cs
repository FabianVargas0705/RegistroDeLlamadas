namespace RegistroLlamadas.Api.Models
{
    public class LlamadaHistorialModel
    {
        public int IdHistorial { get; set; }
        public int IdLlamada { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public int UsuarioId { get; set; }
        public int RegistradoPor { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string RegistradoPorNombre { get; set; }
        public string UsuarioAfectadoNombre { get; set; }
    }
}
