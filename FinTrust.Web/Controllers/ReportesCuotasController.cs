using FinTrust.Web.Data;
using FinTrust.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class ReportesCuotasController : Controller
{
    private readonly FinTrustDbContext _context;

    public ReportesCuotasController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? situacion,
        int? socioId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        situacion = string.IsNullOrWhiteSpace(situacion) ? "PENDIENTES" : situacion;

        var consulta = _context.CuotaPrestamos.AsNoTracking();

        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
        {
            ModelState.AddModelError(
                "",
                "La fecha inicial no puede ser mayor que la fecha final.");
        }
        else
        {
            if (fechaDesde.HasValue)
                consulta = consulta.Where(c => c.FechaVencimiento >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                consulta = consulta.Where(c => c.FechaVencimiento <= fechaHasta.Value);
        }

        if (socioId.HasValue)
            consulta = consulta.Where(c => c.Prestamo.SocioSolicitanteId == socioId.Value);

        consulta = situacion switch
        {
            "VENCIDAS" => consulta.Where(c =>
                c.FechaVencimiento < hoy &&
                c.CapitalProgramado + c.InteresProgramado >
                    (c.PagoCuota.Sum(p => (decimal?)p.MontoAplicado) ?? 0)),

            "POR_VENCER" => consulta.Where(c =>
                c.FechaVencimiento >= hoy &&
                c.CapitalProgramado + c.InteresProgramado >
                    (c.PagoCuota.Sum(p => (decimal?)p.MontoAplicado) ?? 0)),

            "PAGADAS" => consulta.Where(c =>
                c.CapitalProgramado + c.InteresProgramado <=
                    (c.PagoCuota.Sum(p => (decimal?)p.MontoAplicado) ?? 0)),

            _ => consulta.Where(c =>
                c.CapitalProgramado + c.InteresProgramado >
                    (c.PagoCuota.Sum(p => (decimal?)p.MontoAplicado) ?? 0))
        };

        var cuotas = new List<ReporteCuotasFilaViewModel>();

        if (ModelState.IsValid)
        {
            cuotas = await consulta
                .OrderBy(c => c.FechaVencimiento)
                .ThenBy(c => c.Prestamo.NumeroPrestamo)
                .ThenBy(c => c.NumeroCuota)
                .Select(c => new ReporteCuotasFilaViewModel
                {
                    PrestamoId = c.PrestamoId,
                    NumeroPrestamo = c.Prestamo.NumeroPrestamo,
                    CodigoSocio = c.Prestamo.SocioSolicitante.CodigoSocio,
                    NombreSocio = c.Prestamo.SocioSolicitante.Persona.Nombres + " " +
                                  c.Prestamo.SocioSolicitante.Persona.Apellidos,
                    NumeroCuota = c.NumeroCuota,
                    FechaVencimiento = c.FechaVencimiento,
                    CapitalProgramado = c.CapitalProgramado,
                    InteresProgramado = c.InteresProgramado,
                    TotalPagado = c.PagoCuota.Sum(p => (decimal?)p.MontoAplicado) ?? 0
                })
                .ToListAsync();

            foreach (var cuota in cuotas)
            {
                cuota.DiasAtraso = Math.Max(0, hoy.DayNumber - cuota.FechaVencimiento.DayNumber);
            }
        }

        var situaciones = new[]
        {
            new { Valor = "PENDIENTES", Texto = "Pendientes (vencidas y por vencer)" },
            new { Valor = "VENCIDAS", Texto = "Vencidas" },
            new { Valor = "POR_VENCER", Texto = "Por vencer" },
            new { Valor = "PAGADAS", Texto = "Pagadas" }
        };

        var socios = await _context.Socios
            .AsNoTracking()
            .Where(s => s.Prestamos.Any(p => p.CuotaPrestamos.Any()))
            .OrderBy(s => s.CodigoSocio)
            .Select(s => new
            {
                s.SocioId,
                Texto = s.CodigoSocio + " - " + s.Persona.Nombres + " " + s.Persona.Apellidos
            })
            .ToListAsync();

        var modelo = new ReporteCuotasViewModel
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Situacion = situacion,
            SocioId = socioId,
            Situaciones = new SelectList(situaciones, "Valor", "Texto", situacion),
            Socios = new SelectList(socios, "SocioId", "Texto", socioId),
            FechaReporte = hoy,
            Cuotas = cuotas
        };

        return View(modelo);
    }
}
