using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinTrust.Web.ViewModels;

public class ReporteCuotasViewModel
{
    public DateOnly? FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    public string Situacion { get; set; } = "PENDIENTES";

    public int? SocioId { get; set; }

    public SelectList Situaciones { get; set; } = null!;

    public SelectList Socios { get; set; } = null!;

    public DateOnly FechaReporte { get; set; }

    public List<ReporteCuotasFilaViewModel> Cuotas { get; set; } = new();

    public int CantidadCuotas => Cuotas.Count;

    public decimal TotalProgramado => Cuotas.Sum(c => c.TotalCuota);

    public decimal TotalPagado => Cuotas.Sum(c => c.TotalPagado);

    public decimal SaldoPendiente => Cuotas.Sum(c => c.SaldoPendiente);

    public int CantidadVencidas => Cuotas.Count(c => c.EstaVencida);

    public decimal SaldoVencido => Cuotas.Where(c => c.EstaVencida).Sum(c => c.SaldoPendiente);
}

public class ReporteCuotasFilaViewModel
{
    public int PrestamoId { get; set; }

    public string NumeroPrestamo { get; set; } = "";

    public string CodigoSocio { get; set; } = "";

    public string NombreSocio { get; set; } = "";

    public short NumeroCuota { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal CapitalProgramado { get; set; }

    public decimal InteresProgramado { get; set; }

    public decimal TotalPagado { get; set; }

    public int DiasAtraso { get; set; }

    public decimal TotalCuota => CapitalProgramado + InteresProgramado;

    public decimal SaldoPendiente => TotalCuota - TotalPagado;

    public bool EstaVencida => SaldoPendiente > 0 && DiasAtraso > 0;

    public string Situacion =>
        SaldoPendiente <= 0 ? "Pagada" :
        DiasAtraso > 0 ? "Vencida" :
        "Por vencer";
}
