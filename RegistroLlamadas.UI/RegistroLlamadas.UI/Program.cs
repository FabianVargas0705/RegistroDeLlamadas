using RegistroLlamadas.UI.Middleware;
using RegistroLlamadas.UI.Servicios.PermisosServ;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


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


app.UseExceptionHandler("/Error/MostrarError");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseMiddleware<TokenMiddleware>();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
