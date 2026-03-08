using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using EcoInstalacion.Entity;

namespace EcoInstalacion.DAL.EcoInstaladorContext;

public partial class EcoInstaladorContext : DbContext
{
    public EcoInstaladorContext()
    {

    }

    public EcoInstaladorContext(DbContextOptions<EcoInstaladorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<SolicitudesInstalacion> SolicitudesInstalacions { get; set; }

    public virtual DbSet<Tecnico> Tecnicos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VisitasTecnica> VisitasTecnicas { get; set; }

    public virtual DbSet<Configuracion> Configuracions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3214EC070E50B77C");

            entity.HasIndex(e => e.Dui, "UQ__Clientes__C03671B9C52168E1").IsUnique();

            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(150);
            entity.Property(e => e.Dui)
                .HasMaxLength(10)
                .IsRequired()
                .HasColumnName("DUI");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.Telefono)
                .HasMaxLength(15);
            entity.Property(e => e.EsActivo)
                .IsRequired()
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__3C872F76157C4FE8");

            entity.ToTable("Rol");

            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
        });

        modelBuilder.Entity<SolicitudesInstalacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Solicitu__3214EC07F23B5D91");

            entity.ToTable("SolicitudesInstalacion");

            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TipoInstalacion).HasMaxLength(50);

            entity.HasOne(d => d.Cliente).WithMany(p => p.SolicitudesInstalacions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solicitud__Clien__412EB0B6");

            entity.HasOne(d => d.TecnicoAsignado).WithMany(p => p.SolicitudesInstalacions)
                .HasForeignKey(d => d.TecnicoAsignadoId)
                .HasConstraintName("FK__Solicitud__Tecni__440B1D61");
        });

        modelBuilder.Entity<Tecnico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tecnicos__3214EC07230C41D6");

            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Especialidad).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(15);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A678229B6A");

            entity.ToTable("Usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("clave");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NombreFoto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreFoto");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.UrlFoto)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("urlFoto");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Usuario__idRol__59063A47");
        });

        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Configuracion");

            entity.Property(e => e.Propiedad)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("propiedad");
            entity.Property(e => e.Recurso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("recurso");
            entity.Property(e => e.Valor)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("valor");
        });

        modelBuilder.Entity<VisitasTecnica>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VisitasT__3214EC079CE9F9B4");

            entity.ToTable(tb => tb.HasTrigger("tr_ActualizarEstadoSolicitud"));

            entity.Property(e => e.Aprobado).HasDefaultValue(false);
            entity.Property(e => e.FechaVisita).HasColumnType("datetime");
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(d => d.Solicitud).WithMany(p => p.VisitasTecnicas)
                .HasForeignKey(d => d.SolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VisitasTe__Solic__46E78A0C");
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}