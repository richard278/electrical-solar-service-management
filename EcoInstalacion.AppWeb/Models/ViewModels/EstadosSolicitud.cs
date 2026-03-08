namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public class EstadosSolicitud
    {
        public const string Pendiente = "Pendiente";
        public const string Programada = "Programada";
        public const string EnProceso = "En Proceso";
        public const string Cerrada = "Cerrada";
        public const string Rechazada = "Rechazada";

        public static readonly string[] Todos =
            { Pendiente, Programada, EnProceso, Cerrada, Rechazada };
    }
}
