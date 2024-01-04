using ProyectoEcommerce.Models.Entidades;
using System.ComponentModel.DataAnnotations;

namespace ProyectoEcommerce.Models
{
    public class CarritoViewModel
    {
        public Usuario Usuario { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comentario { get; set; } = null;

        public ICollection<VentaTemporal> VentasTemporales { get; set; }

        public int Cantidad => VentasTemporales == null ? 0 : VentasTemporales.Sum(ts => ts.Cantidad);

        [DisplayFormat(DataFormatString = "{0:C2}")]

        public decimal Total => VentasTemporales == null ? 0 : VentasTemporales.Sum(ts => ts.Total);
    }
}
