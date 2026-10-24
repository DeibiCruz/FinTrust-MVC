using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class CuentaSocio
{
    public int CuentaId { get; set; }

    public int SocioId { get; set; }

    public string Rol { get; set; } = null!;

    public bool PuedeOperar { get; set; }

    public DateOnly FechaVinculacion { get; set; }

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual Socio Socio { get; set; } = null!;
}
