using Microsoft.EntityFrameworkCore;
using AnalisisVentas.Data.Models;

namespace AnalisisVentas.Data.Context
{
    public class DbAnalisisContext : DbContext
    {
        public DbAnalisisContext(DbContextOptions<DbAnalisisContext> options) : base(options) { }

        public DbSet<Customer> Clientes { get; set; }
        public DbSet<Product> Productos { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<TipoFuente> TipoFuente { get; set; }
        public DbSet<FuenteDato> FuenteDatos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Clientes
            modelBuilder.Entity<Customer>().ToTable("Clientes");
            modelBuilder.Entity<Customer>().HasKey(c => c.CustomerID);
            modelBuilder.Entity<Customer>().Property(c => c.CustomerID).HasColumnName("IdCliente").ValueGeneratedNever();
            modelBuilder.Entity<Customer>().Property(c => c.FirstName).HasColumnName("Nombre");
            modelBuilder.Entity<Customer>().Ignore(c => c.LastName); 
            modelBuilder.Entity<Customer>().Property(c => c.Email).HasColumnName("Email");
            modelBuilder.Entity<Customer>().Property(c => c.Phone).HasColumnName("Phone");
            modelBuilder.Entity<Customer>().Property(c => c.City).HasColumnName("City");
            modelBuilder.Entity<Customer>().Property(c => c.Country).HasColumnName("Region");

            // Productos
            modelBuilder.Entity<Product>().ToTable("Productos");
            modelBuilder.Entity<Product>().HasKey(p => p.ProductID);
            modelBuilder.Entity<Product>().Property(p => p.ProductID).HasColumnName("IdProducto").ValueGeneratedNever();
            modelBuilder.Entity<Product>().Property(p => p.ProductName).HasColumnName("Nombre");
            modelBuilder.Entity<Product>().Ignore(p => p.Category); 
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnName("Precio").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.Stock).HasColumnName("Stock");

            // Categoria 
            modelBuilder.Entity<Categoria>().HasKey(c => c.IdCategoria);
            modelBuilder.Entity<Categoria>().Property(c => c.IdCategoria).ValueGeneratedOnAdd();

            // TipoFuente
            modelBuilder.Entity<TipoFuente>().HasKey(t => t.IdTipoFuente);
            modelBuilder.Entity<TipoFuente>().Property(t => t.IdTipoFuente).ValueGeneratedOnAdd();

            // FuenteDatos
            modelBuilder.Entity<FuenteDato>().ToTable("FuenteDatos");
            modelBuilder.Entity<FuenteDato>().HasKey(f => f.IdFuente);
            modelBuilder.Entity<FuenteDato>().Property(f => f.IdFuente).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }
    }
}
