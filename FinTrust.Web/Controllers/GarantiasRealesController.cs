
using FinTrust.Web.Data;
using FinTrust.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class GarantiasRealesController : Controller
{
    private readonly FinTrustDbContext _context;

    public GarantiasRealesController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var garantias = await _context.GarantiaReals
            .Include(g => g.Prestamo)
            .Include(g => g.PersonaPropietaria)
            .AsNoTracking()
            .ToListAsync();

        return View(garantias);
    }

    public async Task<IActionResult> Details(int id)
    {
        var garantia = await _context.GarantiaReals
            .Include(g => g.Prestamo)
            .Include(g => g.PersonaPropietaria)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GarantiaId == id);

        if (garantia == null)
            return NotFound();

        return View(garantia);
    }

    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("PrestamoId,PersonaPropietariaId,TipoGarantia,Descripcion,ValorTasado,FechaValuacion")]
        GarantiaReal garantia)
    {
        ModelState.Remove(nameof(GarantiaReal.Prestamo));
        ModelState.Remove(nameof(GarantiaReal.PersonaPropietaria));

        await ValidarGarantia(garantia);

        if (ModelState.IsValid)
        {
            try
            {
                _context.GarantiaReals.Add(garantia);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar la garantía. Revise los datos ingresados.");
            }
        }

        CargarListas();
        return View(garantia);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var garantia = await _context.GarantiaReals.FindAsync(id);

        if (garantia == null)
            return NotFound();

        CargarListas();
        return View(garantia);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("GarantiaId,PrestamoId,PersonaPropietariaId,TipoGarantia,Descripcion,ValorTasado,FechaValuacion")]
        GarantiaReal garantia)
    {
        if (id != garantia.GarantiaId)
            return BadRequest();

        ModelState.Remove(nameof(GarantiaReal.Prestamo));
        ModelState.Remove(nameof(GarantiaReal.PersonaPropietaria));

        await ValidarGarantia(garantia);

        if (ModelState.IsValid)
        {
            var registro = await _context.GarantiaReals.FindAsync(id);

            if (registro == null)
                return NotFound();

            registro.PrestamoId = garantia.PrestamoId;
            registro.PersonaPropietariaId = garantia.PersonaPropietariaId;
            registro.TipoGarantia = garantia.TipoGarantia;
            registro.Descripcion = garantia.Descripcion;
            registro.ValorTasado = garantia.ValorTasado;
            registro.FechaValuacion = garantia.FechaValuacion;

            try
            {
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar la garantía. Revise los datos ingresados.");
            }
        }

        CargarListas();
        return View(garantia);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var garantia = await _context.GarantiaReals
            .Include(g => g.Prestamo)
            .Include(g => g.PersonaPropietaria)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GarantiaId == id);

        if (garantia == null)
            return NotFound();

        return View(garantia);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var garantia = await _context.GarantiaReals.FindAsync(id);

        if (garantia == null)
            return NotFound();

        _context.GarantiaReals.Remove(garantia);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarGarantia(GarantiaReal garantia)
    {
        ReemplazarError(nameof(GarantiaReal.PrestamoId), "Seleccione un préstamo.");
        ReemplazarError(nameof(GarantiaReal.PersonaPropietariaId), "Seleccione una persona propietaria.");
        ReemplazarError(nameof(GarantiaReal.ValorTasado), "Ingrese un valor tasado válido.");
        ReemplazarError(nameof(GarantiaReal.FechaValuacion), "Ingrese una fecha de valuación válida.");
        ModelState.Remove(nameof(GarantiaReal.TipoGarantia));
        ModelState.Remove(nameof(GarantiaReal.Descripcion));

        if (string.IsNullOrWhiteSpace(garantia.TipoGarantia))
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.TipoGarantia),
                "El tipo de garantía es obligatorio.");
        }
        else if (garantia.TipoGarantia.Trim().Length > 20)
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.TipoGarantia),
                "El tipo de garantía admite hasta 20 caracteres.");
        }
        else
        {
            garantia.TipoGarantia = garantia.TipoGarantia.Trim();
        }

        if (string.IsNullOrWhiteSpace(garantia.Descripcion))
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.Descripcion),
                "La descripción es obligatoria.");
        }
        else if (garantia.Descripcion.Trim().Length > 250)
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.Descripcion),
                "La descripción admite hasta 250 caracteres.");
        }
        else
        {
            garantia.Descripcion = garantia.Descripcion.Trim();
        }

        if (!TieneError(nameof(GarantiaReal.ValorTasado)) && garantia.ValorTasado <= 0)
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.ValorTasado),
                "El valor tasado debe ser mayor a cero.");
        }
        else if (garantia.ValorTasado > 9999999999999999.99m ||
                 decimal.Round(garantia.ValorTasado, 2) != garantia.ValorTasado)
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.ValorTasado),
                "El valor tasado debe tener como máximo 2 decimales y un valor válido.");
        }

        if (!TieneError(nameof(GarantiaReal.PrestamoId)) &&
            !await _context.Prestamos.AnyAsync(p => p.PrestamoId == garantia.PrestamoId))
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.PrestamoId),
                "El préstamo seleccionado no existe.");
        }

        if (!TieneError(nameof(GarantiaReal.PersonaPropietariaId)) &&
            !await _context.Personas.AnyAsync(p => p.PersonaId == garantia.PersonaPropietariaId))
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.PersonaPropietariaId),
                "La persona seleccionada no existe.");
        }
    }

    private bool TieneError(string campo)
    {
        return ModelState[campo]?.Errors.Count > 0;
    }

    private void ReemplazarError(string campo, string mensaje)
    {
        if (TieneError(campo))
        {
            ModelState[campo]!.Errors.Clear();
            ModelState.AddModelError(campo, mensaje);
        }
    }

    private void CargarListas()
    {
        ViewBag.PrestamoId = new SelectList(
            _context.Prestamos
                .AsNoTracking()
                .Select(p => new
                {
                    p.PrestamoId,
                    p.NumeroPrestamo
                })
                .ToList(),
            "PrestamoId",
            "NumeroPrestamo");

        ViewBag.PersonaPropietariaId = new SelectList(
            _context.Personas
                .AsNoTracking()
                .Select(p => new
                {
                    p.PersonaId,
                    NombreCompleto = p.Nombres + " " + p.Apellidos
                })
                .ToList(),
            "PersonaId",
            "NombreCompleto");
    }
}
