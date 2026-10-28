using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrust.Web.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cuenta",
                columns: table => new
                {
                    CuentaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoCuenta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false, defaultValue: "BOB")
                        .Annotation("Relational:DefaultConstraintName", "DF_Cuenta_Moneda"),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaApertura = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],sysdatetime()))")
                        .Annotation("Relational:DefaultConstraintName", "DF_Cuenta_Fecha"),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "ACTIVA")
                        .Annotation("Relational:DefaultConstraintName", "DF_Cuenta_Estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuenta", x => x.CuentaId);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoIdentidad = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.PersonaId);
                });

            migrationBuilder.CreateTable(
                name: "Empleado",
                columns: table => new
                {
                    EmpleadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "ACTIVO")
                        .Annotation("Relational:DefaultConstraintName", "DF_Empleado_Estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleado", x => x.EmpleadoId);
                    table.ForeignKey(
                        name: "FK_Empleado_Persona",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId");
                });

            migrationBuilder.CreateTable(
                name: "Socio",
                columns: table => new
                {
                    SocioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    CodigoSocio = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],sysdatetime()))")
                        .Annotation("Relational:DefaultConstraintName", "DF_Socio_FechaIngreso"),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "ACTIVO")
                        .Annotation("Relational:DefaultConstraintName", "DF_Socio_Estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Socio", x => x.SocioId);
                    table.ForeignKey(
                        name: "FK_Socio_Persona",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId");
                });

            migrationBuilder.CreateTable(
                name: "CuentaSocio",
                columns: table => new
                {
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    SocioId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PuedeOperar = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_CuentaSocio_PuedeOperar"),
                    FechaVinculacion = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],sysdatetime()))")
                        .Annotation("Relational:DefaultConstraintName", "DF_CuentaSocio_Fecha")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentaSocio", x => new { x.CuentaId, x.SocioId });
                    table.ForeignKey(
                        name: "FK_CuentaSocio_Cuenta",
                        column: x => x.CuentaId,
                        principalTable: "Cuenta",
                        principalColumn: "CuentaId");
                    table.ForeignKey(
                        name: "FK_CuentaSocio_Socio",
                        column: x => x.SocioId,
                        principalTable: "Socio",
                        principalColumn: "SocioId");
                });

            migrationBuilder.CreateTable(
                name: "OperacionFinanciera",
                columns: table => new
                {
                    OperacionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoOperacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Operacion_Fecha"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SocioEjecutorId = table.Column<int>(type: "int", nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperacionFinanciera", x => x.OperacionId);
                    table.ForeignKey(
                        name: "FK_Operacion_Socio",
                        column: x => x.SocioEjecutorId,
                        principalTable: "Socio",
                        principalColumn: "SocioId");
                });

            migrationBuilder.CreateTable(
                name: "Prestamo",
                columns: table => new
                {
                    PrestamoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroPrestamo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SocioSolicitanteId = table.Column<int>(type: "int", nullable: false),
                    CuentaDesembolsoId = table.Column<int>(type: "int", nullable: true),
                    EmpleadoAprobadorId = table.Column<int>(type: "int", nullable: true),
                    Capital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TasaInteresAnual = table.Column<decimal>(type: "decimal(7,4)", nullable: false),
                    PlazoMeses = table.Column<short>(type: "smallint", nullable: false),
                    FechaSolicitud = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],sysdatetime()))")
                        .Annotation("Relational:DefaultConstraintName", "DF_Prestamo_Fecha"),
                    FechaDesembolso = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "SOLICITADO")
                        .Annotation("Relational:DefaultConstraintName", "DF_Prestamo_Estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamo", x => x.PrestamoId);
                    table.ForeignKey(
                        name: "FK_Prestamo_Cuenta",
                        column: x => x.CuentaDesembolsoId,
                        principalTable: "Cuenta",
                        principalColumn: "CuentaId");
                    table.ForeignKey(
                        name: "FK_Prestamo_Empleado",
                        column: x => x.EmpleadoAprobadorId,
                        principalTable: "Empleado",
                        principalColumn: "EmpleadoId");
                    table.ForeignKey(
                        name: "FK_Prestamo_Socio",
                        column: x => x.SocioSolicitanteId,
                        principalTable: "Socio",
                        principalColumn: "SocioId");
                });

            migrationBuilder.CreateTable(
                name: "MovimientoCuenta",
                columns: table => new
                {
                    MovimientoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperacionId = table.Column<long>(type: "bigint", nullable: false),
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    Sentido = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_Movimiento_Fecha")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoCuenta", x => x.MovimientoId);
                    table.ForeignKey(
                        name: "FK_Movimiento_Cuenta",
                        column: x => x.CuentaId,
                        principalTable: "Cuenta",
                        principalColumn: "CuentaId");
                    table.ForeignKey(
                        name: "FK_Movimiento_Operacion",
                        column: x => x.OperacionId,
                        principalTable: "OperacionFinanciera",
                        principalColumn: "OperacionId");
                });

            migrationBuilder.CreateTable(
                name: "CuotaPrestamo",
                columns: table => new
                {
                    CuotaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    NumeroCuota = table.Column<short>(type: "smallint", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    CapitalProgramado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InteresProgramado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "PENDIENTE")
                        .Annotation("Relational:DefaultConstraintName", "DF_Cuota_Estado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuotaPrestamo", x => x.CuotaId);
                    table.ForeignKey(
                        name: "FK_Cuota_Prestamo",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "PrestamoId");
                });

            migrationBuilder.CreateTable(
                name: "GarantiaReal",
                columns: table => new
                {
                    GarantiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    PersonaPropietariaId = table.Column<int>(type: "int", nullable: false),
                    TipoGarantia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ValorTasado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaValuacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarantiaReal", x => x.GarantiaId);
                    table.ForeignKey(
                        name: "FK_Garantia_Prestamo",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "PrestamoId");
                    table.ForeignKey(
                        name: "FK_Garantia_Propietario",
                        column: x => x.PersonaPropietariaId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId");
                });

            migrationBuilder.CreateTable(
                name: "PrestamoGarante",
                columns: table => new
                {
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    PersonaGaranteId = table.Column<int>(type: "int", nullable: false),
                    TipoResponsabilidad = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "SOLIDARIA")
                        .Annotation("Relational:DefaultConstraintName", "DF_PrestamoGarante_Tipo"),
                    MontoRespaldado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaAceptacion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrestamoGarante", x => new { x.PrestamoId, x.PersonaGaranteId });
                    table.ForeignKey(
                        name: "FK_PrestamoGarante_Persona",
                        column: x => x.PersonaGaranteId,
                        principalTable: "Persona",
                        principalColumn: "PersonaId");
                    table.ForeignKey(
                        name: "FK_PrestamoGarante_Prestamo",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "PrestamoId");
                });

            migrationBuilder.CreateTable(
                name: "PagoCuota",
                columns: table => new
                {
                    PagoCuotaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuotaId = table.Column<int>(type: "int", nullable: false),
                    OperacionId = table.Column<long>(type: "bigint", nullable: false),
                    MontoAplicado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysdatetime())")
                        .Annotation("Relational:DefaultConstraintName", "DF_PagoCuota_Fecha")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoCuota", x => x.PagoCuotaId);
                    table.ForeignKey(
                        name: "FK_PagoCuota_Cuota",
                        column: x => x.CuotaId,
                        principalTable: "CuotaPrestamo",
                        principalColumn: "CuotaId");
                    table.ForeignKey(
                        name: "FK_PagoCuota_Operacion",
                        column: x => x.OperacionId,
                        principalTable: "OperacionFinanciera",
                        principalColumn: "OperacionId");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Cuenta_Numero",
                table: "Cuenta",
                column: "NumeroCuenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentaSocio_SocioId",
                table: "CuentaSocio",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "UQ_Cuota_PrestamoNumero",
                table: "CuotaPrestamo",
                columns: new[] { "PrestamoId", "NumeroCuota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Empleado_Persona",
                table: "Empleado",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarantiaReal_PersonaPropietariaId",
                table: "GarantiaReal",
                column: "PersonaPropietariaId");

            migrationBuilder.CreateIndex(
                name: "IX_GarantiaReal_PrestamoId",
                table: "GarantiaReal",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoCuenta_CuentaId",
                table: "MovimientoCuenta",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoCuenta_OperacionId",
                table: "MovimientoCuenta",
                column: "OperacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OperacionFinanciera_SocioEjecutorId",
                table: "OperacionFinanciera",
                column: "SocioEjecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_PagoCuota_OperacionId",
                table: "PagoCuota",
                column: "OperacionId");

            migrationBuilder.CreateIndex(
                name: "UQ_PagoCuota_CuotaOperacion",
                table: "PagoCuota",
                columns: new[] { "CuotaId", "OperacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_Documento",
                table: "Persona",
                column: "DocumentoIdentidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_CuentaDesembolsoId",
                table: "Prestamo",
                column: "CuentaDesembolsoId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_EmpleadoAprobadorId",
                table: "Prestamo",
                column: "EmpleadoAprobadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_SocioSolicitanteId",
                table: "Prestamo",
                column: "SocioSolicitanteId");

            migrationBuilder.CreateIndex(
                name: "UQ_Prestamo_Numero",
                table: "Prestamo",
                column: "NumeroPrestamo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrestamoGarante_PersonaGaranteId",
                table: "PrestamoGarante",
                column: "PersonaGaranteId");

            migrationBuilder.CreateIndex(
                name: "UQ_Socio_Codigo",
                table: "Socio",
                column: "CodigoSocio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Socio_Persona",
                table: "Socio",
                column: "PersonaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentaSocio");

            migrationBuilder.DropTable(
                name: "GarantiaReal");

            migrationBuilder.DropTable(
                name: "MovimientoCuenta");

            migrationBuilder.DropTable(
                name: "PagoCuota");

            migrationBuilder.DropTable(
                name: "PrestamoGarante");

            migrationBuilder.DropTable(
                name: "CuotaPrestamo");

            migrationBuilder.DropTable(
                name: "OperacionFinanciera");

            migrationBuilder.DropTable(
                name: "Prestamo");

            migrationBuilder.DropTable(
                name: "Cuenta");

            migrationBuilder.DropTable(
                name: "Empleado");

            migrationBuilder.DropTable(
                name: "Socio");

            migrationBuilder.DropTable(
                name: "Persona");
        }
    }
}
