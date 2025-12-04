namespace RegistroLlamadas.UI.Models
{
    public class VisitasViewModel
    {
        public List<VisitaModel> Visitas { get; set; }
        public List<CataogoUsuarioModel> Usuarios { get; set; }
        public int UsuarioActual { get; set; }
    }
}
