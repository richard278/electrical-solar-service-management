using EcoInstalacion.Entity;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public static class EstadosSolicitud
    {
        public const string Pendiente = EstadosSolicitudSistema.Pendiente;
        public const string Programada = EstadosSolicitudSistema.Programada;
        public const string EnProceso = EstadosSolicitudSistema.EnProceso;
        public const string Cerrada = EstadosSolicitudSistema.Cerrada;
        public const string Rechazada = EstadosSolicitudSistema.Rechazada;

        public static IReadOnlyList<string> Todos => EstadosSolicitudSistema.Todos;
    }
}
