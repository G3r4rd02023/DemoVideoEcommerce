using Microsoft.AspNetCore.Identity;
using ProyectoEcommerce.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProyectoEcommerce.Models.Entidades
{
    public class Usuario : IdentityUser
    {
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Nombre { get; set; }

        [Display(Name = "Foto")]
        public string URLFoto { get; set; } = null;

        public TipoUsuario TipoUsuario { get; set; }


    }
}
