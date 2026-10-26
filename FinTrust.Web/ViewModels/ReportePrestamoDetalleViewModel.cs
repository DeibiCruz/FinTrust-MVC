namespace FinTrust.Web.ViewModels;

public class ReportePrestamoDetalleViewModel
{
    public int PrestamoId { get; set; }

    public string NumeroPrestamo { get; set; } = "";

    public string CodigoSocio { get; set; } = "";

    public string NombreSocio { get; set; } = "";

    public decimal Capital { get; set; }

    public string Estado { get; set; } = "";

    public List<ReporteCuotaFilaViewModel> Cuotas { get; set; } = new();

    public decimal TotalProgramado => Cuotas.Sum(c => c.TotalCuota);

    public decimal TotalPagado => Cuotas.Sum(c => c.TotalPagado);

    public decimal SaldoPendiente => TotalProgramado - TotalPagado;
}

public class ReporteCuotaFilaViewModel
{
    public short NumeroCuota { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal CapitalProgramado { get; set; }

    public decimal InteresProgramado { get; set; }

    public decimal TotalCuota => CapitalProgramado + InteresProgramado;

    public decimal TotalPagado { get; set; }

    public decimal SaldoPendiente => TotalCuota - TotalPagado;

    public string Estado { get; set; } = "";
}
