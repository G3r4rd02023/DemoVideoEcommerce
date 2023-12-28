using Microsoft.AspNetCore.Identity;
using ProyectoEcommerce.Models.Entidades;

namespace ProyectoEcommerce.Services
{
    public interface IServicioUsuario
    {
        Task<Usuario> ObtenerUsuario(string email);
        Task<IdentityResult> CrearUsuario(Usuario usuario, string password);
        Task VerificarRol(string nombreRol);
        Task AsignarRol(Usuario usuario, string nombreRol);
        Task<bool> UsuarioEnRol(Usuario usuario, string nombreRol);
    }

}
