namespace RegistroLlamadas.UI.Controllers
{
    public class GuardarPermisosRequest
    {
        public int RoleId { get; set; }
        public List<int> PaginasSeleccionadas { get; set; }
    }
}