namespace RentCar.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Cedula { get; set; }
        public string? NoTarjetaCR { get; set; }
        public decimal LimiteCredito { get; set; }
        public string? TipoPersona { get; set; } // "Física" o "Jurídica"
        public bool Estado { get; set; }
    }
}
