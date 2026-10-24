using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class Persona
{
    public int PersonaId { get; set; }

    public string DocumentoIdentidad { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public virtual Empleado? Empleado { get; set; }

    public virtual ICollection<GarantiaReal> GarantiaReals { get; set; } = new List<GarantiaReal>();

    public virtual ICollection<PrestamoGarante> PrestamoGarantes { get; set; } = new List<PrestamoGarante>();

    public virtual Socio? Socio { get; set; }
}
