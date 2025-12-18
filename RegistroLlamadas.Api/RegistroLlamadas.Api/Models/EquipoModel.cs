namespace RegistroLlamadas.Api.Models
{
    public class EquipoModel
    {
        public int IdEquipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Condicion { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string? Activo { get; set; }
        public int? CentroId { get; set; }
        public int? EstadoId { get; set; }
        public string? CentroNombre { get; set; }
        public string? EstadoDescripcion { get; set; }
    }
}
