namespace RegistroLlamadas.UI.Models.RolPermiso
{
    public class RequestGuardarPermisos
    {
        public int RoleId { get; set; }
        public List<int> PaginasSeleccionadas { get; set; }
    }
}
