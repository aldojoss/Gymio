using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface IAuthService
    {
        // devuelve el usuario si las credenciales son correctas, o null si fallan
        Task<Usuario?> LoginAsync(string email, string passwordPlana);

        // hashea la contraseña antes de guardar al empleado en la base de datos
        Task<bool> CrearUsuarioFuerteAsync(Usuario nuevoUsuario, string passwordPlana);
    }
}