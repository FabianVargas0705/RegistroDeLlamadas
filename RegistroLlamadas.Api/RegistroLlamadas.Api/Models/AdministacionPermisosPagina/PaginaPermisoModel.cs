namespace RegistroLlamadas.Api.Models.AdministacionPermisosPagina
{
    public class PaginaPermisoModel
    {
        public int PaginaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public bool Asignada { get; set; }
    }
}
