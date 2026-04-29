using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymio.Services
{
    public class ClienteService : IClienteService
    {
        private readonly GymioDbContext _context;


        public ClienteService(GymioDbContext context)
        {
            _context = context;
        }

        public async Task<int>CargarCantidadClientes()
        {
            return await _context.Clientes.CountAsync();
        }

        public async Task<decimal>CantidadIngresosHoy()
        {
          
            return await _context.Ventas.Where(c => c.FechaVenta.Date == DateTime.Today).SumAsync(c => c.Total);
        }


        public async Task<bool> RegistrarClienteAsync(Cliente nuevoCliente)
        {
            // 1el codigo generamos uno unico para evitar errores al 100*100
            // Formato: GYM-AÑO-MES-DIA-5LetrasAleatorias
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            string identificadorUnico = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();

            nuevoCliente.CodigoQR = $"GYM-{fecha}-{identificadorUnico}";

            // no creqo eu pasepero verificar que no haiga un cliente con el mismo codigo qr
            bool existe = await _context.Clientes.AnyAsync(c => c.CodigoQR == nuevoCliente.CodigoQR);
            if (existe)
            {
                // Si existe, le agregamos unos milisegundos para desempatar
                nuevoCliente.CodigoQR += $"-{DateTime.Now.Millisecond}";
            }

            // guardamos en la base de datos
            nuevoCliente.FechaRegistro = DateTime.Now;
            nuevoCliente.Activo = true;

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Cliente>> ObtenerClientesAsync(string terminoBusqueda = "")
        {

            var consulta = _context.Clientes.AsQueryable();

        
            if (!string.IsNullOrWhiteSpace(terminoBusqueda))
            {
                consulta = consulta.Where(c=>c.Nombre.Contains(terminoBusqueda)||
                c.Apellido.Contains(terminoBusqueda) ||
                c.CodigoQR.Contains(terminoBusqueda));


            }

          
            return await consulta.OrderByDescending(c => c.FechaRegistro).ToListAsync();
        }

        public async Task<Cliente?> ObtenerClientePorQRAsync(string codigoQR)
        {   
            return await _context.Clientes.FirstOrDefaultAsync(c => c.CodigoQR == codigoQR);
        }

        public async Task<bool> RegistrarAsistenciaAsync(int clienteId, bool accesoPermitido)
        {
            var asistencia = new Asistencia
            {
                ClienteId = clienteId,
                FechaHoraEntrada = DateTime.Now,
                AccesoPermitido = accesoPermitido
            };

            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarClienteAsync(Cliente cliente)
        {
            var clienteTrackeado = await _context.Clientes.FindAsync(cliente.Id);

            if (clienteTrackeado == null) { return false; }

            _context.Entry(clienteTrackeado).CurrentValues.SetValues(cliente);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}