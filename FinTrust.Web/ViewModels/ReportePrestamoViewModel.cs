using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinTrust.Web.ViewModels;

public class ReportePrestamoViewModel
{
    public string? NumeroPrestamo { get; set; }

    public string? Estado { get; set; }

    public int? SocioId { get; set; }

    public SelectList Estados { get; set; } = null!;

    public SelectList Socios { get; set; } = null!;

    public List<ReportePrestamoFilaViewModel> Prestamos { get; set; } = new();

    public int TotalPrestamos => Prestamos.Count;

    public decimal CapitalTotal => Prestamos.Sum(p => p.Capital);

    public decimal TotalProgramado => Prestamos.Sum(p => p.TotalProgramado);

    public decimal TotalPagado => Prestamos.Sum(p => p.TotalPagado);

    public decimal SaldoPendiente => TotalProgramado - TotalPagado;
}

public class ReportePrestamoFilaViewModel
{
    public int PrestamoId { get; set; }

    public string NumeroPrestamo { get; set; } = "";

    public string CodigoSocio { get; set; } = "";

    public string NombreSocio { get; set; } = "";

    public decimal Capital { get; set; }

    public decimal TasaInteresAnual { get; set; }

    public short PlazoMeses { get; set; }

    public DateOnly FechaSolicitud { get; set; }

    public string Estado { get; set; } = "";

    public int CantidadCuotas { get; set; }

    public decimal TotalProgramado { get; set; }

    public decimal TotalPagado { get; set; }

    public decimal SaldoPendiente => TotalProgramado - TotalPagado;
}
