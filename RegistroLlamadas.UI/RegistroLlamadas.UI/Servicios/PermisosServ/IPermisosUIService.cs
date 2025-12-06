namespace RegistroLlamadas.UI.Servicios.PermisosServ
{
    public interface IPermisosUIService
    {
        Task CargarPermisosAsync();
        bool PuedeVer(string ruta);
        bool PuedeVerGrupo(IEnumerable<string> rutas);
    }
}
