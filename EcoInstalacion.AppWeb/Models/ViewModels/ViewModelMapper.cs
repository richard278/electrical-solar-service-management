using EcoInstalacion.Entity;

namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public static class ViewModelMapper
    {
        // Clientes
        public static VMClientes ToVM(this Cliente e) => new()
        {
            Id = e.Id,
            NombreCompleto = e.NombreCompleto,
            DUI = e.Dui,
            Telefono = e.Telefono,
            Direccion = e.Direccion,
            Correo = e.Correo
        };

        public static Cliente ToEntity(this VMClientes vm) => new()
        {
            Id = vm.Id,
            NombreCompleto = vm.NombreCompleto ?? string.Empty, // Ensure non-null value
            Dui = vm.DUI ?? string.Empty, // Ensure non-null value
            Telefono = vm.Telefono ?? string.Empty, // Ensure non-null value
            Direccion = vm.Direccion ?? string.Empty, // Ensure non-null value
            Correo = vm.Correo ?? string.Empty // Ensure non-null value
        };

        // Técnicos
        public static VMTecnicos ToVM(this Tecnico e) => new()
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Especialidad = e.Especialidad,
            Telefono = e.Telefono,
            Correo = e.Correo
        };

        public static Tecnico ToEntity(this VMTecnicos vm) => new()
        {
            Id = vm.Id,
            Nombre = vm.Nombre,
            Especialidad = vm.Especialidad,
            Telefono = vm.Telefono,
            Correo = vm.Correo
        };

        // Solicitudes
        public static VMSolicitudesInstalacion ToVM(this SolicitudesInstalacion e) => new()
        {
            Id = e.Id,
            FechaSolicitud = e.FechaSolicitud,
            ClienteId = e.ClienteId,
            ClienteNombre = e.Cliente?.NombreCompleto,
            TipoInstalacion = e.TipoInstalacion,
            Descripcion = e.Descripcion,
            Estado = e.Estado,
            TecnicoAsignadoId = e.TecnicoAsignadoId,
            TecnicoNombre = e.TecnicoAsignado?.Nombre
        };

        public static SolicitudesInstalacion ToEntity(this VMSolicitudesInstalacion vm) => new()
        {
            Id = vm.Id,

            // Fecha: si por alguna razón viniera null, usamos ahora mismo,
            // pero en teoría ModelState ya obliga a enviarla.
            FechaSolicitud = vm.FechaSolicitud ?? DateTime.UtcNow,

            // 👇 conversión explícita de int? → int
            ClienteId = vm.ClienteId!.Value,

            TipoInstalacion = vm.TipoInstalacion,
            Descripcion = vm.Descripcion,
            Estado = string.IsNullOrWhiteSpace(vm.Estado)
                                ? EstadosSolicitud.Pendiente
                                : vm.Estado,
            TecnicoAsignadoId = vm.TecnicoAsignadoId
        };

        // Visitas
        public static VMVisitasTecnicas ToVM(this VisitasTecnica e) => new()
        {
            Id = e.Id,
            SolicitudId = e.SolicitudId,
            FechaVisita = e.FechaVisita,
            Observaciones = e.Observaciones,
            Aprobado = e.Aprobado ?? false//Provee default value para nullable bool
        };

        public static VisitasTecnica ToEntity(this VMVisitasTecnicas vm) => new()
        {
            Id = vm.Id,
            // Si en el VM SolicitudId también es int?:
            // SolicitudId = vm.SolicitudId!.Value,
            SolicitudId = vm.SolicitudId,
            FechaVisita = vm.FechaVisita,
            Observaciones = vm.Observaciones,
            Aprobado = vm.Aprobado
        };
    }
}
