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
    public class RentasController : Controller
    {
        private readonly RentCarDbContext _context;

        public RentasController(RentCarDbContext context)
        {
            _context = context;
        }

        // GET: Rentas
        public async Task<IActionResult> Index(string searchString)
        {
            var rentas = _context.Rentas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.Inspeccion)
                .Include(r => r.Vehiculo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                rentas = rentas.Where(r =>
                    r.Cliente.Nombre.Contains(searchString) ||
                    r.Empleado.Nombre.Contains(searchString) ||
                    r.Vehiculo.Descripcion.Contains(searchString));
            }

            return View(await rentas.ToListAsync());
        }

        // GET: Rentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var renta = await _context.Rentas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.Inspeccion)
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (renta == null)
            {
                return NotFound();
            }

            return View(renta);
        }

        // GET: Rentas/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["InspeccionId"] = new SelectList(_context.Inspecciones, "Id", "Fecha");
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion");

            return View();
        }

        // POST: Rentas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,VehiculoId,ClienteId,FechaRenta,FechaDevolucion,MontoPorDia,CantidadDias,Comentario,Estado,InspeccionId")] Renta renta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(renta);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Renta registrada exitosamente!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", renta.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre", renta.EmpleadoId);

            // Modificado para incluir el vehículo en la descripción
            ViewData["InspeccionId"] = new SelectList(
                _context.Inspecciones
                    .Include(i => i.Vehiculo)  // Incluir el vehículo relacionado
                    .Select(i => new {
                        i.Id,
                        Descripcion = $"Inspección - {i.Vehiculo.Descripcion} ({i.Fecha.ToString("dd/MM/yyyy")})"
                    }),
                "Id",
                "Descripcion",
                renta.InspeccionId
            );

            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion", renta.VehiculoId);

            return View(renta);
        }

        // GET: Rentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var renta = await _context.Rentas.FindAsync(id);
            if (renta == null)
            {
                return NotFound();
            }

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", renta.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre", renta.EmpleadoId);
            ViewData["InspeccionId"] = new SelectList(_context.Inspecciones, "Id", "Fecha", renta.InspeccionId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion", renta.VehiculoId);

            return View(renta);
        }

        // POST: Rentas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadoId,VehiculoId,ClienteId,FechaRenta,FechaDevolucion,MontoPorDia,CantidadDias,Comentario,Estado,InspeccionId")] Renta renta)
        {
            if (id != renta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(renta);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Renta actualizada exitosamente!"; 

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentaExists(renta.Id))
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

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", renta.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Nombre", renta.EmpleadoId);
            ViewData["InspeccionId"] = new SelectList(_context.Inspecciones, "Id", "Fecha", renta.InspeccionId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "Id", "Descripcion", renta.VehiculoId);

            return View(renta);
        }

        // GET: Rentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var renta = await _context.Rentas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.Inspeccion)
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (renta == null)
            {
                return NotFound();
            }

            return View(renta);
        }

        // POST: Rentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var renta = await _context.Rentas.FindAsync(id);
            if (renta != null)
            {
                _context.Rentas.Remove(renta);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Renta eliminada exitosamente!"; 
            return RedirectToAction(nameof(Index));
        }

        private bool RentaExists(int id)
        {
            return _context.Rentas.Any(e => e.Id == id);
        }
    }
}