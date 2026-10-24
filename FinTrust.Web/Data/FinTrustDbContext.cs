using System;
using System.Collections.Generic;
using FinTrust.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Data;

public partial class FinTrustDbContext : DbContext
{
    public FinTrustDbContext(DbContextOptions<FinTrustDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CuentaSocio> CuentaSocios { get; set; }

    public virtual DbSet<Cuentum> Cuenta { get; set; }

    public virtual DbSet<CuotaPrestamo> CuotaPrestamos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<GarantiaReal> GarantiaReals { get; set; }

    public virtual DbSet<MovimientoCuentum> MovimientoCuenta { get; set; }

    public virtual DbSet<OperacionFinanciera> OperacionFinancieras { get; set; }

    public virtual DbSet<PagoCuotum> PagoCuota { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<PrestamoGarante> PrestamoGarantes { get; set; }

    public virtual DbSet<Socio> Socios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuentaSocio>(entity =>
        {
            entity.HasKey(e => new { e.CuentaId, e.SocioId });

            entity.ToTable("CuentaSocio");

            entity.HasIndex(e => e.SocioId, "IX_CuentaSocio_SocioId");

            entity.Property(e => e.FechaVinculacion).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_CuentaSocio_Fecha");
            entity.Property(e => e.PuedeOperar).HasDefaultValue(true, "DF_CuentaSocio_PuedeOperar");
            entity.Property(e => e.Rol).HasMaxLength(15);

            entity.HasOne(d => d.Cuenta).WithMany(p => p.CuentaSocios)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentaSocio_Cuenta");

            entity.HasOne(d => d.Socio).WithMany(p => p.CuentaSocios)
                .HasForeignKey(d => d.SocioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentaSocio_Socio");
        });

        modelBuilder.Entity<Cuentum>(entity =>
        {
            entity.HasKey(e => e.CuentaId);

            entity.HasIndex(e => e.NumeroCuenta, "UQ_Cuenta_Numero").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ACTIVA", "DF_Cuenta_Estado");
            entity.Property(e => e.FechaApertura).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_Cuenta_Fecha");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValue("BOB", "DF_Cuenta_Moneda");
            entity.Property(e => e.NumeroCuenta).HasMaxLength(30);
            entity.Property(e => e.SaldoActual).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoCuenta).HasMaxLength(20);
        });

        modelBuilder.Entity<CuotaPrestamo>(entity =>
        {
            entity.HasKey(e => e.CuotaId);

            entity.ToTable("CuotaPrestamo");

            entity.HasIndex(e => new { e.PrestamoId, e.NumeroCuota }, "UQ_Cuota_PrestamoNumero").IsUnique();

            entity.Property(e => e.CapitalProgramado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("PENDIENTE", "DF_Cuota_Estado");
            entity.Property(e => e.InteresProgramado).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Prestamo).WithMany(p => p.CuotaPrestamos)
                .HasForeignKey(d => d.PrestamoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cuota_Prestamo");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("Empleado");

            entity.HasIndex(e => e.PersonaId, "UQ_Empleado_Persona").IsUnique();

            entity.Property(e => e.Cargo).HasMaxLength(80);
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ACTIVO", "DF_Empleado_Estado");

            entity.HasOne(d => d.Persona).WithOne(p => p.Empleado)
                .HasForeignKey<Empleado>(d => d.PersonaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleado_Persona");
        });

        modelBuilder.Entity<GarantiaReal>(entity =>
        {
            entity.HasKey(e => e.GarantiaId);

            entity.ToTable("GarantiaReal");

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.TipoGarantia).HasMaxLength(20);
            entity.Property(e => e.ValorTasado).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.PersonaPropietaria).WithMany(p => p.GarantiaReals)
                .HasForeignKey(d => d.PersonaPropietariaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Garantia_Propietario");

            entity.HasOne(d => d.Prestamo).WithMany(p => p.GarantiaReals)
                .HasForeignKey(d => d.PrestamoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Garantia_Prestamo");
        });

        modelBuilder.Entity<MovimientoCuentum>(entity =>
        {
            entity.HasKey(e => e.MovimientoId);

            entity.Property(e => e.FechaRegistro)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Movimiento_Fecha");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sentido).HasMaxLength(10);

            entity.HasOne(d => d.Cuenta).WithMany(p => p.MovimientoCuenta)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_Cuenta");

            entity.HasOne(d => d.Operacion).WithMany(p => p.MovimientoCuenta)
                .HasForeignKey(d => d.OperacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_Operacion");
        });

        modelBuilder.Entity<OperacionFinanciera>(entity =>
        {
            entity.HasKey(e => e.OperacionId);

            entity.ToTable("OperacionFinanciera");

            entity.Property(e => e.FechaHora)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Operacion_Fecha");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Referencia).HasMaxLength(80);
            entity.Property(e => e.TipoOperacion).HasMaxLength(20);

            entity.HasOne(d => d.SocioEjecutor).WithMany(p => p.OperacionFinancieras)
                .HasForeignKey(d => d.SocioEjecutorId)
                .HasConstraintName("FK_Operacion_Socio");
        });

        modelBuilder.Entity<PagoCuotum>(entity =>
        {
            entity.HasKey(e => e.PagoCuotaId);

            entity.HasIndex(e => new { e.CuotaId, e.OperacionId }, "UQ_PagoCuota_CuotaOperacion").IsUnique();

            entity.Property(e => e.FechaPago)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_PagoCuota_Fecha");
            entity.Property(e => e.MontoAplicado).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Cuota).WithMany(p => p.PagoCuota)
                .HasForeignKey(d => d.CuotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoCuota_Cuota");

            entity.HasOne(d => d.Operacion).WithMany(p => p.PagoCuota)
                .HasForeignKey(d => d.OperacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoCuota_Operacion");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("Persona");

            entity.HasIndex(e => e.DocumentoIdentidad, "UQ_Persona_Documento").IsUnique();

            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.DocumentoIdentidad).HasMaxLength(25);
            entity.Property(e => e.Nombres).HasMaxLength(80);
            entity.Property(e => e.Telefono).HasMaxLength(25);
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.ToTable("Prestamo");

            entity.HasIndex(e => e.NumeroPrestamo, "UQ_Prestamo_Numero").IsUnique();

            entity.Property(e => e.Capital).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("SOLICITADO", "DF_Prestamo_Estado");
            entity.Property(e => e.FechaSolicitud).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_Prestamo_Fecha");
            entity.Property(e => e.NumeroPrestamo).HasMaxLength(30);
            entity.Property(e => e.TasaInteresAnual).HasColumnType("decimal(7, 4)");

            entity.HasOne(d => d.CuentaDesembolso).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.CuentaDesembolsoId)
                .HasConstraintName("FK_Prestamo_Cuenta");

            entity.HasOne(d => d.EmpleadoAprobador).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.EmpleadoAprobadorId)
                .HasConstraintName("FK_Prestamo_Empleado");

            entity.HasOne(d => d.SocioSolicitante).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.SocioSolicitanteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prestamo_Socio");
        });

        modelBuilder.Entity<PrestamoGarante>(entity =>
        {
            entity.HasKey(e => new { e.PrestamoId, e.PersonaGaranteId });

            entity.ToTable("PrestamoGarante");

            entity.Property(e => e.MontoRespaldado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoResponsabilidad)
                .HasMaxLength(15)
                .HasDefaultValue("SOLIDARIA", "DF_PrestamoGarante_Tipo");

            entity.HasOne(d => d.PersonaGarante).WithMany(p => p.PrestamoGarantes)
                .HasForeignKey(d => d.PersonaGaranteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrestamoGarante_Persona");

            entity.HasOne(d => d.Prestamo).WithMany(p => p.PrestamoGarantes)
                .HasForeignKey(d => d.PrestamoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrestamoGarante_Prestamo");
        });

        modelBuilder.Entity<Socio>(entity =>
        {
            entity.ToTable("Socio");

            entity.HasIndex(e => e.CodigoSocio, "UQ_Socio_Codigo").IsUnique();

            entity.HasIndex(e => e.PersonaId, "UQ_Socio_Persona").IsUnique();

            entity.Property(e => e.CodigoSocio).HasMaxLength(25);
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValue("ACTIVO", "DF_Socio_Estado");
            entity.Property(e => e.FechaIngreso).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_Socio_FechaIngreso");

            entity.HasOne(d => d.Persona).WithOne(p => p.Socio)
                .HasForeignKey<Socio>(d => d.PersonaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Socio_Persona");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
