using System.ComponentModel.DataAnnotations;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class VMRol
    {
        public int IdRol { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        // 1 = Activo, 2 = Inactivo (mismo patrón que Usuario)
        [Range(1, 2, ErrorMessage = "Valor inválido para estado.")]
        public int EsActivo { get; set; } = 1;

        // Conveniente para la grilla
        public string EstadoTexto => EsActivo == 1 ? "Activo" : "Inactivo";
    }
}
