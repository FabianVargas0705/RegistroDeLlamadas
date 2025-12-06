namespace RegistroLlamadas.Api.Models
{
    public class CorreoAsignacionModel
    {
        public int IdLlamada { get; set; }
        public string Asunto { get; set; }
        public string CorreoUsuario { get; set; }
        public string NombreUsuario { get; set; }
    }
}
