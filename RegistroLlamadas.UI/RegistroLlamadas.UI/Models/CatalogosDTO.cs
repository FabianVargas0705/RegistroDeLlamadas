namespace RegistroLlamadas.UI.Models
{
    public class CatalogosDTO
    {
        public List<EquipoItem> Equipos { get; set; }
        public List<CatalogoItem> Centros { get; set; }
        public List<CataogoUsuarioModel> Usuarios { get; set; }
        public List<CatalogoItem> Estados { get; set; }
        public List<ClienteItem> Clientes { get; set; }

        public List<RolItem> Roles { get; set; }
    }
}
