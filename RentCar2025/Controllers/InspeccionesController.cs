using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentCar.Models;
using System.IO;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RentCar2025.Controllers
{
    public class InspeccionesController : Controller
    {
        private readonly RentCarDbContext _context;

        public InspeccionesController(RentCarDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: Inspecciones
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;

            var inspecciones = _context.Inspecciones
                .Include(i => i.Cliente)
                .Include(i => i.Empleado)
                .Include(i => i.Vehiculo)
                .ThenInclude(v => v.Modelo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                inspecciones = inspecciones.Where(i =>
                    i.Cliente.Nombre.Contains(searchString) ||
                    i.Empleado.Nombre.Contains(searchString) ||
                    i.Vehiculo.NoPlaca.Contains(searchString) ||
                    i.Vehiculo.Modelo.Descripcion.Contains(searchString));
            }

            int totalInspecciones = await inspecciones.CountAsync();
            int totalPages = (int)Math.Ceiling(totalInspecciones / (double)pageSize);

            var inspeccionesPaginadas = await inspecciones
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(inspeccionesPaginadas);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehiculoId,ClienteId,TieneRalladuras,CantidadCombustible,TieneGomaRespuesta,TieneGato,TieneRoturasCristal,EstadoGomas,EstadoGomas2,EstadoGomas3,EstadoGomas4,Fecha,EmpleadoId,Estado")] Inspeccion inspeccion)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehiculoId,ClienteId,TieneRalladuras,CantidadCombustible,TieneGomaRespuesta,TieneGato,TieneRoturasCristal,EstadoGomas,EstadoGomas2,EstadoGomas3,EstadoGomas4,Fecha,EmpleadoId,Estado")] Inspeccion inspeccion)
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

        /// <param name="searchString">
        /// <param name="download">
        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            var inspections = _context.Inspecciones
                .Include(i => i.Cliente)
                .Include(i => i.Empleado)
                .Include(i => i.Vehiculo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                inspections = inspections.Where(i =>
                    i.Cliente.Nombre.Contains(searchString) ||
                    i.Empleado.Nombre.Contains(searchString) ||
                    i.Vehiculo.NoPlaca.Contains(searchString));
            }

            var inspectionList = await inspections.OrderByDescending(i => i.Fecha).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Reporte de Inspecciones - RentCar")
                        .SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                            column.Item().Text($"Total de Inspecciones: {inspectionList.Count}").FontSize(8).AlignRight();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // Fecha
                                    columns.RelativeColumn(1); // Vehículo
                                    columns.RelativeColumn(1); // Cliente
                                    columns.RelativeColumn(1); // Empleado
                                    columns.RelativeColumn(1); // Combustible
                                    columns.RelativeColumn(1); // Ralladuras
                                    columns.RelativeColumn(1); // Estado
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Fecha").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Vehículo").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Cliente").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Empleado").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Combustible").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Ralladuras").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Estado").SemiBold().FontSize(10);
                                });

                                foreach (var inspeccion in inspectionList)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.Fecha.ToShortDateString());
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.Vehiculo.NoPlaca);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.Cliente.Nombre);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.Empleado.Nombre);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.CantidadCombustible);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.TieneRalladuras ? "Sí" : "No");
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(inspeccion.Estado ? "Activa" : "Inactiva").FontColor(inspeccion.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                }
                            });
                        });

                    page.Footer()
                        .AlignRight()
                        .Text(x =>
                        {
                            x.Span("Página ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" de ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();

            if (download)
            {
                return File(pdfBytes, "application/pdf", $"ReporteInspecciones_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=ReporteInspecciones.pdf");
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}