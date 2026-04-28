using Microsoft.EntityFrameworkCore;
using Gymio.Data;
using Gymio.Models;
using Gymio.Interfaces;

namespace Gymio.Services
{
    public class AuthService : IAuthService
    {
        private readonly GymioDbContext _context;

        public AuthService(GymioDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> LoginAsync(string email, string passwordPlana)
        {
            // buscamos si existe un empleado activo con ese correo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Activo);

            if (usuario == null)
            {
                return null; // si no existe o no está activo, no se puede autenticar
            }

            // comparamos con la bibliioteca instalada la contraseña plana contra el Hash de la base de datos
            bool esValida = BCrypt.Net.BCrypt.Verify(passwordPlana, usuario.PasswordHash);

            return esValida ? usuario : null;
        }

        public async Task<bool> CrearUsuarioFuerteAsync(Usuario nuevoUsuario, string passwordPlana)
        {
            // verificamos que el correo no esté duplicado
            if (await _context.Usuarios.AnyAsync(u => u.Email == nuevoUsuario.Email))
            {
                return false;
            }

            // generamosel hash de la contraseña usando la biblioteca BCrypt.Net-Next
            nuevoUsuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlana);
            nuevoUsuario.Activo = true;

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}