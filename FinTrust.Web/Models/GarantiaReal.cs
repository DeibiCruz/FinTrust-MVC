using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class GarantiaReal
{
    public int GarantiaId { get; set; }

    public int PrestamoId { get; set; }

    public int PersonaPropietariaId { get; set; }

    public string TipoGarantia { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal ValorTasado { get; set; }

    public DateOnly? FechaValuacion { get; set; }

    public virtual Persona PersonaPropietaria { get; set; } = null!;

    public virtual Prestamo Prestamo { get; set; } = null!;
}
