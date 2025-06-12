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
    public class InspeccionesController : Controller
    {
        private readonly RentCarDbContext _context;

        public InspeccionesController(RentCarDbContext context)
        {
            _context = context;
        }

        // GET: Inspecciones
        // GET: Inspecciones
        public async Task<IActionResult> Index(string searchString)
        {
            var inspecciones = _context.Inspecciones
                .Include(i => i.Cliente)
                .Include(i => i.Empleado)
                .Include(i => i.Vehiculo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                inspecciones = inspecciones.Where(i =>
                    i.Cliente.Nombre.Contains(searchString) ||
                    i.Empleado.Nombre.Contains(searchString));
            }

            return View(await inspecciones.ToListAsync());
        }


        // GET: Inspecciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspeccion = await _context.Inspecciones
                .Include(i => i.Cliente)
                .Include(i => i.Empleado)
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspeccion == null)
            {
                return NotFound();
            }

            return View(inspeccion);
        }

        // GET: Inspecciones/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion");
            return View();
        }

        // POST: Inspecciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehiculoId,ClienteId,TieneRalladuras,CantidadCombustible,TieneGomaRespuesta,TieneGato,TieneRoturasCristal,EstadoGomas,Fecha,EmpleadoId,Estado")] Inspeccion inspeccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inspeccion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Inspección registrada exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", inspeccion.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre", inspeccion.EmpleadoId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion", inspeccion.VehiculoId);
            return View(inspeccion);
        }

        // GET: Inspecciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspeccion = await _context.Inspecciones.FindAsync(id);
            if (inspeccion == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", inspeccion.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre", inspeccion.EmpleadoId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion", inspeccion.VehiculoId);
            return View(inspeccion);
        }

        // POST: Inspecciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehiculoId,ClienteId,TieneRalladuras,CantidadCombustible,TieneGomaRespuesta,TieneGato,TieneRoturasCristal,EstadoGomas,Fecha,EmpleadoId,Estado")] Inspeccion inspeccion)
        {
            if (id != inspeccion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspeccion);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Inspección actualizada exitosamente!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InspeccionExists(inspeccion.Id))
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", inspeccion.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre", inspeccion.EmpleadoId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion", inspeccion.VehiculoId);
            return View(inspeccion);
        }

        // GET: Inspecciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspeccion = await _context.Inspecciones
                .Include(i => i.Cliente)
                .Include(i => i.Empleado)
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspeccion == null)
            {
                return NotFound();
            }

            return View(inspeccion);
        }

        // POST: Inspecciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspeccion = await _context.Inspecciones.FindAsync(id);
            if (inspeccion != null)
            {
                _context.Inspecciones.Remove(inspeccion);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Inspección eliminada exitosamente!";
            return RedirectToAction(nameof(Index));
        }

        private bool InspeccionExists(int id)
        {
            return _context.Inspecciones.Any(e => e.Id == id);
        }
    }
}
