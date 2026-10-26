using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTrust.Web.Models;

public partial class Persona
{
    public int PersonaId { get; set; }

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(25, ErrorMessage = "El documento admite hasta 25 caracteres.")]
    [Display(Name = "Documento de identidad")]
    public string DocumentoIdentidad { get; set; } = null!;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80, ErrorMessage = "Los nombres admiten hasta 80 caracteres.")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos admiten hasta 100 caracteres.")]
    public string Apellidos { get; set; } = null!;

    [StringLength(25, ErrorMessage = "El teléfono admite hasta 25 caracteres.")]
    [Phone(ErrorMessage = "Ingresa un teléfono válido.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(150, ErrorMessage = "El correo admite hasta 150 caracteres.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    public string? Correo { get; set; }

    public virtual Empleado? Empleado { get; set; }

    public virtual ICollection<GarantiaReal> GarantiaReals { get; set; } = new List<GarantiaReal>();

    public virtual ICollection<PrestamoGarante> PrestamoGarantes { get; set; } = new List<PrestamoGarante>();

    public virtual Socio? Socio { get; set; }
}
