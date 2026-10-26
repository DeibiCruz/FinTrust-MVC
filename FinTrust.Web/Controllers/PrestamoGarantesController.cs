
using FinTrust.Web.Data;
using FinTrust.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class PrestamoGarantesController : Controller
{
    private readonly FinTrustDbContext _context;

    public PrestamoGarantesController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var garantes = await _context.PrestamoGarantes
            .Include(g => g.Prestamo)
            .Include(g => g.PersonaGarante)
            .AsNoTracking()
            .ToListAsync();

        return View(garantes);
    }

    public async Task<IActionResult> Details(int prestamoId, int personaGaranteId)
    {
        var garante = await _context.PrestamoGarantes
            .Include(g => g.Prestamo)
            .Include(g => g.PersonaGarante)
            .AsNoTracking()
            .FirstOrDefaultAsync(g =>
                g.PrestamoId == prestamoId &&
                g.PersonaGaranteId == personaGaranteId);

        if (garante == null)
            return NotFound();

        return View(garante);
    }

    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("PrestamoId,PersonaGaranteId,TipoResponsabilidad,MontoRespaldado,FechaAceptacion")]
        PrestamoGarante garante)
    {
        ModelState.Remove(nameof(PrestamoGarante.Prestamo));
        ModelState.Remove(nameof(PrestamoGarante.PersonaGarante));

        ValidarDatos(garante);

        if (!TieneError(nameof(PrestamoGarante.PrestamoId)) &&
            !await _context.Prestamos.AnyAsync(p => p.PrestamoId == garante.PrestamoId))
        {
            ModelState.AddModelError(
                nameof(PrestamoGarante.PrestamoId),
                "El préstamo seleccionado no existe.");
        }

        if (!TieneError(nameof(PrestamoGarante.PersonaGaranteId)) &&
            !await _context.Personas.AnyAsync(p => p.PersonaId == garante.PersonaGaranteId))
        {
            ModelState.AddModelError(
                nameof(PrestamoGarante.PersonaGaranteId),
                "La persona seleccionada no existe.");
        }

        bool existe = await _context.PrestamoGarantes
            .AnyAsync(g =>
                g.PrestamoId == garante.PrestamoId &&
                g.PersonaGaranteId == garante.PersonaGaranteId);

        if (existe)
        {
            ModelState.AddModelError(
                "",
                "Esta persona ya es garante de este préstamo.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.PrestamoGarantes.Add(garante);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar el garante. Revise los datos ingresados.");
            }
        }

        CargarListas();
        return View(garante);
    }

    public async Task<IActionResult> Edit(int prestamoId, int personaGaranteId)
    {
        var garante = await _context.PrestamoGarantes.FindAsync(
            prestamoId, personaGaranteId);

        if (garante == null)
            return NotFound();

        CargarListas();
        return View(garante);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int prestamoId,
        int personaGaranteId,
        [Bind("PrestamoId,PersonaGaranteId,TipoResponsabilidad,MontoRespaldado,FechaAceptacion")]
        PrestamoGarante garante)
    {
        if (prestamoId != garante.PrestamoId ||
            personaGaranteId != garante.PersonaGaranteId)
        {
            return BadRequest();
        }

        ModelState.Remove(nameof(PrestamoGarante.Prestamo));
        ModelState.Remove(nameof(PrestamoGarante.PersonaGarante));

        ValidarDatos(garante);

        if (ModelState.IsValid)
        {
            var registro = await _context.PrestamoGarantes.FindAsync(
                prestamoId, personaGaranteId);

            if (registro == null)
                return NotFound();

            registro.TipoResponsabilidad = garante.TipoResponsabilidad;
            registro.MontoRespaldado = garante.MontoRespaldado;
            registro.FechaAceptacion = garante.FechaAceptacion;

            try
            {
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar el garante. Revise los datos ingresados.");
            }
        }

        CargarListas();
        return View(garante);
    }

    public async Task<IActionResult> Delete(int prestamoId, int personaGaranteId)
    {
        var garante = await _context.PrestamoGarantes
            .Include(g => g.Prestamo)
            .Include(g => g.PersonaGarante)
            .AsNoTracking()
            .FirstOrDefaultAsync(g =>
                g.PrestamoId == prestamoId &&
                g.PersonaGaranteId == personaGaranteId);

        if (garante == null)
            return NotFound();

        return View(garante);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int prestamoId, int personaGaranteId)
    {
        var garante = await _context.PrestamoGarantes.FindAsync(
            prestamoId, personaGaranteId);

        if (garante == null)
            return NotFound();

        _context.PrestamoGarantes.Remove(garante);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void ValidarDatos(PrestamoGarante garante)
    {
        ReemplazarError(nameof(PrestamoGarante.PrestamoId), "Seleccione un préstamo.");
        ReemplazarError(nameof(PrestamoGarante.PersonaGaranteId), "Seleccione una persona.");
        ReemplazarError(nameof(PrestamoGarante.MontoRespaldado), "Ingrese un monto válido.");
        ReemplazarError(nameof(PrestamoGarante.FechaAceptacion), "Ingrese una fecha de aceptación válida.");
        ModelState.Remove(nameof(PrestamoGarante.TipoResponsabilidad));

        if (garante.TipoResponsabilidad != "SOLIDARIA" &&
            garante.TipoResponsabilidad != "SUBSIDIARIA")
        {
            ModelState.AddModelError(
                nameof(PrestamoGarante.TipoResponsabilidad),
                "Seleccione un tipo de responsabilidad válido.");
        }

        if (garante.MontoRespaldado.HasValue)
        {
            decimal monto = garante.MontoRespaldado.Value;

            if (monto <= 0)
            {
                ModelState.AddModelError(
                    nameof(PrestamoGarante.MontoRespaldado),
                    "El monto respaldado debe ser mayor a cero.");
            }
            else if (monto > 9999999999999999.99m ||
                     decimal.Round(monto, 2) != monto)
            {
                ModelState.AddModelError(
                    nameof(PrestamoGarante.MontoRespaldado),
                    "El monto respaldado debe tener como máximo 2 decimales y un valor válido.");
            }
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
            _context.Prestamos.AsNoTracking().ToList(),
            "PrestamoId",
            "NumeroPrestamo");

        ViewBag.PersonaGaranteId = new SelectList(
            _context.Personas.AsNoTracking().ToList(),
            "PersonaId",
            "Nombres");
    }
}
