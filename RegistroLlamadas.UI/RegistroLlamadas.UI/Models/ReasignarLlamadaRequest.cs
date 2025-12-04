namespace RegistroLlamadas.UI.Models
{
    public class ReasignarLlamadaRequest
    {
        public int IdLlamada { get; set; }
        public int NuevoUsuarioId { get; set; }
        public int RealizadoPor { get; set; }
        public string Comentario { get; set; }
    }
}
