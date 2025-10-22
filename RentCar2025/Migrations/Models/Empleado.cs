using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class Empleado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no puede superar los 20 caracteres.")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tanda laboral es obligatoria.")]
        [StringLength(20, ErrorMessage = "La tanda laboral no puede superar los 20 caracteres.")]
        public string TandaLabor { get; set; } = string.Empty; // Matutina, Vespertina, Nocturna

        [Range(0, 100, ErrorMessage = "El porcentaje de comisión debe estar entre 0 y 100.")]
        public decimal PorcientoComision { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; }

        public bool Estado { get; set; } // true = Activo, false = Inactivo
    }
}
