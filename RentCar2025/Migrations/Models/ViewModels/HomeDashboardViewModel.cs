using RentCar.Models;

namespace RentCar2025.Migrations.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public int TotalVehiculos { get; set; }
        public int TotalMarcas { get; set; }
        public int TotalModelos { get; set; }
        public int TotalTiposVehiculo { get; set; }
        public int TotalClientes { get; set; }
        public int TotalRentas { get; set; }
        public int TotalEmpleados { get; set; }
        public int TotalInspecciones { get; set; }
        public int TotalTiposCombustible { get; set; }
        public List<Vehiculo>? UltimosVehiculos { get; set; }
        public List<Marca>? UltimasMarcas { get; set; }
        public List<Modelo>? UltimosModelos { get; set; }



    }
}
