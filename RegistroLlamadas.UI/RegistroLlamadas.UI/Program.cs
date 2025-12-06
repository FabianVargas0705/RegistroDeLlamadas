using RegistroLlamadas.UI.Middleware;
using RegistroLlamadas.UI.Servicios.PermisosServ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PermisosService>();
builder.Services.AddScoped<IPermisosUIService, PermisosUIService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CargarPermisosFilter>();
});

var app = builder.Build();

// Error pages
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();   // <-- NECESARIO para CSS/JS (si no está, usar MapStaticAssets al final)

app.UseSession();

app.UseMiddleware<TokenMiddleware>();   // <--- Intercepta TODAS las peticiones

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

// Esto va al FINAL
app.MapStaticAssets();

app.Run();
