using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class Renta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El empleado es obligatorio.")]
        public int EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio.")]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

     

        [Required(ErrorMessage = "La fecha de renta es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaRenta { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaDevolucion { get; set; }

        [Range(1, 999999, ErrorMessage = "El monto por día debe ser mayor que 0.")]
        public decimal MontoPorDia { get; set; }

        [Range(1, 365, ErrorMessage = "La cantidad de días debe ser al menos 1 y máximo 365.")]
        public int CantidadDias { get; set; }

        [StringLength(250, ErrorMessage = "El comentario no puede superar los 250 caracteres.")]
        public string? Comentario { get; set; }

        public bool Estado { get; set; } // true = Activa, false = Devuelta
    }
}
