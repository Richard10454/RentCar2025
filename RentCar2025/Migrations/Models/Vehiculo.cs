using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de chasis es obligatorio.")]
        [StringLength(50, ErrorMessage = "El número de chasis no puede superar los 50 caracteres.")]
        public string NoChasis { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de motor es obligatorio.")]
        [StringLength(50, ErrorMessage = "El número de motor no puede superar los 50 caracteres.")]
        public string NoMotor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de placa es obligatorio.")]
        [StringLength(20, ErrorMessage = "El número de placa no puede superar los 20 caracteres.")]
        public string NoPlaca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio.")]
        public int TipoVehiculoId { get; set; }
        public TipoVehiculo? TipoVehiculo { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        public int ModeloId { get; set; }
        public Modelo? Modelo { get; set; }

        [Required(ErrorMessage = "El tipo de combustible es obligatorio.")]
        public int TipoCombustibleId { get; set; }
        public TipoCombustible? TipoCombustible { get; set; }

        public bool Estado { get; set; } 
    }


}
