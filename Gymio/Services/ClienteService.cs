using Gymio.Data;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class ClienteService
    {
        private readonly GymioDbContext _context;


        public ClienteService(GymioDbContext context)
        {
            _context = context;
        }


        public async Task<bool> RegistrarClienteAsync(Cliente nuevoCliente)
        {
            if (string.IsNullOrWhiteSpace(nuevoCliente.Nombre) || string.IsNullOrWhiteSpace(nuevoCliente.Apellido))
                return false;


            string iniciales = $"{nuevoCliente.Nombre.Substring(0, 1)}{nuevoCliente.Apellido.Substring(0, 1)}".ToUpper();
            string timestamp = DateTime.Now.ToString("yyMM");
            int randomNum = new Random().Next(1000, 9999);

            nuevoCliente.CodigoQR = $"GYM-{iniciales}-{timestamp}-{randomNum}";
            nuevoCliente.Email ??= "";
            nuevoCliente.Telefono ??= "";
            nuevoCliente.FechaRegistro = DateTime.Now;
            nuevoCliente.Activo = true;

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync(); // Usamos la versión Async

            return true;
        }

        public async Task<List<Cliente>> ObtenerClientesAsync(string terminoBusqueda = "")
        {

            var consulta = _context.Clientes.AsQueryable();

        
            if (!string.IsNullOrWhiteSpace(terminoBusqueda))
            {
                consulta = consulta.Where(c =>
                    c.Nombre.Contains(terminoBusqueda) ||
                    c.Apellido.Contains(terminoBusqueda) ||
                    c.CodigoQR.Contains(terminoBusqueda));
            }

          
            return await consulta.OrderByDescending(c => c.FechaRegistro).ToListAsync();
        }
    }
}