using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMTecnicos
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [StringLength(50)]
        public string? Especialidad { get; set; }


        [RegularExpression(ValidacionesRegex.Telefono, ErrorMessage = "Teléfono inválido (####-#### o ########)")]
        [StringLength(15)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo inválido")]
        [StringLength(100)]
        public string? Correo { get; set; }
    }
}
