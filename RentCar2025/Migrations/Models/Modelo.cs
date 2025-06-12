namespace RentCar.Models
{
    public class Modelo
    {
        public int Id { get; set; }
        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}
