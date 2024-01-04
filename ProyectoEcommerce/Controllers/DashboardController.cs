using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Enums;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Services;

namespace ProyectoEcommerce.Controllers
{
    public class DashboardController : Controller
    {

        private readonly EcommerceContext _context;
        private readonly IServicioUsuario _servicioUsuario;

        public DashboardController(EcommerceContext context, IServicioUsuario servicioUsuario)
        {
            _context = context;
            _servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CantidadUsuarios = _context.Users.Count();
            ViewBag.CantidadProductos = _context.Productos.Count();
            ViewBag.CantidadVentas = _context.Ventas.Where(o => o.EstadoPedido == EstadoPedido.Nuevo).Count();
            ViewBag.CantidadVentasConfirmadas = _context.Ventas.Where(o => o.EstadoPedido == EstadoPedido.Confirmado).Count();

            return View(await _context.VentasTemporales
                    .Include(v => v.Usuario)
                    .Include(v => v.Producto).ToListAsync());
        }


    }
}
