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
    public class TipoVehiculosController : Controller
    {
        private readonly RentCarDbContext _context;

        public TipoVehiculosController(RentCarDbContext context)
        {
            _context = context;
        }

        // GET: TipoVehiculos
        public async Task<IActionResult> Index(string searchString)
        {
            var tiposVehiculos = from t in _context.TiposVehiculos
                                 select t;

            if (!string.IsNullOrEmpty(searchString))
            {
                tiposVehiculos = tiposVehiculos.Where(t => t.Descripcion.Contains(searchString));
            }

            return View(await tiposVehiculos.ToListAsync());
        }


        // GET: TipoVehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoVehiculo = await _context.TiposVehiculos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoVehiculo == null)
            {
                return NotFound();
            }

            return View(tipoVehiculo);
        }

        // GET: TipoVehiculos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoVehiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,Estado")] TipoVehiculo tipoVehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoVehiculo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Tipo de Vehículo registrado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            return View(tipoVehiculo);
        }

        // GET: TipoVehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoVehiculo = await _context.TiposVehiculos.FindAsync(id);
            if (tipoVehiculo == null)
            {
                return NotFound();
            }
            return View(tipoVehiculo);
        }

        // POST: TipoVehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,Estado")] TipoVehiculo tipoVehiculo)
        {
            if (id != tipoVehiculo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoVehiculo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Tipo de Vehículo actualizado exitosamente!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoVehiculoExists(tipoVehiculo.Id))
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
            return View(tipoVehiculo);
        }

        // GET: TipoVehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoVehiculo = await _context.TiposVehiculos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoVehiculo == null)
            {
                return NotFound();
            }

            return View(tipoVehiculo);
        }

        // POST: TipoVehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoVehiculo = await _context.TiposVehiculos.FindAsync(id);
            if (tipoVehiculo != null)
            {
                _context.TiposVehiculos.Remove(tipoVehiculo);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Tipo de Vehículo eliminado exitosamente!";
            return RedirectToAction(nameof(Index));
        }

        private bool TipoVehiculoExists(int id)
        {
            return _context.TiposVehiculos.Any(e => e.Id == id);
        }
    }
}
