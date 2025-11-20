using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace RegistroLlamadas.UI.Middleware
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value.ToLower();

            // Permitir root
            if (path == "/")
            {
                await _next(context);
                return;
            }

            if (path == "/home/login")
            {
                await _next(context);
                return;
            }

            if (path == "/home/logout")
            {
                await _next(context);
                return;
            }

            // Permitir archivos estáticos
            if (path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/images") ||
                path.StartsWith("/lib"))
            {
                await _next(context);
                return;
            }

            // Validar token en sesión
            var token = context.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                var msg = Uri.EscapeDataString("Iniciar sesión");
                context.Response.Redirect($"/Home/Login?mensaje={msg}");
                return;
            }

            // Validar expiración del token
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var exp = jwt.ValidTo.ToLocalTime();

            if (DateTime.Now > exp)
            {
                context.Session.Clear();
                var msg = Uri.EscapeDataString("Sesión expirada");
                context.Response.Redirect($"/Home/Login?mensaje={msg}");
                return;
            }

            await _next(context);
        }
    }
}
