using System;
using System.Collections.Generic;

namespace FinTrust.Web.Models;

public partial class Prestamo
{
    public int PrestamoId { get; set; }

    public string NumeroPrestamo { get; set; } = null!;

    public int SocioSolicitanteId { get; set; }

    public int? CuentaDesembolsoId { get; set; }

    public int? EmpleadoAprobadorId { get; set; }

    public decimal Capital { get; set; }

    public decimal TasaInteresAnual { get; set; }

    public short PlazoMeses { get; set; }

    public DateOnly FechaSolicitud { get; set; }

    public DateOnly? FechaDesembolso { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cuentum? CuentaDesembolso { get; set; }

    public virtual ICollection<CuotaPrestamo> CuotaPrestamos { get; set; } = new List<CuotaPrestamo>();

    public virtual Empleado? EmpleadoAprobador { get; set; }

    public virtual ICollection<GarantiaReal> GarantiaReals { get; set; } = new List<GarantiaReal>();

    public virtual ICollection<PrestamoGarante> PrestamoGarantes { get; set; } = new List<PrestamoGarante>();

    public virtual Socio SocioSolicitante { get; set; } = null!;
}
