using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMClientes
    {
        public int Id { get; set; }

        // ------------------------------
        // NOMBRE COMPLETO
        // ------------------------------
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [RegularExpression(ValidacionesRegex.SoloLetras, ErrorMessage = "Solo se permiten letras y espacios")]
        public string NombreCompleto { get; set; } = string.Empty;

        // ------------------------------
        // DUI (########-#)
        // ------------------------------
        [Required(ErrorMessage = "El DUI es obligatorio")]
        [RegularExpression(ValidacionesRegex.DUI, ErrorMessage = "Formato DUI inválido (########-#)")]
        public string DUI { get; set; } = string.Empty;

        // ------------------------------
        // TELÉFONO
        // ------------------------------
        [RegularExpression(ValidacionesRegex.Telefono, ErrorMessage = "Teléfono inválido (######## o ####-####)")]
        [StringLength(15)]
        public string? Telefono { get; set; }

        // ------------------------------
        // DIRECCIÓN
        // ------------------------------
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
        [RegularExpression(ValidacionesRegex.Direccion, ErrorMessage = "Dirección contiene caracteres no permitidos")]
        public string? Direccion { get; set; }

        // ------------------------------
        // CORREO
        // ------------------------------
        [StringLength(100)]
        [RegularExpression(ValidacionesRegex.Correo, ErrorMessage = "Correo electrónico inválido")]
        public string? Correo { get; set; }

        // ------------------------------
        // ACTIVO
        // ------------------------------
        public bool EsActivo { get; set; }

    }
}