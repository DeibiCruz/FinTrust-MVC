
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinTrust.Web.Models;
using FinTrust.Web.Data;

public class PrestamosController : Controller
{
    private readonly FinTrustDbContext _context;

    public PrestamosController(FinTrustDbContext context)
    {
        _context = context;
    }

    // GET: Prestamos
    public async Task<IActionResult> Index()
    {
        var prestamos = await _context.Prestamos
            .Include(p => p.SocioSolicitante)
            .AsNoTracking()
            .ToListAsync();

        return View(prestamos);
    }

    // GET: Prestamos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var prestamo = await _context.Prestamos
            .Include(p => p.SocioSolicitante)
            .Include(p => p.CuentaDesembolso)
            .Include(p => p.EmpleadoAprobador)
            .FirstOrDefaultAsync(p => p.PrestamoId == id);

        if (prestamo == null)
        {
            return NotFound();
        }

        return View(prestamo);
    }

    // GET: Prestamos/Create
    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    // POST: Prestamos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("NumeroPrestamo,SocioSolicitanteId,CuentaDesembolsoId,EmpleadoAprobadorId,Capital,TasaInteresAnual,PlazoMeses,FechaSolicitud,FechaDesembolso,Estado")]
        Prestamo prestamo)
    {
        ModelState.Remove(nameof(Prestamo.SocioSolicitante));

        if (prestamo.Capital <= 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.Capital),
                "El capital debe ser mayor que cero.");
        }

        if (prestamo.TasaInteresAnual < 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.TasaInteresAnual),
                "La tasa de interes no puede ser negativa.");
        }

        if (prestamo.PlazoMeses <= 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.PlazoMeses),
                "El plazo debe ser mayor que cero.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Prestamos.Add(prestamo);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar el prestamo. Revisa los datos relacionados.");
            }
        }

        CargarListas();
        return View(prestamo);
    }

    // GET: Prestamos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var prestamo = await _context.Prestamos.FindAsync(id);

        if (prestamo == null)
        {
            return NotFound();
        }

        CargarListas();
        return View(prestamo);
    }

    // POST: Prestamos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("PrestamoId,NumeroPrestamo,SocioSolicitanteId,CuentaDesembolsoId,EmpleadoAprobadorId,Capital,TasaInteresAnual,PlazoMeses,FechaSolicitud,FechaDesembolso,Estado")]
        Prestamo prestamo)
    {
        if (id != prestamo.PrestamoId)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Prestamo.SocioSolicitante));

        if (prestamo.Capital <= 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.Capital),
                "El capital debe ser mayor que cero.");
        }

        if (prestamo.TasaInteresAnual < 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.TasaInteresAnual),
                "La tasa de interes no puede ser negativa.");
        }

        if (prestamo.PlazoMeses <= 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.PlazoMeses),
                "El plazo debe ser mayor que cero.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existente = await _context.Prestamos.FindAsync(id);

                if (existente == null)
                {
                    return NotFound();
                }

                existente.NumeroPrestamo = prestamo.NumeroPrestamo;
                existente.SocioSolicitanteId = prestamo.SocioSolicitanteId;
                existente.CuentaDesembolsoId = prestamo.CuentaDesembolsoId;
                existente.EmpleadoAprobadorId = prestamo.EmpleadoAprobadorId;
                existente.Capital = prestamo.Capital;
                existente.TasaInteresAnual = prestamo.TasaInteresAnual;
                existente.PlazoMeses = prestamo.PlazoMeses;
                existente.FechaSolicitud = prestamo.FechaSolicitud;
                existente.FechaDesembolso = prestamo.FechaDesembolso;
                existente.Estado = prestamo.Estado;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrestamoExists(id))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    "",
                    "Error al actualizar: " + ex.GetBaseException().Message);
            }
        }

        CargarListas();
        return View(prestamo);
    }

    // GET: Prestamos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var prestamo = await _context.Prestamos
            .FirstOrDefaultAsync(p => p.PrestamoId == id);

        if (prestamo == null)
        {
            return NotFound();
        }

        return View(prestamo);
    }

    // POST: Prestamos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var prestamo = await _context.Prestamos.FindAsync(id);

        if (prestamo == null)
        {
            return NotFound();
        }

        try
        {
            _context.Prestamos.Remove(prestamo);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "",
                "No se puede eliminar este prestamo porque tiene registros relacionados.");

            return View("Delete", prestamo);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PrestamoExists(int id)
    {
        return _context.Prestamos.Any(p => p.PrestamoId == id);
    }


    private void CargarListas()
    {
        ViewBag.SocioSolicitanteId = new SelectList(
            _context.Socios
                .AsNoTracking()
                .Select(s => new
                {
                    s.SocioId,
                    s.CodigoSocio
                })
                .ToList(),
            "SocioId",
            "CodigoSocio"
        );

        var cuentas = _context.Cuenta
            .AsNoTracking()
            .Select(c => new
            {
                c.CuentaId,
                c.NumeroCuenta
            })
            .ToList();

        ViewBag.CuentaDesembolsoId = new SelectList(
            cuentas,
            "CuentaId",
            "NumeroCuenta"
        );

        ViewBag.EmpleadoAprobadorId = new SelectList(
            _context.Empleados
                .AsNoTracking()
                .Select(e => new
                {
                    e.EmpleadoId,
                    e.Cargo
                })
                .ToList(),
            "EmpleadoId",
            "Cargo"
        );
    }

}
