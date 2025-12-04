namespace RegistroLlamadas.Api.Models
{
    public class ReasignarLlamadaModel
    {
        public int IdLlamada { get; set; }
        public int NuevoUsuarioId { get; set; }
        public int RealizadoPor { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }
}
