using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Enums;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Entidades;
using ProyectoEcommerce.Services;
using Vereyon.Web;

namespace ProyectoEcommerce.Controllers
{
    
    public class VentasController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IServicioVenta _servicioVenta;
        private readonly IFlashMessage _flashMessage;

        public VentasController(EcommerceContext context, IServicioVenta servicioVenta, IFlashMessage flashMessage)
        {
            _context = context;
            _servicioVenta = servicioVenta;
            _flashMessage = flashMessage;
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ventas
                .Include(s => s.Usuario)
                .Include(s => s.DetallesVenta)
                .ThenInclude(sd => sd.Producto)
                .ToListAsync());
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Venta venta = await _context.Ventas
                .Include(s => s.Usuario)
                .Include(s => s.DetallesVenta)
                .ThenInclude(sd => sd.Producto)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisCompras()
        {
            var ventas = await _context.Ventas
           .Include(s => s.Usuario)
           .Include(s => s.DetallesVenta)
           .ThenInclude(sd => sd.Producto)
           .Where(s => s.Usuario.Email == User.Identity.Name)
           .ToListAsync();

            var usuario = await _context.Users 
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefaultAsync();

            ViewBag.UsuarioActual = usuario;

            return View(ventas);
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisDetalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Venta venta = await _context.Ventas
                .Include(s => s.Usuario)
                .Include(s => s.DetallesVenta)
                .ThenInclude(sd => sd.Producto)
                .Where(s => s.Usuario.Email == User.Identity.Name)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        public async Task<IActionResult> Despachar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Venta venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            if (venta.EstadoPedido != EstadoPedido.Nuevo)
            {
                _flashMessage.Danger("Solo se pueden despachar pedidos que estén en estado 'nuevo'.");
            }
            else
            {
                venta.EstadoPedido = EstadoPedido.Despachado;
                _context.Ventas.Update(venta);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'despachado'.");
            }
            return RedirectToAction(nameof(Detalles), new { venta.Id });
        }

        public async Task<IActionResult> Enviar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Venta venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            if (venta.EstadoPedido != EstadoPedido.Despachado)
            {
                _flashMessage.Danger("Solo se pueden enviar pedidos que estén en estado 'despachado'.");
            }
            else
            {
                venta.EstadoPedido = EstadoPedido.Enviado;
                _context.Ventas.Update(venta);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'enviado'.");
            }
            return RedirectToAction(nameof(Detalles), new { venta.Id });
        }

        public async Task<IActionResult> Confirmar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Venta venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            if (venta.EstadoPedido != EstadoPedido.Enviado)
            {
                _flashMessage.Danger("Solo se pueden confirmar pedidos que estén en estado 'enviado'.");
            }
            else
            {
                venta.EstadoPedido = EstadoPedido.Confirmado;
                _context.Ventas.Update(venta);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'confirmado'.");
            }
            return RedirectToAction(nameof(Detalles), new {venta.Id });
        }

        public async Task<IActionResult> Cancelar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Venta venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            if (venta.EstadoPedido == EstadoPedido.Cancelado)
            {
                _flashMessage.Danger("No se puede cancelar un pedido que esté en estado 'cancelado'.");
            }
            else
            {
                await _servicioVenta.CancelarVenta(venta.Id);
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'cancelado'.");
            }
            return RedirectToAction(nameof(Detalles), new {venta.Id });


        }
    }
}
