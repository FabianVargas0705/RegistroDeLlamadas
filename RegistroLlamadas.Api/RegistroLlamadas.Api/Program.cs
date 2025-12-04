using Microsoft.IdentityModel.Tokens;
using RegistroLlamadas.Api.Middleware;
using RegistroLlamadas.Api.Servicios.Correo;
using RegistroLlamadas.Api.Servicios.Historial;
using RegistroLlamadas.Api.Servicios.ServBitacora;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EnvioCorreo>();
builder.Services.AddScoped<HistorialLlamada>();
string key = builder.Configuration["Valores:KeyJWT"]!;

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<RequestResponseLoggingMiddleware>();


app.MapControllers();

app.Run();
