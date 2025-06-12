using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentCar2025.Models; 
using RentCar2025.Migrations.Models.ViewModels; 
using Microsoft.EntityFrameworkCore;

namespace RentCar2025.Controllers
{
    public class HomeController : Controller
    {
        private readonly RentCarDbContext _context; 

        public HomeController(RentCarDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeDashboardViewModel
            {
                TotalVehiculos = await _context.Vehiculos.CountAsync(),
                TotalMarcas = await _context.Marcas.CountAsync(),
                TotalModelos = await _context.Modelos.CountAsync(),
                TotalTiposVehiculo = await _context.TiposVehiculos.CountAsync(),
                TotalClientes = await _context.Clientes.CountAsync(),
                TotalRentas = await _context.Rentas.CountAsync(),
                TotalEmpleados = await _context.Empleados.CountAsync(),
                TotalInspecciones = await _context.Inspecciones.CountAsync(),
                TotalTiposCombustible = await _context.TiposCombustible.CountAsync(),


                UltimosVehiculos = await _context.Vehiculos
                    .Include(v => v.Marca)
                    .Include(v => v.Modelo)
                    .OrderByDescending(v => v.Id) 
                    .Take(5)
                    .ToListAsync(),

              
                UltimosModelos = await _context.Modelos
                    .Include(m => m.Marca)
                    .OrderByDescending(m => m.Id) 
                    .Take(5)
                    .ToListAsync(),

           
                UltimasMarcas = await _context.Marcas
                    .OrderByDescending(ma => ma.Id) 
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Nosotros()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}