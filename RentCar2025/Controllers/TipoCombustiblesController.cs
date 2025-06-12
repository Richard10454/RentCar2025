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
    public class TipoCombustiblesController : Controller
    {
        private readonly RentCarDbContext _context;

        public TipoCombustiblesController(RentCarDbContext context)
        {
            _context = context;
        }

        // GET: TipoCombustibles
        // GET: TipoCombustibles
        public async Task<IActionResult> Index(string searchString)
        {
            var tipos = from t in _context.TiposCombustible
                        select t;

            if (!String.IsNullOrEmpty(searchString))
            {
                tipos = tipos.Where(t => t.Descripcion.Contains(searchString));
            }

            return View(await tipos.ToListAsync());
        }


        // GET: TipoCombustibles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoCombustible = await _context.TiposCombustible
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoCombustible == null)
            {
                return NotFound();
            }

            return View(tipoCombustible);
        }

        // GET: TipoCombustibles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoCombustibles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,Estado")] TipoCombustible tipoCombustible)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoCombustible);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Tipo de Combustible registrado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            return View(tipoCombustible);
        }

        // GET: TipoCombustibles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoCombustible = await _context.TiposCombustible.FindAsync(id);
            if (tipoCombustible == null)
            {
                return NotFound();
            }
            return View(tipoCombustible);
        }

        // POST: TipoCombustibles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,Estado")] TipoCombustible tipoCombustible)
        {
            if (id != tipoCombustible.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoCombustible);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Tipo de Combustible actualizado exitosamente!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoCombustibleExists(tipoCombustible.Id))
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
            return View(tipoCombustible);
        }

        // GET: TipoCombustibles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoCombustible = await _context.TiposCombustible
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoCombustible == null)
            {
                return NotFound();
            }

            return View(tipoCombustible);
        }

        // POST: TipoCombustibles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoCombustible = await _context.TiposCombustible.FindAsync(id);
            if (tipoCombustible != null)
            {
                _context.TiposCombustible.Remove(tipoCombustible);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Tipo de Combustible eliminado exitosamente!";
            return RedirectToAction(nameof(Index));
        }

        private bool TipoCombustibleExists(int id)
        {
            return _context.TiposCombustible.Any(e => e.Id == id);
        }
    }
}
