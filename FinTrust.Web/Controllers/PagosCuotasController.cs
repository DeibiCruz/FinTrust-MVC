using System.Data;
using FinTrust.Web.Data;
using FinTrust.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class PagosCuotasController : Controller
{
    private const decimal ImporteMaximo = 9999999999999999.99m;

    private const string TipoPagoCuota = "PAGO_CUOTA";

    private readonly FinTrustDbContext _context;

    public PagosCuotasController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var pagos = await _context.PagoCuota
            .Include(p => p.Cuota)
                .ThenInclude(c => c.Prestamo)
            .Include(p => p.Operacion)
            .AsNoTracking()
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        return View(pagos);
    }

    public async Task<IActionResult> Details(long id)
    {
        var pago = await BuscarPagoCompleto(id);

        if (pago == null)
            return NotFound();

        return View(pago);
    }

    public IActionResult Create()
    {
        CargarListas();

        var pago = new PagoCuotum
        {
            FechaPago = DateTime.Now
        };

        return View(pago);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("CuotaId,OperacionId,MontoAplicado,FechaPago")]
        PagoCuotum pago)
    {
        LimpiarModelState();
        TraducirErroresDeFormato();
        ValidarMonto(pago.MontoAplicado);

        if (ModelState.IsValid)
        {
            try
            {
                await using var transaccion = await _context.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable);

                await ValidarRelacionesEImportes(pago, null);

                if (ModelState.IsValid)
                {
                    var nuevo = new PagoCuotum
                    {
                        CuotaId = pago.CuotaId,
                        OperacionId = pago.OperacionId,
                        MontoAplicado = pago.MontoAplicado,
                        FechaPago = pago.FechaPago
                    };

                    _context.PagoCuota.Add(nuevo);
                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    TempData["Mensaje"] = "Pago registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar el pago. Verifique que esta operación no se haya aplicado antes a la misma cuota.");
            }
            catch (SqlException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar el pago porque otro usuario estaba registrando un pago al mismo tiempo. Intente nuevamente.");
            }
        }

        CargarListas();
        return View(pago);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var pago = await BuscarPagoCompleto(id);

        if (pago == null)
            return NotFound();

        return View(pago);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id,
        [Bind("PagoCuotaId,MontoAplicado,FechaPago")]
        PagoCuotum pago)
    {
        if (id != pago.PagoCuotaId)
            return BadRequest();

        var registro = await _context.PagoCuota.FindAsync(id);

        if (registro == null)
            return NotFound();

        pago.CuotaId = registro.CuotaId;
        pago.OperacionId = registro.OperacionId;

        LimpiarModelState();
        ModelState.Remove(nameof(PagoCuotum.CuotaId));
        ModelState.Remove(nameof(PagoCuotum.OperacionId));
        TraducirErroresDeFormato();
        ValidarMonto(pago.MontoAplicado);

        if (ModelState.IsValid)
        {
            try
            {
                await using var transaccion = await _context.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable);

                await ValidarRelacionesEImportes(pago, id);

                if (ModelState.IsValid)
                {
                    registro.MontoAplicado = pago.MontoAplicado;
                    registro.FechaPago = pago.FechaPago;

                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    TempData["Mensaje"] = "Pago actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar el pago. Revise los datos ingresados.");
            }
            catch (SqlException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar el pago porque otro usuario estaba registrando un pago al mismo tiempo. Intente nuevamente.");
            }
        }

        var pagoVista = await BuscarPagoCompleto(id);

        if (pagoVista == null)
            return NotFound();

        pagoVista.MontoAplicado = pago.MontoAplicado;
        pagoVista.FechaPago = pago.FechaPago;

        return View(pagoVista);
    }

    public async Task<IActionResult> Delete(long id)
    {
        var pago = await BuscarPagoCompleto(id);

        if (pago == null)
            return NotFound();

        return View(pago);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var pago = await _context.PagoCuota.FindAsync(id);

        if (pago == null)
            return NotFound();

        try
        {
            _context.PagoCuota.Remove(pago);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pago eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "",
                "No se pudo eliminar el pago. Intente nuevamente.");
        }

        var pagoVista = await BuscarPagoCompleto(id);

        if (pagoVista == null)
            return RedirectToAction(nameof(Index));

        return View("Delete", pagoVista);
    }

    private async Task<PagoCuotum?> BuscarPagoCompleto(long id)
    {
        return await _context.PagoCuota
            .Include(p => p.Cuota)
                .ThenInclude(c => c.Prestamo)
            .Include(p => p.Operacion)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PagoCuotaId == id);
    }

    private async Task ValidarRelacionesEImportes(PagoCuotum pago, long? pagoIdActual)
    {
        var cuota = await _context.CuotaPrestamos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CuotaId == pago.CuotaId);

        if (cuota == null)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.CuotaId),
                "Seleccione una cuota válida.");
        }

        var operacion = await _context.OperacionFinancieras
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OperacionId == pago.OperacionId);

        if (operacion == null)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.OperacionId),
                "Seleccione una operación financiera válida.");
        }

        if (cuota == null || operacion == null)
            return;

        if (pagoIdActual == null)
        {
            if (operacion.TipoOperacion != TipoPagoCuota)
            {
                ModelState.AddModelError(
                    nameof(PagoCuotum.OperacionId),
                    "Solo se pueden aplicar operaciones de tipo PAGO_CUOTA.");
            }

            bool prestamoDesembolsado = await _context.Prestamos.AnyAsync(p =>
                p.PrestamoId == cuota.PrestamoId && p.Estado == "DESEMBOLSADO");

            if (!prestamoDesembolsado)
            {
                ModelState.AddModelError(
                    nameof(PagoCuotum.CuotaId),
                    "Solo se pueden registrar pagos de cuotas de préstamos DESEMBOLSADOS.");
            }

            bool repetido = await _context.PagoCuota
                .AnyAsync(p =>
                    p.CuotaId == pago.CuotaId &&
                    p.OperacionId == pago.OperacionId);

            if (repetido)
            {
                ModelState.AddModelError(
                    "",
                    "Esta operación financiera ya fue aplicada a la cuota seleccionada.");
                return;
            }
        }

        decimal pagadoCuota = await _context.PagoCuota
            .Where(p => p.CuotaId == pago.CuotaId &&
                        (pagoIdActual == null || p.PagoCuotaId != pagoIdActual))
            .SumAsync(p => (decimal?)p.MontoAplicado) ?? 0;

        decimal saldoCuota =
            cuota.CapitalProgramado + cuota.InteresProgramado - pagadoCuota;

        if (pago.MontoAplicado > saldoCuota)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.MontoAplicado),
                $"El monto supera el saldo pendiente de la cuota ({saldoCuota:N2} Bs).");
        }

        decimal usadoOperacion = await _context.PagoCuota
            .Where(p => p.OperacionId == pago.OperacionId &&
                        (pagoIdActual == null || p.PagoCuotaId != pagoIdActual))
            .SumAsync(p => (decimal?)p.MontoAplicado) ?? 0;

        decimal disponibleOperacion = operacion.Monto - usadoOperacion;

        if (pago.MontoAplicado > disponibleOperacion)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.MontoAplicado),
                $"El monto supera el disponible de la operación ({disponibleOperacion:N2} Bs).");
        }
    }

    private void ValidarMonto(decimal monto)
    {
        if (ModelState[nameof(PagoCuotum.MontoAplicado)]?.Errors.Count > 0)
            return;

        if (monto <= 0)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.MontoAplicado),
                "El monto aplicado debe ser mayor a cero.");
        }
        else if (monto > ImporteMaximo)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.MontoAplicado),
                "El monto aplicado es demasiado grande.");
        }
        else if (decimal.Round(monto, 2) != monto)
        {
            ModelState.AddModelError(
                nameof(PagoCuotum.MontoAplicado),
                "El monto aplicado solo puede tener hasta 2 decimales.");
        }
    }

    private void TraducirErroresDeFormato()
    {
        ReemplazarError(nameof(PagoCuotum.CuotaId), "Seleccione una cuota.");
        ReemplazarError(nameof(PagoCuotum.OperacionId), "Seleccione una operación financiera.");
        ReemplazarError(nameof(PagoCuotum.MontoAplicado), "Ingrese un monto válido.");
        ReemplazarError(nameof(PagoCuotum.FechaPago), "Ingrese una fecha de pago válida.");
    }

    private void ReemplazarError(string campo, string mensaje)
    {
        if (ModelState[campo]?.Errors.Count > 0)
        {
            ModelState[campo]!.Errors.Clear();
            ModelState.AddModelError(campo, mensaje);
        }
    }

    private void LimpiarModelState()
    {
        ModelState.Remove(nameof(PagoCuotum.Cuota));
        ModelState.Remove(nameof(PagoCuotum.Operacion));
    }

    private void CargarListas()
    {
        var cuotas = _context.CuotaPrestamos
            .AsNoTracking()
            .Where(c => c.Prestamo.Estado == "DESEMBOLSADO")
            .OrderBy(c => c.Prestamo.NumeroPrestamo)
            .ThenBy(c => c.NumeroCuota)
            .Select(c => new
            {
                c.CuotaId,
                c.Prestamo.NumeroPrestamo,
                c.NumeroCuota,
                Total = c.CapitalProgramado + c.InteresProgramado
            })
            .ToList();

        ViewBag.CuotaId = new SelectList(
            cuotas.Select(c => new
            {
                c.CuotaId,
                Texto = $"{c.NumeroPrestamo} - Cuota {c.NumeroCuota} (Total {c.Total:N2} Bs)"
            }),
            "CuotaId",
            "Texto");

        var operaciones = _context.OperacionFinancieras
            .AsNoTracking()
            .Where(o => o.TipoOperacion == TipoPagoCuota)
            .OrderByDescending(o => o.OperacionId)
            .Select(o => new
            {
                o.OperacionId,
                o.TipoOperacion,
                o.Monto,
                o.Referencia
            })
            .ToList();

        ViewBag.OperacionId = new SelectList(
            operaciones.Select(o => new
            {
                o.OperacionId,
                Texto = $"#{o.OperacionId} - {o.TipoOperacion} - {o.Monto:N2} Bs" +
                        (string.IsNullOrEmpty(o.Referencia) ? "" : $" - {o.Referencia}")
            }),
            "OperacionId",
            "Texto");

        ViewBag.HayCuotas = cuotas.Count > 0;
        ViewBag.HayOperaciones = operaciones.Count > 0;
    }
}
