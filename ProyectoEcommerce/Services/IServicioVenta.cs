using ProyectoEcommerce.Models;

namespace ProyectoEcommerce.Services
{
    public interface IServicioVenta
    {
        Task<Response> ProcesarVenta(CarritoViewModel model);
        Task<Response> CancelarVenta(int id);
    }
}
