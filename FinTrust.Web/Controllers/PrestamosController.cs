
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinTrust.Web.Models;
using FinTrust.Web.Data;

public class PrestamosController : Controller
{
    private static readonly string[] EstadosPermitidos =
    {
        "SOLICITADO", "APROBADO", "RECHAZADO", "DESEMBOLSADO", "CANCELADO"
    };

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

        await ValidarPrestamo(prestamo, null);

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

        await ValidarPrestamo(prestamo, id);

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
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar el prestamo. Revisa los datos relacionados.");
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

        bool tieneRelacionados =
            await _context.CuotaPrestamos.AnyAsync(c => c.PrestamoId == id) ||
            await _context.PrestamoGarantes.AnyAsync(g => g.PrestamoId == id) ||
            await _context.GarantiaReals.AnyAsync(g => g.PrestamoId == id);

        if (tieneRelacionados)
        {
            ModelState.AddModelError(
                "",
                "No se puede eliminar este prestamo porque tiene cuotas, garantes o garantias registradas.");

            return View("Delete", prestamo);
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

    private async Task ValidarPrestamo(Prestamo prestamo, int? idActual)
    {
        ReemplazarError(nameof(Prestamo.SocioSolicitanteId), "Seleccione un socio solicitante.");
        ReemplazarError(nameof(Prestamo.Capital), "Ingrese un capital valido.");
        ReemplazarError(nameof(Prestamo.TasaInteresAnual), "Ingrese una tasa de interes valida.");
        ReemplazarError(nameof(Prestamo.PlazoMeses), "Ingrese un plazo valido.");
        ReemplazarError(nameof(Prestamo.FechaSolicitud), "Ingrese una fecha de solicitud valida.");
        ModelState.Remove(nameof(Prestamo.NumeroPrestamo));
        ModelState.Remove(nameof(Prestamo.Estado));

        if (string.IsNullOrWhiteSpace(prestamo.NumeroPrestamo))
        {
            ModelState.AddModelError(
                nameof(Prestamo.NumeroPrestamo),
                "El numero de prestamo es obligatorio.");
        }
        else
        {
            prestamo.NumeroPrestamo = prestamo.NumeroPrestamo.Trim();

            if (prestamo.NumeroPrestamo.Length > 30)
            {
                ModelState.AddModelError(
                    nameof(Prestamo.NumeroPrestamo),
                    "El numero de prestamo admite hasta 30 caracteres.");
            }
            else
            {
                bool repetido = await _context.Prestamos.AnyAsync(p =>
                    p.NumeroPrestamo == prestamo.NumeroPrestamo &&
                    (idActual == null || p.PrestamoId != idActual));

                if (repetido)
                {
                    ModelState.AddModelError(
                        nameof(Prestamo.NumeroPrestamo),
                        "Ya existe un prestamo con este numero.");
                }
            }
        }

        if (!TieneError(nameof(Prestamo.Capital)) && prestamo.Capital <= 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.Capital),
                "El capital debe ser mayor que cero.");
        }
        else if (prestamo.Capital > 9999999999999999.99m ||
                 decimal.Round(prestamo.Capital, 2) != prestamo.Capital)
        {
            ModelState.AddModelError(
                nameof(Prestamo.Capital),
                "El capital debe tener como maximo 2 decimales y un valor valido.");
        }

        if (!TieneError(nameof(Prestamo.TasaInteresAnual)) && prestamo.TasaInteresAnual < 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.TasaInteresAnual),
                "La tasa de interes no puede ser negativa.");
        }
        else if (prestamo.TasaInteresAnual > 100)
        {
            ModelState.AddModelError(
                nameof(Prestamo.TasaInteresAnual),
                "La tasa de interes no puede ser mayor a 100.");
        }
        else if (decimal.Round(prestamo.TasaInteresAnual, 4) != prestamo.TasaInteresAnual)
        {
            ModelState.AddModelError(
                nameof(Prestamo.TasaInteresAnual),
                "La tasa de interes admite hasta 4 decimales.");
        }

        if (!TieneError(nameof(Prestamo.PlazoMeses)) && prestamo.PlazoMeses <= 0)
        {
            ModelState.AddModelError(
                nameof(Prestamo.PlazoMeses),
                "El plazo debe ser mayor que cero.");
        }

        if (!EstadosPermitidos.Contains(prestamo.Estado))
        {
            ModelState.AddModelError(
                nameof(Prestamo.Estado),
                "Seleccione un estado valido.");
        }

        if (!TieneError(nameof(Prestamo.SocioSolicitanteId)) &&
            !await _context.Socios.AnyAsync(s => s.SocioId == prestamo.SocioSolicitanteId))
        {
            ModelState.AddModelError(
                nameof(Prestamo.SocioSolicitanteId),
                "El socio seleccionado no existe.");
        }

        if (prestamo.CuentaDesembolsoId.HasValue &&
            !await _context.Cuenta.AnyAsync(c => c.CuentaId == prestamo.CuentaDesembolsoId))
        {
            ModelState.AddModelError(
                nameof(Prestamo.CuentaDesembolsoId),
                "La cuenta seleccionada no existe.");
        }

        if (prestamo.EmpleadoAprobadorId.HasValue &&
            !await _context.Empleados.AnyAsync(e => e.EmpleadoId == prestamo.EmpleadoAprobadorId))
        {
            ModelState.AddModelError(
                nameof(Prestamo.EmpleadoAprobadorId),
                "El empleado seleccionado no existe.");
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
