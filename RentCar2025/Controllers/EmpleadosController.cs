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
    // -------------------------------------------------------------------
    // ESTA CLASE 'Utilidades' FUE ELIMINADA DE AQUÍ
    // para resolver el conflicto. ASUMIMOS que existe en otro archivo
    // dentro del mismo namespace (o se agregó un 'using').
    // -------------------------------------------------------------------

    public class EmpleadosController : Controller
    {
        private readonly RentCarDbContext _context;

        public EmpleadosController(RentCarDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;

            var empleados = from c in _context.Empleados
                            select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(c => c.Nombre.Contains(searchString) || c.Cedula.Contains(searchString));
            }

            int totalEmpleados = await empleados.CountAsync();
            int totalPages = (int)Math.Ceiling(totalEmpleados / (double)pageSize);

            var empleadosPaginados = await empleados
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(empleadosPaginados);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Cedula,TandaLabor,PorcientoComision,FechaIngreso,Estado")] Empleado empleado)
        {
            // La llamada a Utilidades.ValidaCedula ya no es ambigua
            if (!Utilidades.ValidaCedula(empleado.Cedula))
            {
                ModelState.AddModelError("Cedula", "El número de Cédula");
            }

            if (await _context.Empleados.AnyAsync(e => e.Cedula == empleado.Cedula))
            {
                ModelState.AddModelError("Cedula", "Ya existe un empleado registrado con esta Cédula.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Empleado registrado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Cedula,TandaLabor,PorcientoComision,FechaIngreso,Estado")] Empleado empleado)
        {
            if (id != empleado.Id)
            {
                return NotFound();
            }

            // La llamada a Utilidades.ValidaCedula ya no es ambigua
            if (!Utilidades.ValidaCedula(empleado.Cedula))
            {
                ModelState.AddModelError("Cedula", "El número de Cédula no es válido");
            }

            if (await _context.Empleados.AnyAsync(e => e.Cedula == empleado.Cedula && e.Id != empleado.Id))
            {
                ModelState.AddModelError("Cedula", "Ya existe otro empleado registrado con esta Cédula.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Empleado actualizado exitosamente!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id))
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
            return View(empleado);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            var tieneRentas = await _context.Rentas.AnyAsync(r => r.EmpleadoId == id);
            var tieneInspecciones = await _context.Inspecciones.AnyAsync(i => i.EmpleadoId == id);

            if (tieneInspecciones)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el empleado porque tiene inspecciones asociadas.";
                return RedirectToAction(nameof(Index));
            }

            if (tieneRentas)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el empleado porque tiene rentas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Empleado eliminado exitosamente!";
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            IQueryable<Empleado> empleados = _context.Empleados;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(e => e.Nombre.Contains(searchString) ||
                                                 e.Cedula.Contains(searchString));
            }

            var empleadoList = await empleados.OrderBy(e => e.Nombre).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Reporte de Empleados - RentCar")
                        .SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                            column.Item().Text($"Total de Empleados: {empleadoList.Count}").FontSize(8).AlignRight();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Nombre").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Cédula").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Tanda Labor").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Porciento Comision").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Fecha Ingreso").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Estado").SemiBold().FontSize(10);
                                });

                                foreach (var empleado in empleadoList)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(empleado.Nombre);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(empleado.Cedula);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(empleado.TandaLabor);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(empleado.PorcientoComision.ToString("P0"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(empleado.FechaIngreso.ToShortDateString());
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(empleado.Estado ? "Activo" : "Inactivo").FontColor(empleado.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
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
                return File(pdfBytes, "application/pdf", $"ReporteEmpleados_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=ReporteEmpleados.pdf");
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}