namespace RegistroLlamadas.Api.Models
{
    public class CatalogosDTO
    {
        public List<CatalogoItemModels> Equipos { get; set; }
        public List<CatalogoItemModels> Centros { get; set; }
        public List<CatalogoItemModels> Usuarios { get; set; }
        public List<CatalogoItemModels> Estados { get; set; }
        public List<ClienteItemModels> Clientes { get; set; }
    }
}
