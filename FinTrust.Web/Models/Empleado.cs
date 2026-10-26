using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class Empleado
{
    public int EmpleadoId { get; set; }

    public int PersonaId { get; set; }

    public string Cargo { get; set; } = null!;

    public DateOnly FechaIngreso { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Persona Persona { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
