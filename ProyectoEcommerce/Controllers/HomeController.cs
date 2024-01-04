using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Entidades;
using ProyectoEcommerce.Services;
using System.Diagnostics;

namespace ProyectoEcommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EcommerceContext _context;
        private readonly IServicioUsuario _servicioUsuario;
        private readonly IServicioVenta _servicioVenta;

        public HomeController(ILogger<HomeController> logger, EcommerceContext context,
            IServicioUsuario servicioUsuario, IServicioVenta servicioVenta)
        {
            _logger = logger;
            _context = context;
            _servicioUsuario = servicioUsuario;
            _servicioVenta = servicioVenta;
        }

        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "NameDesc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "PriceDesc" : "Price";
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            IQueryable<Producto> query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Inventario > 0);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query
                    .Where(p => (p.Nombre.ToLower().Contains(searchString.ToLower()) ||
                                p.Categoria.Nombre.ToLower().Contains(searchString.ToLower())));
            }

            switch (sortOrder)
            {
                case "NameDesc":
                    query = query.OrderByDescending(p => p.Nombre);
                    break;
                default:
                    query = query.OrderBy(p => p.Nombre);
                    break;
            }

            int pageSize = 8;

            CatalogoViewModel model = new()
            {
                Productos = await PaginatedList<Producto>.CreateAsync(query, pageNumber ?? 1, pageSize),
                Categorias = await _context.Categorias.ToListAsync(),

            };

            Usuario usuario = await _servicioUsuario.ObtenerUsuario(User.Identity.Name);
            if (usuario != null)
            {
                model.Cantidad = await _context.VentasTemporales
                .Where(ts => ts.Usuario.Id == usuario.Id)
                .SumAsync(ts => ts.Cantidad);
            }

            return View(model);
        }

        public async Task<IActionResult> AgregarAlCarrito(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("IniciarSesion", "Login");
            }

            Producto producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            Usuario usuario = await _servicioUsuario.ObtenerUsuario(User.Identity.Name);
            if (usuario == null)
            {
                return NotFound();
            }

            VentaTemporal ventaTemporal = new()
            {
                Producto = producto,
                Cantidad = 1,
                Usuario = usuario
            };

            _context.VentasTemporales.Add(ventaTemporal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null || _context.Productos == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [Authorize]
        public async Task<IActionResult> VerCarrito()
        {
            Usuario usuario = await _servicioUsuario.ObtenerUsuario(User.Identity.Name);
            if (usuario == null)
            {
                return NotFound();
            }

            List<VentaTemporal> temporalSale = await _context.VentasTemporales
                .Include(ts => ts.Producto)
                .Where(ts => ts.Usuario.Id == usuario.Id)
                .ToListAsync();

            CarritoViewModel model = new()
            {
                Usuario = usuario,
                VentasTemporales = temporalSale,
            };

            return View(model);
        }

        public async Task<IActionResult> DisminuirCantidad(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VentaTemporal ventaTemporal = await _context.VentasTemporales.FindAsync(id);
            if (ventaTemporal == null)
            {
                return NotFound();
            }

            if (ventaTemporal.Cantidad > 1)
            {
                ventaTemporal.Cantidad--;
                _context.VentasTemporales.Update(ventaTemporal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(VerCarrito));
        }

        public async Task<IActionResult> IncrementarCantidad(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VentaTemporal ventaTemporal = await _context.VentasTemporales.FindAsync(id);
            if (ventaTemporal == null)
            {
                return NotFound();
            }

            ventaTemporal.Cantidad++;
            _context.VentasTemporales.Update(ventaTemporal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(VerCarrito));
        }

        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VentaTemporal ventaTemporal = await _context.VentasTemporales.FindAsync(id);
            if (ventaTemporal == null)
            {
                return NotFound();
            }

            _context.VentasTemporales.Remove(ventaTemporal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(VerCarrito));
        }

        [Authorize]
        public IActionResult ConfirmarVenta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerCarrito(CarritoViewModel model)
        {
            Usuario usuario = await _servicioUsuario.ObtenerUsuario(User.Identity.Name);
            if (usuario == null)
            {
                return NotFound();
            }

            model.Usuario = usuario;
            model.VentasTemporales = await _context.VentasTemporales
                .Include(ts => ts.Producto)
                .Where(ts => ts.Usuario.Id == usuario.Id)
                .ToListAsync();

            Response response = await _servicioVenta.ProcesarVenta(model);
            if (response.IsSuccess)
            {
                return RedirectToAction(nameof(ConfirmarVenta));
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return View(model);
        }


        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
