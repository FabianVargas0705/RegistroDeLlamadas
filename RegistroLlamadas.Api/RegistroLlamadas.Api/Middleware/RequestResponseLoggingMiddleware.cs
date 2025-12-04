using RegistroLlamadas.Api.Models.Middleware;
using RegistroLlamadas.Api.Servicios.ServBitacora;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace RegistroLlamadas.Api.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public RequestResponseLoggingMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var watch = Stopwatch.StartNew();

            
            context.Request.EnableBuffering();
            string requestBody = await ReadRequestBody(context.Request);

         
            var originalBodyStream = context.Response.Body;
            using var tempResponseStream = new MemoryStream();
            context.Response.Body = tempResponseStream;

            await _next(context);

            string responseBody = await ReadResponseBody(context.Response);
            watch.Stop();

            // Obtener usuario_id del token
            int usuarioId = ObtenerUsuarioId(context);
            string nombreUsuario = ObtenerNombreUsuario(context);
            string rolUsuario = ObtenerRolUsuario(context);

            
            string accion = DeterminarAccion(context.Request.Method, context.Response.StatusCode);

           
            string tablaAfectada = DeterminarTablaAfectada(context.Request.Path);

            var bitacora = new Bitacora
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Fecha = DateTime.Now,
                TablaAfectada = tablaAfectada,
                Metodo = context.Request.Method,
                Ruta = context.Request.Path,
                QueryString = context.Request.QueryString.Value,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                RequestBody = TruncarTexto(requestBody, 4000),
                ResponseBody = TruncarTexto(responseBody, 4000),
                StatusCode = context.Response.StatusCode,
                TiempoMs = (int)watch.ElapsedMilliseconds,
                NombreUsuario = nombreUsuario,
                RolUsuario = rolUsuario
            };

            // Guardar en la base de datos
            using (var scope = _scopeFactory.CreateScope())
            {
                var bitacoraService = scope.ServiceProvider.GetRequiredService<IBitacoraService>();
                await bitacoraService.GuardarBitacora(bitacora);
            }

            
            await tempResponseStream.CopyToAsync(originalBodyStream);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            try
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                return body;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            try
            {
                response.Body.Position = 0;
                using var reader = new StreamReader(response.Body, leaveOpen: true);
                string text = await reader.ReadToEndAsync();
                response.Body.Position = 0;
                return text;
            }
            catch
            {
                return string.Empty;
            }
        }

        private int ObtenerUsuarioId(HttpContext context)
        {
            var userIdClaim = context.User?.Claims?.FirstOrDefault(c => c.Type == "id");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }


            return 0; 
        }

        private string ObtenerNombreUsuario(HttpContext context)
        {
            var nombreClaim = context.User?.Claims?.FirstOrDefault(c => c.Type == "nombre");
            return nombreClaim?.Value;
        }

        private string ObtenerRolUsuario(HttpContext context)
        {
            var rolClaim = context.User?.Claims?.FirstOrDefault(c => c.Type == "rol");
            return rolClaim?.Value;
        }

        private string DeterminarAccion(string metodo, int statusCode)
        {
            // Si el request falló, registrar como ERROR
            if (statusCode >= 400)
            {
                return "ERROR";
            }

            return metodo switch
            {
                "GET" => "CONSULTA",
                "POST" => "INSERCION",
                "PUT" => "ACTUALIZACION",
                "PATCH" => "ACTUALIZACION",
                "DELETE" => "ELIMINACION",
                _ => "REQUEST"
            };
        }

        private string DeterminarTablaAfectada(PathString path)
        {
            // Extrae el nombre del controlador/recurso de la ruta
            // Ejemplo: /api/citas/123 -> CITAS
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments != null && segments.Length > 1)
            {
                // Asume que el segundo segmento es el recurso (después de "api")
                return segments[1].ToUpper();
            }

            return "API";
        }

        private string TruncarTexto(string texto, int maxLength)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            if (texto.Length <= maxLength)
                return texto;

            return texto.Substring(0, maxLength) + "... [truncado]";
        }
    }
}
