using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class Cuentum
{
    public int CuentaId { get; set; }

    public string NumeroCuenta { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!;

    public string Moneda { get; set; } = null!;

    public decimal SaldoActual { get; set; }

    public DateOnly FechaApertura { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<CuentaSocio> CuentaSocios { get; set; } = new List<CuentaSocio>();

    public virtual ICollection<MovimientoCuentum> MovimientoCuenta { get; set; } = new List<MovimientoCuentum>();

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
