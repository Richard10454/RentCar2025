namespace RentCar.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Cedula { get; set; }
        public string? TandaLabor { get; set; } // Matutina, Vespertina, Nocturna
        public decimal PorcientoComision { get; set; }
        public DateTime FechaIngreso { get; set; }
        public bool Estado { get; set; } // Activo/Inactivo
    }
}
