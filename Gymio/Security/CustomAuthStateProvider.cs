using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace Gymio.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // intentamos leer la sesión encriptada del navegador
                var userSessionResult = await _sessionStorage.GetAsync<string>("UsuarioSesion");
                var userSession = userSessionResult.Success ? userSessionResult.Value : null;

                if (string.IsNullOrEmpty(userSession))
                {
                    return new AuthenticationState(_anonymous); // no hay sesión = desconocido
                }

                // si hay sesión, le creamos su "gafete" de acceso
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession),
                    new Claim(ClaimTypes.Role, "Administrador") // más adelante validaremos roles
                };

                var claimsIdentity = new ClaimsIdentity(claims, "GymioAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                // esto evita que la app truene durante la carga inicial de Blazor
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task IniciarSesion(string nombreUsuario)
        {
            await _sessionStorage.SetAsync("UsuarioSesion", nombreUsuario);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim(ClaimTypes.Role, "Administrador")
            };
            var identity = new ClaimsIdentity(claims, "GymioAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task CerrarSesion()
        {
            await _sessionStorage.DeleteAsync("UsuarioSesion");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}