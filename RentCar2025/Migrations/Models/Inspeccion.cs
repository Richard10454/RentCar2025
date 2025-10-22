using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class Inspeccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio.")]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public bool TieneRalladuras { get; set; }

        [Required(ErrorMessage = "La cantidad de combustible es obligatoria.")]
        [StringLength(10, ErrorMessage = "La cantidad de combustible no puede superar los 10 caracteres.")]
        public string CantidadCombustible { get; set; } = string.Empty; // Ej: "1/4", "1/2", "3/4", "Lleno"

        public bool TieneGomaRespuesta { get; set; }
        public bool TieneGato { get; set; }
        public bool TieneRoturasCristal { get; set; }

        // Estado de cada goma
        public bool EstadoGomas { get; set; }
        public bool EstadoGomas2 { get; set; }
        public bool EstadoGomas3 { get; set; }
        public bool EstadoGomas4 { get; set; }

        [Required(ErrorMessage = "La fecha de inspección es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El empleado es obligatorio.")]
        public int EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        public bool Estado { get; set; } // true = Activo, false = Inactivo
    }
}
