namespace EcoInstalacion.Entity.Seguridad;

/// <summary>
/// Catálogo cerrado de roles que pueden operar en el sistema.
/// Los identificadores y nombres deben coincidir con los registros canónicos
/// de la tabla Rol.
/// </summary>
public static class RolesSistema
{
    public const int AdministradorId = 1;
    public const int TecnicoId = 2;

    public const string Administrador = "Administrador";
    public const string Tecnico = "Técnico";
    public const string AdministradorYTecnico = Administrador + "," + Tecnico;

    public static bool EsIdOperativo(int? idRol) =>
        idRol is AdministradorId or TecnicoId;

    public static bool CoincideConCatalogo(int? idRol, string? descripcion)
    {
        var nombre = descripcion?.Trim();

        return idRol switch
        {
            AdministradorId => string.Equals(nombre, Administrador, StringComparison.Ordinal),
            TecnicoId => string.Equals(nombre, Tecnico, StringComparison.Ordinal),
            _ => false
        };
    }

    public static bool EsRolOperativo(Rol? rol) =>
        rol is not null &&
        rol.EsActivo == true &&
        CoincideConCatalogo(rol.IdRol, rol.Descripcion);
}
