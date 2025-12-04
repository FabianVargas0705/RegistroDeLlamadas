namespace RegistroLlamadas.UI.Models
{
    public class CatalogosDTO
    {
        public List<CatalogoItem> Equipos { get; set; }
        public List<CatalogoItem> Centros { get; set; }
        public List<CataogoUsuarioModel> Usuarios { get; set; }
        public List<CatalogoItem> Estados { get; set; }
        public List<ClienteItem> Clientes { get; set; }
    }
}
