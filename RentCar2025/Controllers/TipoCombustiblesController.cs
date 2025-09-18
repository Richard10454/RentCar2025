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
    public class TipoCombustiblesController : Controller
    {
        private readonly RentCarDbContext _context;

        public TipoCombustiblesController(RentCarDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: TipoCombustibles
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;

            var tipos = from t in _context.TiposCombustible
                        select t;

            if (!string.IsNullOrEmpty(searchString))
            {
                tipos = tipos.Where(t => t.Descripcion.Contains(searchString));
            }

            int totalTipos = await tipos.CountAsync();
            int totalPages = (int)Math.Ceiling(totalTipos / (double)pageSize);

            var tiposPaginados = await tipos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(tiposPaginados);
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

            var tieneVehiculos = await _context.Vehiculos.AnyAsync(v => v.TipoCombustibleId == id);
            if (tieneVehiculos)
            {
                TempData["ErrorMessage"] = "¡No se puede eliminar este tipo de combustible porque tiene vehículos asociados!";
                return RedirectToAction(nameof(Index));
            }

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

        // GET: TipoCombustibles/GeneratePdfReport
        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            IQueryable<TipoCombustible> tiposCombustible = _context.TiposCombustible;

            if (!string.IsNullOrEmpty(searchString))
            {
                tiposCombustible = tiposCombustible.Where(tc => tc.Descripcion.Contains(searchString));
            }

            var tipoCombustibleList = await tiposCombustible.OrderBy(tc => tc.Descripcion).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Reporte de Tipos de Combustible - RentCar")
                        .SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                            column.Item().Text($"Total de Tipos de Combustible: {tipoCombustibleList.Count}").FontSize(8).AlignRight();

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

                                foreach (var tipo in tipoCombustibleList)
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
                return File(pdfBytes, "application/pdf", $"ReporteTiposCombustible_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=ReporteTiposCombustible.pdf");
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}