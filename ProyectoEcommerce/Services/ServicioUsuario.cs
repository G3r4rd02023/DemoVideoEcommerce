using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Entidades;

namespace ProyectoEcommerce.Services
{
    public class ServicioUsuario : IServicioUsuario
    {
        private readonly EcommerceContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ServicioUsuario(EcommerceContext context, 
            UserManager<Usuario> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task AsignarRol(Usuario usuario, string nombreRol)
        {
            await _userManager.AddToRoleAsync(usuario, nombreRol);
        }

        public async Task<IdentityResult> CrearUsuario(Usuario usuario, string password)
        {
            return await _userManager.CreateAsync(usuario, password);
        }

        public async Task<Usuario> ObtenerUsuario(string email)
        {
            return await _context.Users           
            .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UsuarioEnRol(Usuario usuario, string nombreRol)
        {
            return await _userManager.IsInRoleAsync(usuario, nombreRol);
        }

        public async Task VerificarRol(string nombreRol)
        {
            bool roleExiste = await _roleManager.RoleExistsAsync(nombreRol);
            if (!roleExiste)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = nombreRol
                });
            }

        }
    }
}
