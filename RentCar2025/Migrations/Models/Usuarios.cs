using System.ComponentModel.DataAnnotations;

namespace RentCar2025.Migrations.Models
{
    public class Usuarios
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public string? correo { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? Contrasena { get; set; }

        //public string? Rol { get; set; } // Administrador, Empleado, Cliente
        public string? Telefono { get; set; } 
        public DateTime? FechaNacimiento { get; set; } 

        public string? Genero { get; set; } 

        public string? Nacionalidad { get; set; } 

        public string? EstadoCivil { get; set; } 

      

    }
}
