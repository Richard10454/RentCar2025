using Microsoft.EntityFrameworkCore;
using RentCar.Models;
using System.Collections.Generic;

public class RentCarDbContext : DbContext
{
    public RentCarDbContext(DbContextOptions<RentCarDbContext> options) : base(options)
    {
    }

    // Define tus DbSets (tablas) aquí
    public DbSet<TipoVehiculo> TiposVehiculos { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Modelo> Modelos { get; set; }
    public DbSet<TipoCombustible> TiposCombustible { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Inspeccion> Inspecciones { get; set; }
    public DbSet<Renta> Rentas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar relaciones y restricciones
        modelBuilder.Entity<Vehiculo>()
            .HasOne(v => v.Marca)
            .WithMany()
            .HasForeignKey(v => v.MarcaId)
            .OnDelete(DeleteBehavior.Restrict); // Cambiado de CASCADE a RESTRICT

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

        // Configuración para Modelo
        modelBuilder.Entity<Modelo>()
            .HasOne(m => m.Marca)
            .WithMany()
            .HasForeignKey(m => m.MarcaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}