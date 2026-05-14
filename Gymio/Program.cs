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
using Radzen;
using System.Security.Claims;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// configuración de SQL Server
//lo que se hace aqui es usar el DbContextFactory, que es como un servicio de instancias del dbcontext, para que cada vez que se necesite una instancia del dbcontext, se cree una nueva, lo cual es importante para evitar problemas de concurrencia y para que cada servicio tenga su propia instancia del dbcontext.

builder.Services.AddDbContextFactory<GymioDbContext>(options =>
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

// Configuración de Firebase
var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebaseCredentialsPath) && System.IO.File.Exists(firebaseCredentialsPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseCredentialsPath),
        ProjectId = firebaseProjectId
    });
}

builder.Services.AddSingleton(provider =>
{
    // Construye el cliente de Firestore, asume que la variable de entorno GOOGLE_APPLICATION_CREDENTIALS
    // puede estar configurada o usa el builder default si FirebaseApp ya tiene la credencial
    if (!string.IsNullOrEmpty(firebaseCredentialsPath))
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseCredentialsPath);
    }
    return FirestoreDb.Create(firebaseProjectId);
});

// Registrar IFirebaseService
builder.Services.AddScoped<IFirebaseService, FirebaseService>();

// Servicios de negocio
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IRutinaService, RutinaService>();
builder.Services.AddScoped<IChatService,ChatService >();
builder.Services.AddScoped<IRegistroEntrenamientoService, RegistroEntrenamientoService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPlanillaService, PagoPlanillaService>();
builder.Services.AddRadzenComponents();

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

app.UseStatusCodePagesWithReExecute("/not-found");
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



app.MapGet("/api/crear-prueba", async (Gymio.Data.GymioDbContext db) =>
{
    string passHash = BCrypt.Net.BCrypt.HashPassword("123456");

    var entrenador = new Gymio.Models.Usuario { Nombre = "Entrenador Test", Email = "coach@gymio.com", PasswordHash = passHash, Rol = "Entrenador", Activo = true, FechaRegistro = DateTime.Now };
    var cliente = new Gymio.Models.Cliente { Nombre = "Cliente Test", Email = "cliente@gymio.com", PasswordHash = passHash, Rol = "Cliente", Activo = true, FechaRegistro = DateTime.Now, CodigoQR = "QR-TEST-001" };

    if (!db.Usuarios.Any(u => u.Email == "coach@gymio.com")) db.Usuarios.Add(entrenador);
    if (!db.Clientes.Any(c => c.Email == "cliente@gymio.com")) db.Clientes.Add(cliente);
    
    await db.SaveChangesAsync();

    return Results.Ok(new { Mensaje = "Cuentas creadas", Entrenador = "coach@gymio.com / 123456", Cliente = "cliente@gymio.com / 123456" });
});

app.Run();