namespace RegistroLlamadas.Api.Models
{
    public class ActualizarContrasenaDTO
    {
        public int IdUsuario { get; set; }

        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
