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
    public class RentasController : Controller
    {
        private readonly RentCarDbContext _context;

        public RentasController(RentCarDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: Rentas
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;
            var rentas = _context.Rentas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                rentas = rentas.Where(r =>
                    r.Cliente.Nombre.Contains(searchString) ||
                    r.Empleado.Nombre.Contains(searchString) ||
                    r.Vehiculo.Descripcion.Contains(searchString) ||
                    r.Inspeccion.Id.ToString().Contains(searchString));
            }

            int totalRentas = await rentas.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRentas / (double)pageSize);

            var rentasPaginadas = await rentas
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(rentasPaginadas);
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
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Estado), "Id", "Nombre");
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.Estado), "Id", "Nombre");
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos.Where(v => v.Estado), "Id", "Descripcion");

            var inspeccionesDisponibles = _context.Inspecciones
                .Include(i => i.Vehiculo)
                .Where(i => i.Estado && i.Vehiculo.Estado)
                .Select(i => new
                {
                    i.Id,
                    DisplayText = $"Inspección #{i.Id} - {i.Vehiculo.Descripcion}"
                });

            ViewData["InspeccionId"] = new SelectList(inspeccionesDisponibles, "Id", "DisplayText");
            return View();
        }

        // POST: Rentas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadoId,VehiculoId,ClienteId,InspeccionId,FechaRenta,FechaDevolucion,MontoPorDia,CantidadDias,Comentario,Estado")] Renta renta)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(renta.VehiculoId);
            var empleado = await _context.Empleados.FindAsync(renta.EmpleadoId);
            var inspeccion = await _context.Inspecciones
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == renta.InspeccionId);

            if (vehiculo == null || !vehiculo.Estado)
            {
                ModelState.AddModelError("VehiculoId", "El vehículo seleccionado no está disponible.");
            }
            if (empleado == null || !empleado.Estado)
            {
                ModelState.AddModelError("EmpleadoId", "El empleado seleccionado no está activo.");
            }
            if (inspeccion == null || !inspeccion.Estado)
            {
                ModelState.AddModelError("InspeccionId", "La inspección seleccionada no es válida o no está disponible.");
            }
            if (inspeccion != null && inspeccion.VehiculoId != renta.VehiculoId)
            {
                ModelState.AddModelError("InspeccionId", "La inspección seleccionada no corresponde al vehículo elegido.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    renta.Estado = true;
                    renta.FechaRenta = DateTime.Now;

                    vehiculo.Estado = false;
                    _context.Update(vehiculo);

                    inspeccion.Estado = false;
                    _context.Update(inspeccion);

                    _context.Add(renta);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "¡Renta registrada exitosamente!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocurrió un error al intentar registrar la renta. " + ex.Message);
                }
            }

            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Estado), "Id", "Nombre", renta.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.Estado), "Id", "Nombre", renta.EmpleadoId);
            ViewData["VehiculoId"] = new SelectList(
                _context.Vehiculos.Where(v => v.Estado || v.Id == renta.VehiculoId),
                "Id",
                "Descripcion",
                renta.VehiculoId
            );
            ViewData["InspeccionId"] = new SelectList(
                _context.Inspecciones.Where(i => (i.Estado && i.Vehiculo.Estado) || i.Id == renta.InspeccionId),
                "Id",
                "Id",
                renta.InspeccionId
            );

            return View(renta);
        }

        // GET: Rentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var renta = await _context.Rentas
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (renta == null)
            {
                return NotFound();
            }

            ViewData["IsRented"] = !renta.Vehiculo.Estado;
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Estado), "Id", "Nombre", renta.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.Estado), "Id", "Nombre", renta.EmpleadoId);
            ViewData["VehiculoId"] = new SelectList(
                _context.Vehiculos.Where(v => v.Estado || v.Id == renta.VehiculoId),
                "Id",
                "Descripcion",
                renta.VehiculoId
            );
            ViewData["InspeccionId"] = new SelectList(
                _context.Inspecciones.Where(i => (i.Estado && i.Vehiculo.Estado) || i.Id == renta.InspeccionId),
                "Id",
                "Id",
                renta.InspeccionId
            );
            return View(renta);
        }

        // POST: Rentas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadoId,VehiculoId,ClienteId,InspeccionId,FechaRenta,FechaDevolucion,MontoPorDia,CantidadDias,Comentario,Estado")] Renta renta)
        {
            if (id != renta.Id)
            {
                return NotFound();
            }

            var rentaOriginal = await _context.Rentas
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rentaOriginal == null)
            {
                return NotFound();
            }

            if (rentaOriginal.Estado == false && rentaOriginal.FechaDevolucion.HasValue)
            {
                TempData["ErrorMessage"] = "No se puede editar una renta que ya ha sido devuelta.";
                return RedirectToAction(nameof(Index));
            }

            var nuevaInspeccion = await _context.Inspecciones
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == renta.InspeccionId);

            if (nuevaInspeccion == null || (!nuevaInspeccion.Estado && nuevaInspeccion.Id != rentaOriginal.InspeccionId))
            {
                ModelState.AddModelError("InspeccionId", "La inspección seleccionada no es válida.");
            }
            else if (nuevaInspeccion.VehiculoId != renta.VehiculoId)
            {
                ModelState.AddModelError("InspeccionId", "La inspección seleccionada no corresponde al vehículo elegido.");
            }
            var nuevoVehiculo = await _context.Vehiculos.FindAsync(renta.VehiculoId);
            if (nuevoVehiculo == null || (!nuevoVehiculo.Estado && nuevoVehiculo.Id != rentaOriginal.VehiculoId))
            {
                ModelState.AddModelError("VehiculoId", "El vehículo seleccionado no está disponible.");
            }

            var nuevoEmpleado = await _context.Empleados.FindAsync(renta.EmpleadoId);
            if (nuevoEmpleado == null || !nuevoEmpleado.Estado)
            {
                ModelState.AddModelError("EmpleadoId", "El empleado seleccionado no está activo.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (renta.VehiculoId != rentaOriginal.VehiculoId)
                    {
                        var vehiculoAnterior = await _context.Vehiculos.FindAsync(rentaOriginal.VehiculoId);
                        if (vehiculoAnterior != null)
                        {
                            vehiculoAnterior.Estado = true;
                            _context.Update(vehiculoAnterior);
                        }
                        if (nuevoVehiculo != null)
                        {
                            nuevoVehiculo.Estado = false;
                            _context.Update(nuevoVehiculo);
                        }
                    }

                    if (renta.InspeccionId != rentaOriginal.InspeccionId)
                    {
                        var inspeccionAnterior = await _context.Inspecciones.FindAsync(rentaOriginal.InspeccionId);
                        if (inspeccionAnterior != null)
                        {
                            inspeccionAnterior.Estado = true;
                            _context.Update(inspeccionAnterior);
                        }

                        if (nuevaInspeccion != null)
                        {
                            nuevaInspeccion.Estado = false;
                            _context.Update(nuevaInspeccion);
                        }
                    }

                    _context.Update(renta);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "¡Renta actualizada exitosamente!";
                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocurrió un error al intentar actualizar la renta. " + ex.Message);
                }
            }

            ViewData["IsRented"] = !renta.Vehiculo.Estado;
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Estado), "Id", "Nombre", renta.ClienteId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados.Where(e => e.Estado), "Id", "Nombre", renta.EmpleadoId);
            ViewData["VehiculoId"] = new SelectList(
                _context.Vehiculos.Where(v => v.Estado || v.Id == renta.VehiculoId),
                "Id",
                "Descripcion",
                renta.VehiculoId
            );
            ViewData["InspeccionId"] = new SelectList(
                _context.Inspecciones.Where(i => (i.Estado && i.Vehiculo.Estado) || i.Id == renta.InspeccionId),
                "Id",
                "Id",
                renta.InspeccionId
            );
            return View(renta);
        }

        // GET: Rentas/Devolver/5
        public async Task<IActionResult> Devolver(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var renta = await _context.Rentas
                .Include(r => r.Cliente)
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (renta == null)
            {
                return NotFound();
            }
            if (renta.Estado == false)
            {
                TempData["ErrorMessage"] = "Esta renta ya fue devuelta.";
                return RedirectToAction(nameof(Index));
            }

            return View(renta);
        }

        // POST: Rentas/Devolver/5
        [HttpPost, ActionName("Devolver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevolverConfirmed(int id)
        {
            var renta = await _context.Rentas
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (renta == null)
            {
                return NotFound();
            }

            if (renta.Estado == false)
            {
                TempData["ErrorMessage"] = "Esta renta ya fue devuelta.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                renta.Estado = false;
                renta.FechaDevolucion = DateTime.Now;

                if (renta.Vehiculo != null)
                {
                    renta.Vehiculo.Estado = true;
                    _context.Update(renta.Vehiculo);
                }

                _context.Update(renta);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Renta devuelta exitosamente!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al devolver la renta: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetRentasForDashboard()
        {
            try
            {
                var rentas = await _context.Rentas
                    .Include(r => r.Cliente)
                    .Include(r => r.Empleado)
                    .Include(r => r.Vehiculo)
                    .ThenInclude(v => v.TipoVehiculo)
                    .Include(r => r.Inspeccion)
                    .OrderByDescending(r => r.FechaRenta)
                    .ToListAsync();

                var tiposVehiculo = await _context.Vehiculos
                    .Select(v => v.TipoVehiculo.Descripcion)
                    .Distinct()
                    .ToListAsync();

                var result = rentas.Select(r => new
                {
                    id = r.Id,
                    vehiculo = new
                    {
                        id = r.Vehiculo.Id,
                        descripcion = r.Vehiculo.Descripcion,
                        tipoVehiculo = r.Vehiculo.TipoVehiculo?.Descripcion
                    },
                    cliente = new
                    {
                        id = r.Cliente.Id,
                        nombre = r.Cliente.Nombre
                    },
                    empleado = new
                    {
                        id = r.Empleado.Id,
                        nombre = r.Empleado.Nombre
                    },
                    inspeccion = new
                    {
                        id = r.Inspeccion?.Id,
                        tieneRalladuras = r.Inspeccion?.TieneRalladuras
                    },
                    fechaRenta = r.FechaRenta,
                    fechaDevolucion = r.FechaDevolucion,
                    montoPorDia = r.MontoPorDia,
                    cantidadDias = r.CantidadDias,
                    comentario = r.Comentario,
                    estado = r.Estado
                });

                return Ok(new
                {
                    rentas = result,
                    tiposVehiculo = tiposVehiculo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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
                .Include(r => r.Vehiculo)
                 .Include(r => r.Empleado)
                .Include(r => r.Inspeccion)
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
            var renta = await _context.Rentas
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .Include(r => r.Empleado) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (renta == null)
            {
                return NotFound();
            }

            try
            {
                if (renta.Vehiculo != null)
                {
                    renta.Vehiculo.Estado = true;
                    _context.Update(renta.Vehiculo);
                }

                if (renta.Inspeccion != null)
                {
                    renta.Inspeccion.Estado = true;
                    _context.Update(renta.Inspeccion);
                }

                _context.Rentas.Remove(renta);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Renta eliminada exitosamente!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la renta: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RentaExists(int id)
        {
            return _context.Rentas.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            var rentas = _context.Rentas
                .Include(r => r.Cliente)
                .Include(r => r.Empleado)
                .Include(r => r.Vehiculo)
                .Include(r => r.Inspeccion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                rentas = rentas.Where(r =>
                    r.Cliente.Nombre.Contains(searchString) ||
                    r.Empleado.Nombre.Contains(searchString) ||
                    r.Vehiculo.Descripcion.Contains(searchString) ||
                    r.Inspeccion.Id.ToString().Contains(searchString));
            }

            var rentaList = await rentas.OrderBy(r => r.FechaRenta).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Reporte de Rentas - RentCar")
                        .SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(5);
                            column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                            column.Item().Text($"Total de Rentas: {rentaList.Count}").FontSize(8).AlignRight();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).Padding(5).Text("Fecha Renta").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Vehículo").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Cliente").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Empleado").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Inspección").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Monto Total").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).Padding(5).Text("Estado").SemiBold().FontSize(10);
                                });

                                foreach (var renta in rentaList)
                                {
                                    var montoTotal = renta.MontoPorDia * renta.CantidadDias;
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(renta.FechaRenta?.ToShortDateString() ?? "");
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(renta.Vehiculo.Descripcion);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(renta.Cliente.Nombre);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(renta.Empleado.Nombre);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(renta.Inspeccion?.Id.ToString() ?? "N/A");
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(montoTotal.ToString("C"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(renta.Estado ? "Activa" : "Inactiva").FontColor(renta.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
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
                return File(pdfBytes, "application/pdf", $"ReporteRentas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=ReporteRentas.pdf");
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}