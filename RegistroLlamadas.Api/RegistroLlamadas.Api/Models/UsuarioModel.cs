namespace RegistroLlamadas.Api.Models
{
    public class UsuarioModel
    {
        public int ConsecutivoUsuario { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasenna { get; set; } = string.Empty;
        public string? Estado { get; set; }
        public int ConsecutivoPerfil { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
        public string ContrasennaConfirmar { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
