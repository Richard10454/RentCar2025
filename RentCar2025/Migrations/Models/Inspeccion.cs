namespace RentCar.Models
{
    public class Inspeccion
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public bool TieneRalladuras { get; set; }
        public string? CantidadCombustible { get; set; } // "1/4", "1/2", "3/4", "lleno"
        public bool TieneGomaRespuesta { get; set; }
        public bool TieneGato { get; set; }
        public bool TieneRoturasCristal { get; set; }
        public string? EstadoGomas { get; set; }  // Estado de las 4 gomas
        public DateTime Fecha { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }
        public bool Estado { get; set; }
    }
}
