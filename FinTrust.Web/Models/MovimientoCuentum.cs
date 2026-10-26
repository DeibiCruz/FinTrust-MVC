using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class MovimientoCuentum
{
    public long MovimientoId { get; set; }

    public long OperacionId { get; set; }

    public int CuentaId { get; set; }

    public string Sentido { get; set; } = null!;

    public decimal Monto { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual OperacionFinanciera Operacion { get; set; } = null!;
}
