namespace RegistroLlamadas.UI.Models
{
    public class FinalizarLlamadaModel
    {
        public int IdLlamada { get; set; }
        public string DescripcionSolucion { get; set; }
        public string HoraFinal { get; set; }
        public bool CorreoEnviado { get; set; }
    }
}
