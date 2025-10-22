using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class Modelo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public bool Estado { get; set; } 
    }
}
