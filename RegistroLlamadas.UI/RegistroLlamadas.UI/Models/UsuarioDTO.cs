namespace RegistroLlamadas.UI.Models
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? PrimerApellido { get; set; }

        public string? SegundoApellido { get; set; }

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public int EstadoId { get; set; }
        public int? RolId { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public string? RolNombre { get; set; }
    }
}
