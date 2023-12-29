using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoEcommerce.Services
{
    public interface IServicioLista
    {
        Task<IEnumerable<SelectListItem>> GetListaCategorias();
    }
}
