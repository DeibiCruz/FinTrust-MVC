using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class OperacionFinanciera
{
    public long OperacionId { get; set; }

    public string TipoOperacion { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public decimal Monto { get; set; }

    public int? SocioEjecutorId { get; set; }

    public string? Referencia { get; set; }

    public virtual ICollection<MovimientoCuentum> MovimientoCuenta { get; set; } = new List<MovimientoCuentum>();

    public virtual ICollection<PagoCuotum> PagoCuota { get; set; } = new List<PagoCuotum>();

    public virtual Socio? SocioEjecutor { get; set; }
}
