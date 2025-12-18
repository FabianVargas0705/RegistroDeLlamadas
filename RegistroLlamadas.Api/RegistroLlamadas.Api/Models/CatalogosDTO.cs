namespace RegistroLlamadas.Api.Models
{
    public class CatalogosDTO
    {
        public List<EquipoItem> Equipos { get; set; }
        public List<CatalogoItemModels> Centros { get; set; }
        public List<CataogoUsuarioModel> Usuarios { get; set; }
        public List<CatalogoItemModels> Estados { get; set; }
        public List<ClienteItemModels> Clientes { get; set; }

        public List<RolItem> Roles { get; set; }
    }
}
