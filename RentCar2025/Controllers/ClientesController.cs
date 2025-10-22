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
using System.Text.RegularExpressions; // Necesario para la validación numérica opcional

namespace RentCar2025.Controllers
{
    public static class Utilidades
    {

        public static bool ValidaCedula(string pCedula)
        {
            if (string.IsNullOrEmpty(pCedula))
                return false;

            int vnTotal = 0;
            string vcCedula = pCedula.Replace("-", "").Trim();
            int pLongCed = vcCedula.Length;
            int[] digitoMult = new int[11] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1 };

            if (pLongCed != 11)
                return false;


            if (!vcCedula.All(char.IsDigit))
                return false;

            for (int vDig = 1; vDig <= pLongCed; vDig++)
            {
                int digito = Int32.Parse(vcCedula.Substring(vDig - 1, 1));
                int vCalculo = digito * digitoMult[vDig - 1];

                if (vCalculo < 10)
                    vnTotal += vCalculo;
                else

                    vnTotal += (vCalculo / 10) + (vCalculo % 10);
            }


            return (vnTotal % 10 == 0);
        }

        // Nuevo método de validación para el número de tarjeta de crédito
        public static bool ValidaNumeroTarjeta(string pNoTarjetaCR)
        {
            if (string.IsNullOrEmpty(pNoTarjetaCR))
            {
                // Permitimos que sea null/vacío si la tarjeta es opcional, si es obligatoria
                // esta validación debe retornar false. Asumiré que debe ser obligatorio.
                // return false; // Descomenta si debe ser obligatorio y no acepta vacío
                return true; // Asumo que si está vacío es válido si el campo es opcional
            }

            // Remueve cualquier espacio o guión si los permites en la entrada
            string vcTarjeta = pNoTarjetaCR.Replace(" ", "").Replace("-", "").Trim();

            // 1. Validar longitud (Ejemplo: 16 dígitos)
            const int LongitudEsperada = 16;
            if (vcTarjeta.Length != LongitudEsperada)
            {
                return false;
            }

            // 2. Validar que sean solo dígitos
            // Usa Regex para validar si contiene solo dígitos
            if (!Regex.IsMatch(vcTarjeta, @"^\d+$"))
            {
                return false;
            }

            // Opcional: Se podría agregar la validación del Algoritmo de Luhn aquí

            return true;
        }

    }

    public class ClientesController : Controller
    {
        private readonly RentCarDbContext _context;

        public ClientesController(RentCarDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Nombre.Contains(searchString) || c.Cedula.Contains(searchString));
            }

            int totalClientes = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalClientes / (double)pageSize);

            var clientesPaginados = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchString"] = searchString;

            return View(clientesPaginados);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Cedula,NoTarjetaCR,LimiteCredito,TipoPersona,Estado")] Cliente cliente)
        {
            // Validaciones
            if (!Utilidades.ValidaCedula(cliente.Cedula))
            {
                ModelState.AddModelError("Cedula", "El número de Cédula no es válido.");
            }

            if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula))
            {
                ModelState.AddModelError("Cedula", "Ya existe un cliente registrado con esta Cédula.");
            }

            // **NUEVA VALIDACIÓN PARA LA TARJETA DE CRÉDITO**
            if (!string.IsNullOrEmpty(cliente.NoTarjetaCR) && !Utilidades.ValidaNumeroTarjeta(cliente.NoTarjetaCR))
            {
                ModelState.AddModelError("NoTarjetaCR", "El número de Tarjeta de Crédito no es válido. Debe tener 16 dígitos y solo contener números.");
            }
            // ----------------------------------------------------

            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Cliente registrado exitosamente! 🥳";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Cedula,NoTarjetaCR,LimiteCredito,TipoPersona,Estado")] Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            // Validaciones
            if (!Utilidades.ValidaCedula(cliente.Cedula))
            {
                ModelState.AddModelError("Cedula", "El número de Cédula no es válido");
            }

            if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula && c.Id != cliente.Id))
            {
                ModelState.AddModelError("Cedula", "Ya existe otro cliente registrado con esta Cédula.");
            }

            // **NUEVA VALIDACIÓN PARA LA TARJETA DE CRÉDITO**
            if (!string.IsNullOrEmpty(cliente.NoTarjetaCR) && !Utilidades.ValidaNumeroTarjeta(cliente.NoTarjetaCR))
            {
                ModelState.AddModelError("NoTarjetaCR", "El número de Tarjeta de Crédito no es válido. Debe tener 16 dígitos y solo contener números.");
            }
            // ----------------------------------------------------


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "¡Cliente actualizado exitosamente! 🚀";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
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
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tieneRentas = await _context.Rentas.AnyAsync(r => r.ClienteId == id);
            var tieneInspecciones = await _context.Inspecciones.AnyAsync(i => i.ClienteId == id);

            if (tieneRentas)
            {
                TempData["ErrorMessage"] = "¡No se puede eliminar este cliente porque tiene rentas asociadas! 🚫";
                return RedirectToAction(nameof(Index));
            }

            if (tieneInspecciones)
            {
                TempData["ErrorMessage"] = "¡No se puede eliminar este cliente porque tiene inspecciones asociadas! 🚫";
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Cliente eliminado exitosamente! 👋";
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdfReport(string searchString, bool download = true)
        {
            IQueryable<Cliente> clientes = _context.Clientes;
            if (!string.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(c => c.Nombre.Contains(searchString) || c.Cedula.Contains(searchString) || c.NoTarjetaCR.Contains(searchString));
            }

            var clientList = await clientes.OrderBy(c => c.Nombre).ToListAsync();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text("Reporte de Clientes - RentCar").SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(5);
                        column.Item().Text($"Fecha del Reporte: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignRight();
                        column.Item().Text($"Total de Clientes: {clientList.Count}").FontSize(8).AlignRight();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
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
                                header.Cell().BorderBottom(1).Padding(5).Text("No. Tarjeta CR").SemiBold().FontSize(10);
                                header.Cell().BorderBottom(1).Padding(5).Text("Límite Crédito").SemiBold().FontSize(10);
                                header.Cell().BorderBottom(1).Padding(5).Text("Tipo Persona").SemiBold().FontSize(10);
                                header.Cell().BorderBottom(1).Padding(5).Text("Estado").SemiBold().FontSize(10);
                            });

                            foreach (var client in clientList)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(client.Nombre);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(client.Cedula);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(client.NoTarjetaCR);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(client.LimiteCredito.ToString("C"));
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(client.TipoPersona);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(client.Estado ? "Activo" : "Inactivo").FontColor(client.Estado ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            }
                        });
                    });

                    page.Footer().AlignRight().Text(x =>
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
                return File(pdfBytes, "application/pdf", $"ReporteClientes_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            else
            {
                return File(pdfBytes, "application/pdf");
            }
        }
    }
}