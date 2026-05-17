using Microsoft.EntityFrameworkCore;
using Gymio.Models;

namespace Gymio.Data
{
    public class GymioDbContext  : DbContext
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
       
        public DbSet<AsignacionEntrenador> AsignacionesEntrenadores { get; set; }
        public DbSet<CategoriaEgreso> CategoriasEgresos { get; set; }
        public DbSet<Egreso> Egresos { get; set; }
        public DbSet<PagoPlanilla> PagosPlanilla { get; set; }
        public DbSet<CompraInventario> ComprasInventario { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Maquina> Maquinas { get; set; }
        public DbSet<MantenimientoMaquina> MantenimientosMaquinas { get; set; }
        public DbSet<TurnoCaja> TurnosCaja { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
    
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();//claves unica  UNIQUE para estos parametros
            modelBuilder.Entity<Cliente>().HasIndex(c => c.Email).IsUnique();
            modelBuilder.Entity<Cliente>().HasIndex(c => c.CodigoQR).IsUnique();



            modelBuilder.Entity<CategoriaEgreso>().HasData(
                new CategoriaEgreso { Id = 1, Nombre = "Planilla" },
                new CategoriaEgreso { Id = 2, Nombre = "Servicios Básicos" },
                new CategoriaEgreso { Id = 3, Nombre = "Inventario" },
                new CategoriaEgreso { Id = 4, Nombre = "Mantenimiento" },
                new CategoriaEgreso { Id = 5, Nombre = "Otros" }
            );
            //esto es para que el enum se guarde como texto 
            modelBuilder.Entity<Usuario>()
    .Property(u => u.FrecuenciaPago)
    .HasConversion<string>();

            //lo que hace esto es que si el ususario tiene egresos registrados
            //no permita eliminarlo, para evitar asi la famosa eliminacion en cascada porque egresos tiene una relacion con usuario
            modelBuilder.Entity<Egreso>()
                .HasOne(e=>e.UsuarioRegistra)
                .WithMany()
                .HasForeignKey(e=>e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoPlanilla>()
        .HasOne(p => p.Entrenador)
        .WithMany()
        .HasForeignKey(p => p.EntrenadorId)
        .OnDelete(DeleteBehavior.Restrict);

           //qui tambien como egresoid esta relacionado con planilla se hace lo mismo
            modelBuilder.Entity<PagoPlanilla>()
                .HasOne(p => p.EgresoGenerado)
                .WithMany()
                .HasForeignKey(p => p.EgresoId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
