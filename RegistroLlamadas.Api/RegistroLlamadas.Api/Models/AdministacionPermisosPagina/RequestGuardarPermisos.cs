namespace RegistroLlamadas.Api.Models.AdministacionPermisosPagina
{
    public class RequestGuardarPermisos
    {
        public int RoleId { get; set; }
        public List<int> PaginasSeleccionadas { get; set; }
    }
}
