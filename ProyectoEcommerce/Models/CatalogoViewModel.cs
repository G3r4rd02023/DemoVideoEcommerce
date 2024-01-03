using ProyectoEcommerce.Models.Entidades;
using ProyectoEcommerce.Services;

namespace ProyectoEcommerce.Models
{
    public class CatalogoViewModel
    {
        public int Cantidad { get; set; }

        public PaginatedList<Producto> Productos { get; set; }

        public ICollection<Categoria> Categorias { get; set; }
    }
}
