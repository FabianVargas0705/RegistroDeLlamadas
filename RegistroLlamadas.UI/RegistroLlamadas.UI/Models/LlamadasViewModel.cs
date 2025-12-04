namespace RegistroLlamadas.UI.Models
{
    public class LlamadasViewModel
    {
        public List<LlamadaModel> Llamadas { get; set; }
        public List<VisitaModel> Visitas { get; set; }

        public List<CatalogoItem> Equipos { get; set; }
        public List<CatalogoItem> Centros { get; set; }
        public List<CataogoUsuarioModel> Usuarios { get; set; }
        public List<CatalogoItem> Estados { get; set; }
        public List<ClienteItem> Clientes { get; set; }
    }
}
