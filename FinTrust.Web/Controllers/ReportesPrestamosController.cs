using FinTrust.Web.Data;
using FinTrust.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class ReportesPrestamosController : Controller
{
    private readonly FinTrustDbContext _context;

    public ReportesPrestamosController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? numeroPrestamo, string? estado, int? socioId)
    {
        var consulta = _context.Prestamos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(numeroPrestamo))
        {
            string texto = numeroPrestamo.Trim();
            consulta = consulta.Where(p => p.NumeroPrestamo.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            consulta = consulta.Where(p => p.Estado == estado);
        }

        if (socioId.HasValue)
        {
            consulta = consulta.Where(p => p.SocioSolicitanteId == socioId.Value);
        }

        var prestamos = await consulta
            .OrderBy(p => p.NumeroPrestamo)
            .Select(p => new ReportePrestamoFilaViewModel
            {
                PrestamoId = p.PrestamoId,
                NumeroPrestamo = p.NumeroPrestamo,
                CodigoSocio = p.SocioSolicitante.CodigoSocio,
                NombreSocio = p.SocioSolicitante.Persona.Nombres + " " +
                              p.SocioSolicitante.Persona.Apellidos,
                Capital = p.Capital,
                TasaInteresAnual = p.TasaInteresAnual,
                PlazoMeses = p.PlazoMeses,
                FechaSolicitud = p.FechaSolicitud,
                Estado = p.Estado,
                CantidadCuotas = p.CuotaPrestamos.Count(),
                TotalProgramado = p.CuotaPrestamos
                    .Sum(c => (decimal?)(c.CapitalProgramado + c.InteresProgramado)) ?? 0,
                TotalPagado = p.CuotaPrestamos
                    .SelectMany(c => c.PagoCuota)
                    .Sum(pg => (decimal?)pg.MontoAplicado) ?? 0
            })
            .ToListAsync();

        var estados = await _context.Prestamos
            .AsNoTracking()
            .Select(p => p.Estado)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();

        var socios = await _context.Socios
            .AsNoTracking()
            .Where(s => s.Prestamos.Any())
            .OrderBy(s => s.CodigoSocio)
            .Select(s => new
            {
                s.SocioId,
                Texto = s.CodigoSocio + " - " + s.Persona.Nombres + " " + s.Persona.Apellidos
            })
            .ToListAsync();

        var modelo = new ReportePrestamoViewModel
        {
            NumeroPrestamo = numeroPrestamo,
            Estado = estado,
            SocioId = socioId,
            Estados = new SelectList(estados, estado),
            Socios = new SelectList(socios, "SocioId", "Texto", socioId),
            Prestamos = prestamos
        };

        return View(modelo);
    }

    public async Task<IActionResult> Details(int id)
    {
        var detalle = await _context.Prestamos
            .AsNoTracking()
            .Where(p => p.PrestamoId == id)
            .Select(p => new ReportePrestamoDetalleViewModel
            {
                PrestamoId = p.PrestamoId,
                NumeroPrestamo = p.NumeroPrestamo,
                CodigoSocio = p.SocioSolicitante.CodigoSocio,
                NombreSocio = p.SocioSolicitante.Persona.Nombres + " " +
                              p.SocioSolicitante.Persona.Apellidos,
                Capital = p.Capital,
                Estado = p.Estado
            })
            .FirstOrDefaultAsync();

        if (detalle == null)
            return NotFound();

        detalle.Cuotas = await _context.CuotaPrestamos
            .AsNoTracking()
            .Where(c => c.PrestamoId == id)
            .OrderBy(c => c.NumeroCuota)
            .Select(c => new ReporteCuotaFilaViewModel
            {
                NumeroCuota = c.NumeroCuota,
                FechaVencimiento = c.FechaVencimiento,
                CapitalProgramado = c.CapitalProgramado,
                InteresProgramado = c.InteresProgramado,
                TotalPagado = c.PagoCuota.Sum(pg => (decimal?)pg.MontoAplicado) ?? 0,
                Estado = c.Estado
            })
            .ToListAsync();

        return View(detalle);
    }
}
