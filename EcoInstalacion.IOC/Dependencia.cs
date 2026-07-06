using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using EcoInstalacion.BLL.Implementacion;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Implementacion;
using EcoInstalacion.DAL.Interfaces;

namespace EcoInstalacion.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencia(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISolicitudRepository, SolicitudRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<ITecnicoRepository, TecnicoRepository>();
            services.AddScoped<IVisitaTecnicaRepository, VisitaTecnicaRepository>();
            services.AddScoped<IReporteRepository, ReporteRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ICorreoService, CorreoService>();
            services.AddScoped<IFirebaseService, FirebaseService>();
            services.AddScoped<IUtilidadesService, UtilidadesService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<ITecnicoService, TecnicoService>();
            services.AddScoped<ISolicitudService, SolicitudService>();
            services.AddScoped<IVisitaTecnicaService, VisitaTecnicaService>();
            services.AddScoped<IReporteService, ReporteService>();
            services.AddScoped<IReportePdfService, ReportePdfService>();
        }
    }
}
