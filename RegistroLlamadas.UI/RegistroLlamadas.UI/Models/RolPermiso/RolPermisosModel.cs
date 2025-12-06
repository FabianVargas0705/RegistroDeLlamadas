namespace RegistroLlamadas.UI.Models.RolPermiso
{
    public class RolPermisosModel
    {
        public int RoleId { get; set; }
        public string NombreRol { get; set; } = string.Empty;

        public List<PaginaPermisoModel> Paginas { get; set; } = new();
    }
}
