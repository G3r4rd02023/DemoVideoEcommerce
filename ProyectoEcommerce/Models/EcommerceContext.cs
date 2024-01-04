using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Models.Entidades;

namespace ProyectoEcommerce.Models
{
    public class EcommerceContext : IdentityDbContext<Usuario>
    {
        public EcommerceContext(DbContextOptions<EcommerceContext> options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<VentaTemporal> VentasTemporales { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<Venta> Ventas { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Categoria>().HasIndex(c => c.Nombre).IsUnique();
        }
    }
}
