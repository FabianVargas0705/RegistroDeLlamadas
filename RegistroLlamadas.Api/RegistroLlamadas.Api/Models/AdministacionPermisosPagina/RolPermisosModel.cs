namespace RegistroLlamadas.Api.Models.AdministacionPermisosPagina
{
    public class RolPermisosModel
    {
        public int RoleId { get; set; }
        public string NombreRol { get; set; } = string.Empty;

        public List<PaginaPermisoModel> Paginas { get; set; } = new();
    }
}
