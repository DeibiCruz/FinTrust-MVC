using FinTrust.Web.Data;
using FinTrust.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class OperacionesFinancierasController : Controller
{
    private static readonly string[] TiposPermitidos =
    {
        "DEPOSITO", "RETIRO", "TRANSFERENCIA", "PAGO_CUOTA", "DESEMBOLSO"
    };

    private readonly FinTrustDbContext _context;

    public OperacionesFinancierasController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var operaciones = await _context.OperacionFinancieras
            .AsNoTracking()
            .Include(o => o.SocioEjecutor)
                .ThenInclude(s => s!.Persona)
            .OrderByDescending(o => o.FechaHora)
            .ThenByDescending(o => o.OperacionId)
            .ToListAsync();

        return View(operaciones);
    }

    public async Task<IActionResult> Details(long? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var operacion = await _context.OperacionFinancieras
            .AsNoTracking()
            .Include(o => o.SocioEjecutor)
                .ThenInclude(s => s!.Persona)
            .Include(o => o.MovimientoCuenta)
            .Include(o => o.PagoCuota)
            .FirstOrDefaultAsync(o => o.OperacionId == id);

        if (operacion is null)
        {
            return NotFound();
        }

        return View(operacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarSociosAsync();

        return View(new OperacionFinanciera
        {
            FechaHora = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("TipoOperacion,FechaHora,Monto,SocioEjecutorId,Referencia")]
        OperacionFinanciera operacion)
    {
        await ValidarOperacionAsync(operacion);

        if (ModelState.IsValid)
        {
            NormalizarOperacion(operacion);

            try
            {
                _context.Add(operacion);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Operación registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar la operación. Verifica los datos ingresados.");
            }
        }

        await CargarSociosAsync(operacion.SocioEjecutorId);
        return View(operacion);
    }

    public async Task<IActionResult> Edit(long? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var operacion = await _context.OperacionFinancieras.FindAsync(id);
        if (operacion is null)
        {
            return NotFound();
        }

        await CargarSociosAsync(operacion.SocioEjecutorId);
        return View(operacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id,
        [Bind("OperacionId,TipoOperacion,FechaHora,Monto,SocioEjecutorId,Referencia")]
        OperacionFinanciera operacion)
    {
        if (id != operacion.OperacionId)
        {
            return NotFound();
        }

        await ValidarOperacionAsync(operacion);

        if (ModelState.IsValid)
        {
            NormalizarOperacion(operacion);

            try
            {
                _context.Update(operacion);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Operación actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OperacionExistsAsync(operacion.OperacionId))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo actualizar la operación. Verifica los datos ingresados.");
            }
        }

        await CargarSociosAsync(operacion.SocioEjecutorId);
        return View(operacion);
    }

    public async Task<IActionResult> Delete(long? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var operacion = await _context.OperacionFinancieras
            .AsNoTracking()
            .Include(o => o.SocioEjecutor)
                .ThenInclude(s => s!.Persona)
            .FirstOrDefaultAsync(o => o.OperacionId == id);

        if (operacion is null)
        {
            return NotFound();
        }

        return View(operacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var operacion = await _context.OperacionFinancieras
            .Include(o => o.SocioEjecutor)
                .ThenInclude(s => s!.Persona)
            .FirstOrDefaultAsync(o => o.OperacionId == id);

        if (operacion is null)
        {
            return NotFound();
        }

        try
        {
            _context.OperacionFinancieras.Remove(operacion);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No se puede eliminar la operación porque tiene movimientos o pagos relacionados.");

            return View("Delete", operacion);
        }

        TempData["Mensaje"] = "Operación eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarOperacionAsync(OperacionFinanciera operacion)
    {
        ModelState.Remove(nameof(OperacionFinanciera.SocioEjecutor));
        ModelState.Remove(nameof(OperacionFinanciera.MovimientoCuenta));
        ModelState.Remove(nameof(OperacionFinanciera.PagoCuota));

        if (string.IsNullOrWhiteSpace(operacion.TipoOperacion))
        {
            ModelState.AddModelError(
                nameof(OperacionFinanciera.TipoOperacion),
                "Selecciona el tipo de operación.");
        }
        else if (!TiposPermitidos.Contains(operacion.TipoOperacion.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(
                nameof(OperacionFinanciera.TipoOperacion),
                "Selecciona un tipo de operación válido.");
        }

        if (operacion.FechaHora == default)
        {
            ModelState.AddModelError(
                nameof(OperacionFinanciera.FechaHora),
                "Ingresa una fecha y hora válidas.");
        }

        if (operacion.Monto <= 0)
        {
            ModelState.AddModelError(
                nameof(OperacionFinanciera.Monto),
                "El monto debe ser mayor que cero.");
        }

        if (operacion.Referencia?.Trim().Length > 80)
        {
            ModelState.AddModelError(
                nameof(OperacionFinanciera.Referencia),
                "La referencia admite hasta 80 caracteres.");
        }

        if (operacion.OperacionId > 0)
        {
            decimal montoAplicado = await _context.PagoCuota
                .Where(p => p.OperacionId == operacion.OperacionId)
                .SumAsync(p => (decimal?)p.MontoAplicado) ?? 0;

            if (montoAplicado > 0)
            {
                if (operacion.Monto < montoAplicado)
                {
                    ModelState.AddModelError(
                        nameof(OperacionFinanciera.Monto),
                        $"El monto no puede ser menor a lo ya aplicado a cuotas ({montoAplicado:N2} Bs).");
                }

                if (operacion.TipoOperacion?.Trim().ToUpperInvariant() != "PAGO_CUOTA")
                {
                    ModelState.AddModelError(
                        nameof(OperacionFinanciera.TipoOperacion),
                        "No se puede cambiar el tipo de una operación que ya tiene pagos de cuotas aplicados.");
                }
            }
        }

        if (operacion.SocioEjecutorId is int socioId &&
            !await _context.Socios.AnyAsync(s => s.SocioId == socioId))
        {
            ModelState.AddModelError(
                nameof(OperacionFinanciera.SocioEjecutorId),
                "Selecciona un socio existente.");
        }
    }

    private async Task CargarSociosAsync(int? seleccionado = null)
    {
        var socios = await _context.Socios
            .AsNoTracking()
            .Include(s => s.Persona)
            .OrderBy(s => s.CodigoSocio)
            .Select(s => new
            {
                s.SocioId,
                Nombre = s.CodigoSocio + " - " + s.Persona.Nombres + " " + s.Persona.Apellidos
            })
            .ToListAsync();

        ViewData["SocioEjecutorId"] = new SelectList(
            socios,
            nameof(Socio.SocioId),
            "Nombre",
            seleccionado);
    }

    private static void NormalizarOperacion(OperacionFinanciera operacion)
    {
        operacion.TipoOperacion = operacion.TipoOperacion.Trim().ToUpperInvariant();
        operacion.Referencia = string.IsNullOrWhiteSpace(operacion.Referencia)
            ? null
            : operacion.Referencia.Trim();
    }

    private Task<bool> OperacionExistsAsync(long id)
    {
        return _context.OperacionFinancieras.AnyAsync(o => o.OperacionId == id);
    }
}
