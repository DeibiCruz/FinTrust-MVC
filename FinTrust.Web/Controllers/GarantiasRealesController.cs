
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
    public async Task<IActionResult> Create(GarantiaReal garantia)
    {
        ModelState.Remove(nameof(GarantiaReal.Prestamo));
        ModelState.Remove(nameof(GarantiaReal.PersonaPropietaria));

        if (garantia.ValorTasado <= 0)
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.ValorTasado),
                "El valor tasado debe ser mayor a cero.");
        }

        if (ModelState.IsValid)
        {
            _context.GarantiaReals.Add(garantia);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Edit(int id, GarantiaReal garantia)
    {
        if (id != garantia.GarantiaId)
            return BadRequest();

        ModelState.Remove(nameof(GarantiaReal.Prestamo));
        ModelState.Remove(nameof(GarantiaReal.PersonaPropietaria));

        if (garantia.ValorTasado <= 0)
        {
            ModelState.AddModelError(
                nameof(GarantiaReal.ValorTasado),
                "El valor tasado debe ser mayor a cero.");
        }

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

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
