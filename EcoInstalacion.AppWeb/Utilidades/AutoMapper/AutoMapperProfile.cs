using AutoMapper;
using EcoInstalacion.AppWeb.Models.ViewModels;
using EcoInstalacion.Entity;
/// <summary>
/// Perfil de mapeo central para VMRol, VMUsuario y VMPerfil.
/// - Convierte bool <-> int para EsActivo (1 = Activo, 2 = Inactivo).
/// - Expone NombreRol/Rol desde la navegación del Rol con null-safety.
/// - Ignora navegaciones al mapear de VM -> Entity (se resuelven por FK).
/// </summary>

namespace EcoInstalacion.AppWeb.Utilidades.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ========== Rol ==========
            // Entity -> VM
            CreateMap<Rol, VMRol>()
                .ForMember(dest => dest.EsActivo,
                    opt => opt.MapFrom(src => src.EsActivo == true ? 1 : 2));

            // VM -> Entity
            CreateMap<VMRol, Rol>()
                .ForMember(dest => dest.EsActivo,
                    opt => opt.MapFrom(src => src.EsActivo == 1));

            // ========== Usuario ==========
            // Entity -> VM
            CreateMap<Usuario, VMUsuario>()
                .ForMember(d => d.EsActivo, o => o.MapFrom(s => s.EsActivo== true ? 1 : 2))
                .ForMember(d => d.NombreRol, o => o.MapFrom(
                    s => s.IdRolNavigation != null ? s.IdRolNavigation.Descripcion : string.Empty
                ));

            // VM -> Entity
            CreateMap<VMUsuario, Usuario>()
                .ForMember(d => d.EsActivo, o => o.MapFrom(s => s.EsActivo == 1))
                .ForMember(d => d.IdRolNavigation, o => o.Ignore()); // se maneja por FK IdRol

            // ========== Perfil (vista de perfil) ==========
            // Entity -> VMPerfil (para mostrar datos y foto actual)
            // NOTA: aquí asumo que tu VMPerfil tiene la propiedad "Rol" (según tu snippet).
            // Si en tu VM usas "RolNombre", cambia el nombre en el ForMember.
            CreateMap<Usuario, VMPerfil>()
                .ForMember(d => d.Rol, m => m.MapFrom(
                    s => s.IdRolNavigation != null ? s.IdRolNavigation.Descripcion : string.Empty
                ))
                // Si quieres llenar UrlFoto con una imagen por defecto cuando venga null/vacío,
                // descomenta la siguiente línea y ajusta la ruta:
               // .ForMember(d => d.UrlFoto, m => m.MapFrom(s => string.IsNullOrWhiteSpace(s.UrlFoto) ? "/img/default-user.png" : s.UrlFoto))
                ;

            // VMPerfil -> Entity (para GuardarPerfil: correo/teléfono/nombre; la imagen la maneja el servicio)
            CreateMap<VMPerfil, Usuario>()
                .ForMember(d => d.IdRolNavigation, o => o.Ignore())
                .ForMember(d => d.UrlFoto, o => o.Ignore()) // lo setea el service tras guardar el archivo
                .ForMember(d => d.NombreFoto, o => o.Ignore());

            CreateMap<Cliente, VMClientes>()
           .ForMember(d => d.EsActivo, o => o.MapFrom(s => s.EsActivo))
           .ReverseMap();

            CreateMap<Tecnico, VMTecnicos>().ReverseMap();

        }
    }
}