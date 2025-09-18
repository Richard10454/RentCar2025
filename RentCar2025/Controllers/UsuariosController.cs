using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentCar2025.Migrations.Models;
using Microsoft.AspNetCore.Http; 

namespace RentCar2025.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly RentCarDbContext _context;

        public UsuariosController(RentCarDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuarios == null)
            {
                return NotFound();
            }

            return View(usuarios);
        }

        // GET: Usuarios/Create 
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,correo,Contrasena,Telefono,FechaNacimiento,Genero,Nacionalidad,EstadoCivil")] Usuarios usuarios)
        {
            if (ModelState.IsValid)
            {
                if (_context.Usuarios.Any(u => u.correo == usuarios.correo))
                {
                    ModelState.AddModelError("correo", "Este correo ya está registrado.");
                    return View(usuarios);
                }

                _context.Add(usuarios);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Usuario {usuarios.Nombre} creado exitosamente!";

                HttpContext.Session.SetString("UserName", usuarios.Nombre);

                return RedirectToAction("Index", "Home"); 
            }
            TempData["ErrorMessage"] = "Hubo un error al crear el usuario";
            return View(usuarios);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,correo,Contrasena,Telefono,FechaNacimiento,Genero,Nacionalidad,EstadoCivil")] Usuarios usuarios)
        {
            if (id != usuarios.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarios);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuariosExists(usuarios.Id))
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
            return View(usuarios);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuarios == null)
            {
                return NotFound();
            }

            return View(usuarios);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios != null)
            {
                _context.Usuarios.Remove(usuarios);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuariosExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        // GET: Usuarios/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            var user = await _context.Usuarios
                                     .FirstOrDefaultAsync(u => u.correo == correo && u.Contrasena == contrasena);

            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.Nombre);
                TempData["SuccessMessage"] = $"Bienvenido, {user.Nombre}!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Correo o contraseña inválidos.");
            TempData["ErrorMessage"] = "Correo o contraseña incorrectos.";
            return View();
        }

        // POST: Usuarios/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");
            TempData["SuccessMessage"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Login", "Usuarios"); 
        }
    }
}