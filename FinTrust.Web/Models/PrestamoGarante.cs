using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class PrestamoGarante
{
    public int PrestamoId { get; set; }

    public int PersonaGaranteId { get; set; }

    public string TipoResponsabilidad { get; set; } = null!;

    public decimal? MontoRespaldado { get; set; }

    public DateOnly FechaAceptacion { get; set; }

    public virtual Persona PersonaGarante { get; set; } = null!;

    public virtual Prestamo Prestamo { get; set; } = null!;
}
