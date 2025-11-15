namespace RegistroLlamadas.Api.Models
{
    public class ClienteModel
    {
        public int IdCliente { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public int EstadoId { get; set; }
    }
}
