using Microsoft.EntityFrameworkCore;
using RentCar.Models;
using RentCar2025.Migrations.Models;
using RentCar2025.Migrations.Models.ViewModels;

public class RentCarDbContext : DbContext
{
    public RentCarDbContext(DbContextOptions<RentCarDbContext> options) : base(options)
    {
    }

    public DbSet<TipoVehiculo> TiposVehiculos { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Modelo> Modelos { get; set; }
    public DbSet<TipoCombustible> TiposCombustible { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Inspeccion> Inspecciones { get; set; }
    public DbSet<Renta> Rentas { get; set; }
    public DbSet<Usuarios> Usuarios { get; set; }
    //public DbSet<ReportViewModel> ReportViewModel { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.Marca)
            .WithMany()
            .HasForeignKey(v => v.MarcaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.Modelo)
            .WithMany()
            .HasForeignKey(v => v.ModeloId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.TipoCombustible)
            .WithMany()
            .HasForeignKey(v => v.TipoCombustibleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.TipoVehiculo)
            .WithMany()
            .HasForeignKey(v => v.TipoVehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Modelo>()
            .HasOne(m => m.Marca)
            .WithMany()
            .HasForeignKey(m => m.MarcaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = "Server=localhost;Database=RentCarDB;User=root;Password=1234;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}
