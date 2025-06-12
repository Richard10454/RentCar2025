namespace RentCar.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
        public string? NoChasis { get; set; }
        public string? NoMotor { get; set; }
        public string? NoPlaca { get; set; }
        public int TipoVehiculoId { get; set; }
        public TipoVehiculo? TipoVehiculo { get; set; }
        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }
        public int ModeloId { get; set; }
        public Modelo? Modelo { get; set; }
        public int TipoCombustibleId { get; set; }
        public TipoCombustible? TipoCombustible { get; set; }
        public bool Estado { get; set; } // Disponible/Rentado/Mantenimiento
    }
}
