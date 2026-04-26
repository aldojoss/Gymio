using Gymio.Client.Pages;

using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configuración de SQL Server
builder.Services.AddDbContext<GymioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GymioSQLConnection")));

// Configuración de MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("GymioMongoConnection");
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

// Servicios de negocio
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// =====================================================================
// 🔒 1. CONFIGURACIÓN DE SEGURIDAD NATIVA (COOKIES)
// Aquí le decimos a .NET que use Cookies reales y quite el error 401
// =====================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Redirige aquí si no hay sesión
        options.AccessDeniedPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
// =====================================================================

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    })
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

// Activar el Guardián HTTP
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<Gymio.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Gymio.Client._Imports).Assembly);

// =====================================================================
// 🔒 2. API DE SESIÓN (LOGIN Y LOGOUT NATIVO)
// Este es el motor que lee el formulario HTML de Login.razor
// =====================================================================
app.MapPost("/api/login", async (HttpContext context, IAuthService authService) =>
{
    var email = context.Request.Form["email"].ToString();
    var password = context.Request.Form["password"].ToString();

    var usuario = await authService.LoginAsync(email, password);
    if (usuario == null) return Results.Redirect("/login?error=Credenciales incorrectas");

    var claims = new List<Claim> {
        new Claim(ClaimTypes.Name, usuario.NombreCompleto),
        new Claim(ClaimTypes.Role, usuario.Rol ?? "Empleado")
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapGet("/api/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});
// =====================================================================

app.Run();