using Microsoft.AspNetCore.Mvc.Filters;

namespace RegistroLlamadas.UI.Servicios.PermisosServ
{
    public class CargarPermisosFilter : IAsyncActionFilter
    {
        private readonly IPermisosUIService _permisos;

        public CargarPermisosFilter(IPermisosUIService permisos)
        {
            _permisos = permisos;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await _permisos.CargarPermisosAsync();
            await next();
        }
    }
}
