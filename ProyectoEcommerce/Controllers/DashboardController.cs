using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Enums;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Entidades;
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

        public IActionResult ResumenVenta()
        {

            DateTime FechaInicio = DateTime.Now;
            FechaInicio = FechaInicio.AddDays(-5);

            List<VentasViewModel> Lista = (from tbventa in _context.Ventas
                                   where tbventa.Fecha.Date >= FechaInicio.Date
                                   group tbventa by tbventa.Fecha.Date into grupo
                                   select new VentasViewModel
                                   {
                                       fecha = grupo.Key.ToString("dd/MM/yyyy"),
                                       cantidad = grupo.Count(),
                                   }).ToList();



            return StatusCode(StatusCodes.Status200OK, Lista);
        }

        public IActionResult ResumenProducto()
        {


            List<ProductosViewModel> Lista = (from tbdetalleventa in _context.DetalleVentas
                                      group tbdetalleventa by tbdetalleventa.Producto.Nombre into grupo
                                      orderby grupo.Count() descending
                                      select new ProductosViewModel
                                      {
                                          producto = grupo.Key,
                                          cantidad = grupo.Count(),
                                      }).Take(4).ToList();



            return StatusCode(StatusCodes.Status200OK, Lista);
        }

    }
}
