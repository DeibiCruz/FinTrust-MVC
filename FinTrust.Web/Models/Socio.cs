using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class Socio
{
    public int SocioId { get; set; }

    public int PersonaId { get; set; }

    public string CodigoSocio { get; set; } = null!;

    public DateOnly FechaIngreso { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<CuentaSocio> CuentaSocios { get; set; } = new List<CuentaSocio>();

    public virtual ICollection<OperacionFinanciera> OperacionFinancieras { get; set; } = new List<OperacionFinanciera>();

    public virtual Persona Persona { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
