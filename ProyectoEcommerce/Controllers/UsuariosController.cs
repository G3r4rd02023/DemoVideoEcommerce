using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Enums;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Entidades;
using ProyectoEcommerce.Services;

namespace ProyectoEcommerce.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IServicioUsuario _servicioUsuario;
        private readonly IServicioLista _servicioLista;
        private readonly IServicioImagen _servicioImagen;
        private readonly EcommerceContext _context;

        public UsuariosController(IServicioUsuario servicioUsuario, IServicioLista servicioLista,
            IServicioImagen servicioImagen, EcommerceContext context)
        {
            _servicioUsuario = servicioUsuario;
            _servicioLista = servicioLista;
            _servicioImagen = servicioImagen;
            _context = context;
        }

        public async Task<IActionResult> Lista()
        {
            return View(await _context.Users                        
            .ToListAsync());
        }

        public IActionResult Crear()
        {
            UsuarioViewModel model = new()
            {
                Id = Guid.Empty.ToString(),
                TipoUsuario = TipoUsuario.Administrador,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioViewModel model, IFormFile Imagen)
        {
            if (ModelState.IsValid)
            {
                Stream image = Imagen.OpenReadStream();
                string urlImagen = await _servicioImagen.SubirImagen(image, Imagen.FileName);

                model.URLFoto = urlImagen;

                Usuario usuario = await _servicioUsuario.CrearUsuario(model);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo usado.");
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

    }
}
