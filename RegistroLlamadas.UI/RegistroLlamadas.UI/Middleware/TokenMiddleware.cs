using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Permitir root
            if (path == "/")
            {
                await _next(context);
                return;
            }

            if (path == "/home/login" ||
                path == "/home/logout" ||
                path == "/home/recuperaracceso")
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

            // ValidTo viene en UTC
            var exp = jwt.ValidTo; // UTC

            if (DateTime.UtcNow > exp)
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