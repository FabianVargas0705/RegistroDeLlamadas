using RegistroLlamadas.Api.Models.Middleware;

namespace RegistroLlamadas.Api.Servicios.ServBitacora
{
    public interface IBitacoraService
    {
        Task GuardarBitacora(Bitacora bitacora);

    }
}
