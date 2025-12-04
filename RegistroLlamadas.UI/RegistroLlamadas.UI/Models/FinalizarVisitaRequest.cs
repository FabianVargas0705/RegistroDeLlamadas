namespace RegistroLlamadas.UI.Models
{
    public class FinalizarVisitaRequest
    {
        public int IdVisita { get; set; }
        public string Descripcion { get; set; }
        public string HoraFinal { get; set; }

        public bool EnviarCorreo { get; set; }
    }
}
