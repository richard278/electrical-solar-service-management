using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoInstalacion.Entity
{
    public class ReporteTecnicosAsignados
    {
        public string Tecnico { get; set; } = "";
        public string Especialidad { get; set; } = "";
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaVisita { get; set; }
        public string? TipoInstalacion { get; set; } = "";
        public string? Descripcion { get; set; } = "";
        public string? Estado { get; set; } = "";
        public string Observaciones { get; set; } = "";
        public bool Aprobado { get; set; }
    }
}
