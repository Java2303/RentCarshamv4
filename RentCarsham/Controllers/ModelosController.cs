using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentCarsham.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing; // Para trabajar con imágenes
using System.Drawing;

namespace RentCarsham.Controllers
{
    public class ModelosController : Controller
    {
        private readonly RentCarshamContext _context;

        public ModelosController(RentCarshamContext context)
        {
            _context = context;
        }

        // GET: Modelos
        public async Task<IActionResult> Index()
        {
            var rentCarshamContext = _context.Modelos.Include(m => m.Marca);
            return View(await rentCarshamContext.ToListAsync());
        }

        public async Task<IActionResult> ExportToExcel()
        {
            // Establece el contexto de licencia para EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Obtén los datos desde la base de datos
            var modelos = await _context.Modelos.Include(m => m.Marca).ToListAsync();

            // Crea un paquete de Excel
            using (var package = new ExcelPackage())
            {
                // Añade una hoja de cálculo
                var worksheet = package.Workbook.Worksheets.Add("Modelos");

                // Define encabezados
                worksheet.Cells[1, 1].Value = "Imagen";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "Caja";
                worksheet.Cells[1, 4].Value = "Capacidad Personas";
                worksheet.Cells[1, 5].Value = "Capacidad Maletero";
                worksheet.Cells[1, 6].Value = "Marca";

                // Aplica formato de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Agrega datos a la tabla
                int row = 2;
                foreach (var modelo in modelos)
                {
                    // Inserta datos de texto
                    worksheet.Cells[row, 2].Value = modelo.Nombre;
                    worksheet.Cells[row, 3].Value = modelo.Caja;
                    worksheet.Cells[row, 4].Value = modelo.CapacidadPersonas;
                    worksheet.Cells[row, 5].Value = modelo.CapacidadMaletero;
                    worksheet.Cells[row, 6].Value = modelo.Marca.Nombre;

                    // Inserta la imagen, si existe
                    if (!string.IsNullOrEmpty(modelo.ImagenRuta))
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", modelo.ImagenRuta.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            // Se pasa la ruta del archivo a AddPicture
                            var picture = worksheet.Drawings.AddPicture($"Image_{row}", imagePath);

                            // Ajusta la posición de la imagen
                            picture.SetPosition(row - 1, 0, 0, 0); // Row y columna basados en índices 0
                            picture.SetSize(50, 50); // Tamaño en píxeles
                        }
                    }

                    row++;
                }

                // Ajusta las columnas automáticamente
                worksheet.Cells.AutoFitColumns();

                // Devuelve el archivo Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Modelos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }


        // GET: Modelos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelo = await _context.Modelos
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.ModeloId == id);
            if (modelo == null)
            {
                return NotFound();
            }

            return View(modelo);
        }

        // GET: Modelos/Create
        public IActionResult Create()
        {
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Nombre");
            return View();
        }

        // POST: Modelos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Modelo modelo, IFormFile? imagen)
        {
            if (true)
            {
                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }
                    var fileName = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }
                    modelo.ImagenRuta = "/images/" + fileName;
                }

                _context.Add(modelo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }
    

// GET: Modelos/Edit/5
public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelo = await _context.Modelos.FindAsync(id);
            if (modelo == null)
            {
                return NotFound();
            }
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Nombre", modelo.MarcaId);
            return View(modelo);
        }

        // POST: Modelos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ModeloId,Nombre,Caja,CapacidadPersonas,CapacidadMaletero,MarcaId")] Modelo modelo)
        {
            if (id != modelo.ModeloId)
            {
                return NotFound();
            }

            if (true)
            {
                try
                {
                    _context.Update(modelo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeloExists(modelo.ModeloId))
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
            ViewData["MarcaId"] = new SelectList(_context.Marcas, "MarcaId", "Nombre", modelo.MarcaId);
            return View(modelo);
        }

        // GET: Modelos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelo = await _context.Modelos
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.ModeloId == id);
            if (modelo == null)
            {
                return NotFound();
            }

            return View(modelo);
        }

        // POST: Modelos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modelo = await _context.Modelos.FindAsync(id);
            if (modelo != null)
            {
                _context.Modelos.Remove(modelo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModeloExists(int id)
        {
            return _context.Modelos.Any(e => e.ModeloId == id);
        }
    }
}