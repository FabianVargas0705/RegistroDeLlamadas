using System.ComponentModel.DataAnnotations.Schema;

namespace RegistroLlamadas.Api.Models
{
    public class DatosUsuarioResponseModel
    {

        [Column("id_usuario")]
        public int ConsecutivoUsuario { get; set; }

        [Column("usuario")]
        public string Identificacion { get; set; } = string.Empty;

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("correo")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Column("contrasena")]
        public string Contrasenna { get; set; } = string.Empty;

        [Column("estado")]
        public string? Estado { get; set; }

        [Column("rol_id")]
        public int ConsecutivoPerfil { get; set; }

        [Column("Contemporal")]
        public bool Contemporal { get; set; }

        [NotMapped]
        public string NombrePerfil { get; set; } = string.Empty;

        [NotMapped]
        public string Token { get; set; } = string.Empty;
    }
}
