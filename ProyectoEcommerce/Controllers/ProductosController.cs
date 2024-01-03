using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Entidades;
using ProyectoEcommerce.Services;

namespace ProyectoEcommerce.Controllers
{
    public class ProductosController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IServicioLista _servicioLista;
        private readonly IServicioImagen _servicioImagen;

        public ProductosController(EcommerceContext context, IServicioLista servicioLista, IServicioImagen servicioImagen)
        {
            _context = context;
            _servicioLista = servicioLista;
            _servicioImagen = servicioImagen;
        }

        public async Task<IActionResult> Lista()
        {
            return View(await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync());
        }

        public async Task<IActionResult> Crear()
        {
            Producto producto = new()
            {
                Categorias = await _servicioLista.GetListaCategorias(),
            };

            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Producto producto, IFormFile Imagen)
        {
            if (ModelState.IsValid)
            {

                Stream image = Imagen.OpenReadStream();
                string urlimagen = await _servicioImagen.SubirImagen(image, Imagen.FileName);

                try
                {

                    producto.URLFoto = urlimagen;
                    _context.Add(producto);
                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Producto creado exitosamente!!!";
                    return RedirectToAction("Lista");
                }
                catch
                {
                    ModelState.AddModelError(String.Empty, "Ha ocurrido un error");
                }
            }
            producto.Categorias = await _servicioLista.GetListaCategorias();

            return View(producto);
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.Categorias = await _servicioLista.GetListaCategorias();


            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Producto producto, IFormFile Imagen)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var productoExistente = await _context.Productos.FindAsync(producto.Id);
                    if (productoExistente == null)
                    {
                        return NotFound();
                    }

                    if (Imagen != null)
                    {
                        Stream image = Imagen.OpenReadStream();
                        string urlimagen = await _servicioImagen.SubirImagen(image, Imagen.FileName);
                        productoExistente.URLFoto = urlimagen;
                    }

                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.Categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
                    productoExistente.Precio = producto.Precio;
                    productoExistente.Inventario = producto.Inventario;

                    _context.Update(productoExistente);
                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Producto actualizado exitosamente!!!";
                    return RedirectToAction("Lista");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Ha ocurrido un error");
                }
            }

            producto.Categorias = await _servicioLista.GetListaCategorias();


            return View(producto);
        }

        public async Task<IActionResult> Eliminar(int? id)
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

            try
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Producto eliminado exitosamente!!!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Ocurrio un error, no se pudo eliminar el registro");
            }

            return RedirectToAction(nameof(Lista));
        }
    }
}
