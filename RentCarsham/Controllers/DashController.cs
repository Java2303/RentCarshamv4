using Microsoft.AspNetCore.Mvc;
using RentCarsham.Models;
using Newtonsoft.Json;

namespace RentCarsham.Controllers
{
    public class DashboardController : Controller
    {
        private readonly RentCarshamContext _context;

        public DashboardController(RentCarshamContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Datos de Marcas
            var marcasData = _context.Modelos
                .GroupBy(m => m.Marca.Nombre)
                .Select(g => new { Marca = g.Key, Total = g.Count() })
                .ToList();

            var marcasLabels = marcasData.Select(d => d.Marca).ToArray();
            var marcasTotals = marcasData.Select(d => d.Total).ToArray();

            // Datos de Tipos de Caja
            var cajasData = _context.Modelos
                .GroupBy(m => m.Caja)
                .Select(g => new { Caja = g.Key, Total = g.Count() })
                .ToList();

            var cajasLabels = cajasData.Select(d => d.Caja).ToArray();
            var cajasTotals = cajasData.Select(d => d.Total).ToArray();

            // Serializar datos como JSON
            ViewBag.MarcasLabels = JsonConvert.SerializeObject(marcasLabels);
            ViewBag.MarcasTotals = JsonConvert.SerializeObject(marcasTotals);
            ViewBag.CajasLabels = JsonConvert.SerializeObject(cajasLabels);
            ViewBag.CajasTotals = JsonConvert.SerializeObject(cajasTotals);

            return View();
        }
    }
}
