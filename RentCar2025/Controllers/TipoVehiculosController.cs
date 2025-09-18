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
    public class TipoVehiculosController : Controller
    {
        private readonly RentCarDbContext _context;

        public TipoVehiculosController(RentCarDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: TipoVehiculos
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;

            var tiposVehiculos = from t in _context.TiposVehiculos
                                 select t;

            if (!string.IsNullOrEmpty(searchString))
            {
                tiposVehiculos = tiposVehiculos.Where(t => t.Descripcion.Contains(searchString));
            }

            int totalTipos = await tiposVehiculos.CountAsync();
            int totalPages = (int)Math.Ceiling(totalTipos / (double)pageSize);

            var tiposVehiculosPaginados = await tiposVehiculos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(tiposVehiculosPaginados);
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
            var tieneVehiculos = await _context.Vehiculos.AnyAsync(v => v.TipoVehiculoId == id);
            if (tieneVehiculos)
            {
                TempData["ErrorMessage"] = "¡No se puede eliminar este tipo de vehículo porque tiene vehículos asociados!";
                return RedirectToAction(nameof(Index));
            }
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

        // GET: TipoVehiculos/GeneratePdfReport
        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            IQueryable<TipoVehiculo> tiposVehiculos = _context.TiposVehiculos;

            if (!string.IsNullOrEmpty(searchString))
            {
                tiposVehiculos = tiposVehiculos.Where(tv => tv.Descripcion.Contains(searchString));
            }

            var tipoVehiculoList = await tiposVehiculos.OrderBy(tv => tv.Descripcion).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Reporte de Tipos de Vehículo - RentCar")
                        .SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                            column.Item().Text($"Total de Tipos de Vehículo: {tipoVehiculoList.Count}").FontSize(8).AlignRight();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Descripción
                                    columns.RelativeColumn(); // Estado
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Descripción").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Estado").SemiBold().FontSize(10);
                                });

                                foreach (var tipo in tipoVehiculoList)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tipo.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tipo.Estado ? "Activo" : "Inactivo").FontColor(tipo.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
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
                return File(pdfBytes, "application/pdf", $"ReporteTiposVehiculo_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=ReporteTiposVehiculo.pdf");
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}