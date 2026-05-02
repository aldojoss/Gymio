using Microsoft.EntityFrameworkCore;
using Gymio.Data;
using Gymio.Models;
using Gymio.Interfaces;


namespace Gymio.Services
{
    public class AuthService : IAuthService
    {
        private readonly GymioDbContext _context;
        private readonly IClienteService _clienteService;

        public AuthService(GymioDbContext context, IClienteService clienteService)
        {
            _context = context;
            this._clienteService = clienteService;
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
            string correoLimpio = nuevoUsuario.Email.Trim().ToLower();
            nuevoUsuario.Email = correoLimpio;

            bool existeEnUsuarios = await _context.Usuarios.AnyAsync(u => u.Email == correoLimpio);

            var todosLosClientes = await _clienteService.ObtenerClientesAsync();
            bool existeEnClientes = todosLosClientes.Any(c => c.Email != null && c.Email.Trim().ToLower() == correoLimpio);

   
            if (existeEnUsuarios || existeEnClientes)
            {
                return false;
            }

         
            nuevoUsuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlana);
            nuevoUsuario.Activo = true;

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LoginResult> AutenticarHibridoAsync(string email, string password, string tipoAcceso)
        {
           
            if (tipoAcceso == "Staff")
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Activo);

                if (usuario != null && BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                {
                    return new LoginResult
                    {
                        Exito = true,
                        Rol = usuario.Rol,
                        Id = usuario.Id,
                        Nombre = usuario.NombreCompleto
                    };
                }
            }
    
            else if (tipoAcceso == "Socio")
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email && c.Activo);

                if (cliente != null && BCrypt.Net.BCrypt.Verify(password, cliente.PasswordHash))
                {
                    return new LoginResult
                    {
                        Exito = true,
                        Rol = "Cliente",
                        Id = cliente.Id,
                        Nombre = $"{cliente.Nombre} {cliente.Apellido}"
                    };
                }
            }

            
            return new LoginResult
            {
                Exito = false,
                Mensaje = "Credenciales inválidas o tipo de cuenta incorrecto"
            };
        }
    }
}