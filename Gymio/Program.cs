using Gymio.Client.Pages;
using Gymio.Data;
using Gymio.Hubs;
using Gymio.Interfaces;
using Gymio.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// configuración de SQL Server
builder.Services.AddDbContext<GymioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GymioSQLConnection")));

// configuración de MongoDB

var mongoConnectionString = builder.Configuration.GetConnectionString("GymioMongoConnection");

// registramos el cliente de Mongo como Singleton (una sola conexión para toda la app)
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

// registramos la Base de Datos específica para inyectarla en los servicios
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("GymioMongoDB"); // nombre de la bd en mongo compass
});

// Servicios de negocio
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IRutinaService, RutinaService>();
builder.Services.AddScoped<IChatService,ChatService >();
builder.Services.AddSignalR();


// =====================================================================
// aqui implementamos el servicio de seguridad por cookies, que es el más sencillo para este tipo de aplicaciones.
// 
// =====================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/login"; //si el usuario no esta autenticado osea no tiene la cookie, lo redirige a esta ruta
        option.AccessDeniedPath = "/login"; //si su rol no es el correcto, lo redirige a esta ruta
    }


    );

//agregamos el servicio de autorización y el estado de autenticación en cascada para que los componentes puedan acceder a la información del usuario autenticado.
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


app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<Gymio.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Gymio.Client._Imports).Assembly);

// =====================================================================
// 
// =====================================================================
app.MapPost("/api/login", async ([FromForm] string email, [FromForm] string password, [FromForm] string tipoAcceso, Gymio.Interfaces.IAuthService authService, HttpContext context) =>
{
    var resultado = await authService.AutenticarHibridoAsync(email, password, tipoAcceso);

    if (resultado.Exito)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, resultado.Nombre!),
            new Claim(ClaimTypes.NameIdentifier, resultado.Id.ToString()),
            new Claim(ClaimTypes.Role, resultado.Rol!)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        
        if (resultado.Rol == "Cliente")
        {
            return Results.Redirect("/mi-portal");
        }
        else if (resultado.Rol == "Entrenador")
        {
            return Results.Redirect("/portal-entrenador");
        }
        else
        {
            
            return Results.Redirect("/");
        }
    }

    return Results.Redirect("/login?error=Credenciales incorrectas");
}).DisableAntiforgery();
// =====================================================================

// mapeamos el hub de SignalR para la comunicación en tiempo real del chat
app.MapHub<ChatHub>("/chathub");



app.Run();