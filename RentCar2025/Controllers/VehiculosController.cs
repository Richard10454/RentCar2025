using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentCar.Models;

namespace RentCar2025.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly RentCarDbContext _context;

        public VehiculosController(RentCarDbContext context)
        {
            _context = context;
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index(string searchString)
        {
            var vehiculos = _context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Include(v => v.TipoCombustible)
                .Include(v => v.TipoVehiculo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                vehiculos = vehiculos.Where(v =>
                    v.Marca.Descripcion.Contains(searchString) ||
                    v.Modelo.Descripcion.Contains(searchString));
            }

            return View(await vehiculos.ToListAsync());
        }


        // GET: Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Include(v => v.TipoCombustible)
                .Include(v => v.TipoVehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Descripcion");
            ViewData["ModeloId"] = new SelectList(_context.Modelos, "Id", "Descripcion");
            ViewData["TipoCombustibleId"] = new SelectList(_context.TiposCombustible, "Id", "Descripcion");
            ViewData["TipoVehiculoId"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion");
            return View();
        }

        // POST: Vehiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,NoChasis,NoMotor,NoPlaca,TipoVehiculoId,MarcaId,ModeloId,TipoCombustibleId,Estado")] Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Vehículo registrado exitosamente!"; // Added success message
                return RedirectToAction(nameof(Index));
            }
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Descripcion", vehiculo.MarcaId);
            ViewData["ModeloId"] = new SelectList(_context.Modelos, "Id", "Descripcion", vehiculo.ModeloId);
            ViewData["TipoCombustibleId"] = new SelectList(_context.TiposCombustible, "Id", "Descripcion", vehiculo.TipoCombustibleId);
            ViewData["TipoVehiculoId"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.TipoVehiculoId);
            return View(vehiculo);
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Descripcion", vehiculo.MarcaId);
            ViewData["ModeloId"] = new SelectList(_context.Modelos, "Id", "Descripcion", vehiculo.ModeloId);
            ViewData["TipoCombustibleId"] = new SelectList(_context.TiposCombustible, "Id", "Descripcion", vehiculo.TipoCombustibleId);
            ViewData["TipoVehiculoId"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.TipoVehiculoId);
            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,NoChasis,NoMotor,NoPlaca,TipoVehiculoId,MarcaId,ModeloId,TipoCombustibleId,Estado")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Vehículo actualizado exitosamente!"; // Added success message

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "Id", "Descripcion", vehiculo.MarcaId);
            ViewData["ModeloId"] = new SelectList(_context.Modelos, "Id", "Descripcion", vehiculo.ModeloId);
            ViewData["TipoCombustibleId"] = new SelectList(_context.TiposCombustible, "Id", "Descripcion", vehiculo.TipoCombustibleId);
            ViewData["TipoVehiculoId"] = new SelectList(_context.TiposVehiculos, "Id", "Descripcion", vehiculo.TipoVehiculoId);
            return View(vehiculo);
        }

        // GET: Vehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Include(v => v.TipoCombustible)
                .Include(v => v.TipoVehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Vehículo eliminado exitosamente!"; // Added success message
            return RedirectToAction(nameof(Index));
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }
    }
}
