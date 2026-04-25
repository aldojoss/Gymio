using Microsoft.EntityFrameworkCore;
using Gymio.Models;

namespace Gymio.Data
{
    public class GymioDbContext : DbContext
    {
        public GymioDbContext(DbContextOptions<GymioDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Plan> Planes { get; set; }
        public DbSet<SuscripcionCliente> SuscripcionesClientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<Egreso> Egresos { get; set; }
        public DbSet<AsignacionEntrenador> AsignacionesEntrenadores { get; set; }

    }
}
