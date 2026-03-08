using EcoInstalacion.Entity;
using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMUsuario
    {
        public int IdUsuario { get; set; }

        // ------------------------------
        // NOMBRE
        // ------------------------------
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            ValidacionesRegex.SoloLetras,
            ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string? Nombre { get; set; }

        // ------------------------------
        // CORREO
        // ------------------------------
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(
            ValidacionesRegex.Correo,
            ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(120, ErrorMessage = "El correo es demasiado largo.")]
        public string? Correo { get; set; }

        // ------------------------------
        // TELÉFONO
        // Solo 8 dígitos numéricos
        // ------------------------------
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(
            ValidacionesRegex.Telefono,
            ErrorMessage = "El teléfono debe tener exactamente 8 dígitos.")]
        public string? Telefono { get; set; }

        // ------------------------------
        // ROL
        // ------------------------------
        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public int? IdRol { get; set; }

        public string? NombreRol { get; set; }

        // ------------------------------
        // FOTO (Opcional)
        // ------------------------------
        public string? UrlFoto { get; set; }
        public string? NombreFoto { get; set; }

        // ------------------------------
        // ESTADO (1 = Activo, 0 = Inactivo)
        // ------------------------------
        [Required(ErrorMessage = "Debe indicar si el usuario está activo.")]
        [Range(0, 1, ErrorMessage = "Valor de estado inválido.")]
        public int? EsActivo { get; set; }

    }
}
