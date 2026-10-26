using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class PagoCuotum
{
    public long PagoCuotaId { get; set; }

    public int CuotaId { get; set; }

    public long OperacionId { get; set; }

    public decimal MontoAplicado { get; set; }

    public DateTime FechaPago { get; set; }

    public virtual CuotaPrestamo Cuota { get; set; } = null!;

    public virtual OperacionFinanciera Operacion { get; set; } = null!;
}
