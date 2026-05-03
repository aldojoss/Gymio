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

            bool existeEnClientes = await _context.Clientes.AnyAsync(c => c.Email == nuevoCliente.Email);
            bool existeEnUsuarios = await _context.Usuarios.AnyAsync(u => u.Email == nuevoCliente.Email);

            if (existeEnClientes || existeEnUsuarios)
            {
                // Aquí podrías lanzar una excepción o retornar false para mostrar error en la UI
                return false;
            }
            // 1el codigo generamos uno unico para evitar errores al 100*100
            // el formato: GYM-AÑO-MES-DIA-5LetrasAleatorias
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
            var fechaHoy = DateTime.Today;


            var yaAsistio = await _context.Asistencias
                .AnyAsync(a => a.ClienteId == clienteId && a.FechaHoraEntrada.Date == fechaHoy);

        
            if (yaAsistio) return false;


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


        public async Task<int> ObtenerIdEntrenadorAsignadoAsync(int clienteId)
        {
            var asignacion = await _context.AsignacionesEntrenadores
                .FirstOrDefaultAsync(a => a.ClienteId == clienteId);


            return asignacion?.EntrenadorId ?? 0;
        }

        public async Task<bool> AsignarEntrenadorAsync(int clienteId, int entrenadorId)
        {
            
            var asignacionExistente = await _context.AsignacionesEntrenadores
                .FirstOrDefaultAsync(a => a.ClienteId == clienteId);

            if (asignacionExistente != null)
            {
              
                asignacionExistente.EntrenadorId = entrenadorId;
                asignacionExistente.FechaAsignacion = DateTime.Now;
                _context.AsignacionesEntrenadores.Update(asignacionExistente);
            }
            else
            {
              
                var nuevaAsignacion = new AsignacionEntrenador
                {
                    ClienteId = clienteId,
                    EntrenadorId = entrenadorId,
                    FechaAsignacion = DateTime.Now
                };
                _context.AsignacionesEntrenadores.Add(nuevaAsignacion);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Usuario>> ObtenerEntrenadoresAsync()
        {
            return await _context.Usuarios.Where(e => e.Rol=="Entrenador").ToListAsync();
        }

        public async Task<List<Cliente>> BuscarClientesParaSuscripcionAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return new List<Cliente>();

            return await _context.Clientes
                .Where(c => c.Nombre.Contains(termino) || c.Apellido.Contains(termino))
                .Take(5) 
                .ToListAsync();
        }

        public async Task<List<Cliente>> ObtenerClientesPorEntrenadorAsync(int entrenadorId)
        {
            
            var asignaciones = await _context.AsignacionesEntrenadores
                .Include(a => a.Cliente) 
                .Where(a => a.EntrenadorId == entrenadorId)
                .ToListAsync();

            
            return asignaciones.Select(a => a.Cliente).ToList();
        }
    }
}