using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class CuotaPrestamo
{
    public int CuotaId { get; set; }

    public int PrestamoId { get; set; }

    public short NumeroCuota { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal CapitalProgramado { get; set; }

    public decimal InteresProgramado { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<PagoCuotum> PagoCuota { get; set; } = new List<PagoCuotum>();

    public virtual Prestamo Prestamo { get; set; } = null!;
}
