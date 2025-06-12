namespace RentCar.Models
{
    public class Renta
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime? FechaRenta { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public decimal MontoPorDia { get; set; }
        public int CantidadDias { get; set; }
        public string? Comentario { get; set; }
        public bool Estado { get; set; } // Activa/Devuelta
        public int? InspeccionId { get; set; }
        public Inspeccion? Inspeccion { get; set; }
    }
}
