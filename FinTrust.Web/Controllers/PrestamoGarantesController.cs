
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
    public async Task<IActionResult> Create(PrestamoGarante garante)
    {
        ModelState.Remove(nameof(PrestamoGarante.Prestamo));
        ModelState.Remove(nameof(PrestamoGarante.PersonaGarante));

        if (garante.MontoRespaldado.HasValue &&
            garante.MontoRespaldado.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(PrestamoGarante.MontoRespaldado),
                "El monto respaldado debe ser mayor a cero.");
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
            _context.PrestamoGarantes.Add(garante);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
        PrestamoGarante garante)
    {
        if (prestamoId != garante.PrestamoId ||
            personaGaranteId != garante.PersonaGaranteId)
        {
            return BadRequest();
        }

        ModelState.Remove(nameof(PrestamoGarante.Prestamo));
        ModelState.Remove(nameof(PrestamoGarante.PersonaGarante));

        if (garante.MontoRespaldado.HasValue &&
            garante.MontoRespaldado.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(PrestamoGarante.MontoRespaldado),
                "El monto respaldado debe ser mayor a cero.");
        }

        if (ModelState.IsValid)
        {
            var registro = await _context.PrestamoGarantes.FindAsync(
                prestamoId, personaGaranteId);

            if (registro == null)
                return NotFound();

            registro.TipoResponsabilidad = garante.TipoResponsabilidad;
            registro.MontoRespaldado = garante.MontoRespaldado;
            registro.FechaAceptacion = garante.FechaAceptacion;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
