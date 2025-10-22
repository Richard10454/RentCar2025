using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no puede superar los 20 caracteres.")]
        public required string Cedula { get; set; }

        [Required(ErrorMessage = "El número de tarjeta de crédito es obligatorio.")]
        [StringLength(20, ErrorMessage = "El número de tarjeta no puede superar los 20 caracteres.")]
        public required string NoTarjetaCR { get; set; }

        [Required(ErrorMessage = "El límite de crédito es obligatorio.")]
        [Range(0, 9999999, ErrorMessage = "El límite de crédito debe ser mayor o igual a 0.")]
        public decimal LimiteCredito { get; set; }

        [Required(ErrorMessage = "El tipo de persona es obligatorio (Física o Jurídica).")]
        [StringLength(20, ErrorMessage = "El tipo de persona no puede superar los 20 caracteres.")]
        public required string TipoPersona { get; set; } // "Física" o "Jurídica"

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Estado { get; set; }
    }
}
