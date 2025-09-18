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
    public class VehiculosController : Controller
    {
        private readonly RentCarDbContext _context;

        public VehiculosController(RentCarDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;

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
                    v.Modelo.Descripcion.Contains(searchString) ||
                    v.NoPlaca.Contains(searchString) ||
                    v.Descripcion.Contains(searchString));
            }

            int totalVehiculos = await vehiculos.CountAsync();
            int totalPages = (int)Math.Ceiling(totalVehiculos / (double)pageSize);

            var vehiculosPaginados = await vehiculos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(vehiculosPaginados);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,NoChasis,NoMotor,NoPlaca,TipoVehiculoId,MarcaId,ModeloId,TipoCombustibleId,Estado")] Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Vehículo registrado exitosamente!";
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

            var isRented = await _context.Rentas.AnyAsync(r => r.VehiculoId == id && r.Estado);
            if (isRented)
            {
                TempData["ErrorMessage"] = "No se puede editar este vehículo porque tiene una renta activa.";
                return RedirectToAction(nameof(Index));
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,NoChasis,NoMotor,NoPlaca,TipoVehiculoId,MarcaId,ModeloId,TipoCombustibleId,Estado")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }

            var isRented = await _context.Rentas.AnyAsync(r => r.VehiculoId == id && r.Estado);

            if (isRented)
            {
                TempData["ErrorMessage"] = "No se puede editar este vehículo porque tiene una renta activa.";
                return RedirectToAction(nameof(Index));
            }

            var originalVehiculo = await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (originalVehiculo != null && originalVehiculo.Estado == false && vehiculo.Estado == true)
            {
                bool hasActiveRental = await _context.Rentas
                    .AnyAsync(r => r.VehiculoId == vehiculo.Id && r.Estado);

                if (hasActiveRental)
                {
                    ModelState.AddModelError("Estado", "Este vehículo no puede marcarse como disponible porque tiene una renta activa.");
                    TempData["ErrorMessage"] = "Este vehículo no puede marcarse como disponible porque tiene una renta activa.";
                    return RedirectToAction(nameof(Edit), new { id = vehiculo.Id });
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Vehículo actualizado exitosamente!";
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

            var isRented = await _context.Rentas.AnyAsync(r => r.VehiculoId == id && r.Estado);
            if (isRented)
            {
                TempData["ErrorMessage"] = "No se puede eliminar este vehículo porque tiene una renta activa.";
                return RedirectToAction(nameof(Index));
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
            var isRented = await _context.Rentas.AnyAsync(r => r.VehiculoId == id && r.Estado);

            if (isRented)
            {
                TempData["ErrorMessage"] = "No se puede eliminar este vehículo porque tiene una renta activa.";
                return RedirectToAction(nameof(Index));
            }
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Vehículo eliminado exitosamente!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult GetModelosByMarca(int marcaId)
        {
            var modelos = _context.Modelos
                .Where(m => m.MarcaId == marcaId)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Descripcion
                })
                .ToList();

            return Json(modelos);
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            IQueryable<Vehiculo> vehiculos = _context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Include(v => v.TipoCombustible)
                .Include(v => v.TipoVehiculo);

            if (!string.IsNullOrEmpty(searchString))
            {
                vehiculos = vehiculos.Where(v =>
                    v.Marca.Descripcion.Contains(searchString) ||
                    v.Modelo.Descripcion.Contains(searchString) ||
                    v.NoPlaca.Contains(searchString) ||
                    v.Descripcion.Contains(searchString));
            }

            var vehiculoList = await vehiculos.OrderBy(v => v.Descripcion).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Reporte de Vehículos - RentCar")
                        .SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                            column.Item().Text($"Total de Vehículos: {vehiculoList.Count}").FontSize(8).AlignRight();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Descripción
                                    columns.RelativeColumn(1); // No. Placa
                                    columns.RelativeColumn(1); // Marca
                                    columns.RelativeColumn(1); // Modelo
                                    columns.RelativeColumn(1); // Tipo
                                    columns.RelativeColumn(1); // Combustible
                                    columns.RelativeColumn(1); // Estado
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Descripción").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("No. Placa").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Marca").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Modelo").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Tipo").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Combustible").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Estado").SemiBold().FontSize(10);
                                });

                                foreach (var vehiculo in vehiculoList)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.NoPlaca);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.Marca.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.Modelo.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.TipoVehiculo.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.TipoCombustible.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(vehiculo.Estado ? "Disponible" : "Rentado").FontColor(vehiculo.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
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
                return File(pdfBytes, "application/pdf", $"ReporteVehiculos_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=ReporteVehiculos.pdf");
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}