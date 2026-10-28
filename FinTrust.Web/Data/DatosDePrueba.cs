using Bogus;
using FinTrust.Web.Models;

namespace FinTrust.Web.Data;

public static class DatosDePrueba
{
    public static void Cargar(FinTrustDbContext contexto)
    {
        if (contexto.Personas.Any())
            return;

        AgregarDatos(contexto);
        contexto.SaveChanges();
    }

    private static void AgregarDatos(FinTrustDbContext contexto)
    {
        Randomizer.Seed = new Random(2026);
        var f = new Faker("es");
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var personas = new Faker<Persona>("es")
            .RuleFor(p => p.DocumentoIdentidad, x => (5000000 + x.IndexFaker).ToString())
            .RuleFor(p => p.Nombres, x => x.Name.FirstName())
            .RuleFor(p => p.Apellidos, x => x.Name.LastName() + " " + x.Name.LastName())
            .RuleFor(p => p.Telefono, x => x.Phone.PhoneNumber("7#######"))
            .RuleFor(p => p.Correo, (x, p) => x.Internet.Email(p.Nombres, p.Apellidos, "correo.com").ToLower())
            .Generate(20);

        contexto.Personas.AddRange(personas);

        var socios = new List<Socio>();
        var cuentas = new List<Cuentum>();

        for (int i = 0; i < 15; i++)
        {
            var socio = new Socio
            {
                Persona = personas[i],
                CodigoSocio = $"SOC-{i + 1:D3}",
                FechaIngreso = hoy.AddDays(-f.Random.Int(400, 1000)),
                Estado = "ACTIVO"
            };

            var cuenta = new Cuentum
            {
                NumeroCuenta = $"CTA-{i + 1:D4}",
                TipoCuenta = f.PickRandom("AHORRO", "CORRIENTE"),
                Moneda = "BOB",
                SaldoActual = f.Random.Int(100, 20000),
                FechaApertura = socio.FechaIngreso,
                Estado = "ACTIVA"
            };

            contexto.CuentaSocios.Add(new CuentaSocio
            {
                Cuenta = cuenta,
                Socio = socio,
                Rol = "TITULAR",
                PuedeOperar = true,
                FechaVinculacion = cuenta.FechaApertura
            });

            socios.Add(socio);
            cuentas.Add(cuenta);
        }

        var oficial = new Empleado
        {
            Persona = personas[15],
            Cargo = "Oficial de créditos",
            FechaIngreso = hoy.AddYears(-2),
            Estado = "ACTIVO"
        };

        contexto.Empleados.Add(oficial);
        contexto.Empleados.Add(new Empleado
        {
            Persona = personas[16],
            Cargo = "Cajero",
            FechaIngreso = hoy.AddYears(-1),
            Estado = "ACTIVO"
        });

        for (int i = 0; i < 10; i++)
        {
            int s = f.Random.Int(0, socios.Count - 1);
            var fechaSolicitud = hoy.AddDays(-f.Random.Int(60, 300));
            string estado = f.PickRandom("SOLICITADO", "APROBADO", "DESEMBOLSADO", "DESEMBOLSADO");
            bool desembolsado = estado == "DESEMBOLSADO";

            var prestamo = new Prestamo
            {
                NumeroPrestamo = $"PRE-{i + 1:D3}",
                SocioSolicitante = socios[s],
                CuentaDesembolso = desembolsado ? cuentas[s] : null,
                EmpleadoAprobador = estado == "SOLICITADO" ? null : oficial,
                Capital = f.Random.Int(10, 300) * 100m,
                TasaInteresAnual = f.PickRandom(10.5m, 12m, 12.5m, 14m),
                PlazoMeses = f.PickRandom<short>(6, 12, 24),
                FechaSolicitud = fechaSolicitud,
                FechaDesembolso = desembolsado ? fechaSolicitud.AddDays(7) : null,
                Estado = estado
            };

            contexto.Prestamos.Add(prestamo);

            if (desembolsado)
                AgregarCuotasYPrimerPago(contexto, prestamo);
        }
    }

    private static void AgregarCuotasYPrimerPago(FinTrustDbContext contexto, Prestamo prestamo)
    {
        decimal capitalCuota = Math.Round(prestamo.Capital / prestamo.PlazoMeses, 2);
        decimal saldo = prestamo.Capital;
        CuotaPrestamo? primeraCuota = null;

        for (short n = 1; n <= prestamo.PlazoMeses; n++)
        {
            decimal capital = n == prestamo.PlazoMeses ? saldo : capitalCuota;
            decimal interes = Math.Round(saldo * prestamo.TasaInteresAnual / 100 / 12, 2);

            var cuota = new CuotaPrestamo
            {
                Prestamo = prestamo,
                NumeroCuota = n,
                FechaVencimiento = prestamo.FechaDesembolso!.Value.AddMonths(n),
                CapitalProgramado = capital,
                InteresProgramado = interes,
                Estado = "PENDIENTE"
            };

            contexto.CuotaPrestamos.Add(cuota);
            primeraCuota ??= cuota;
            saldo -= capital;
        }

        if (primeraCuota == null)
            return;

        decimal total = primeraCuota.CapitalProgramado + primeraCuota.InteresProgramado;
        var fechaPago = primeraCuota.FechaVencimiento.ToDateTime(new TimeOnly(10, 0));

        var operacion = new OperacionFinanciera
        {
            TipoOperacion = "PAGO_CUOTA",
            FechaHora = fechaPago,
            Monto = total,
            SocioEjecutor = prestamo.SocioSolicitante,
            Referencia = $"Pago {prestamo.NumeroPrestamo} cuota 1"
        };

        contexto.OperacionFinancieras.Add(operacion);
        contexto.PagoCuota.Add(new PagoCuotum
        {
            Cuota = primeraCuota,
            Operacion = operacion,
            MontoAplicado = total,
            FechaPago = fechaPago
        });
    }
}
