using FinTrust.Web.Data;
using FinTrust.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class CuentasController : Controller
{
    private readonly FinTrustDbContext _context;

    public CuentasController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Cuenta.AsNoTracking().OrderBy(c => c.NumeroCuenta).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        return await MostrarDetalle(id);
    }

    public IActionResult Create()
    {
        return View(new Cuentum
        {
            FechaApertura = DateOnly.FromDateTime(DateTime.Today),
            Moneda = "BOB",
            TipoCuenta = "AHORRO",
            Estado = "ACTIVA"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NumeroCuenta,TipoCuenta,Moneda,SaldoActual,FechaApertura,Estado")] Cuentum cuenta)
    {
        await ValidarCuenta(cuenta);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Cuenta.Add(cuenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = cuenta.CuentaId });
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar la cuenta. Revisa los datos y el número de cuenta.");
            }
        }
        return View(cuenta);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cuenta = await _context.Cuenta.FindAsync(id);
        return cuenta == null ? NotFound() : View(cuenta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CuentaId,NumeroCuenta,TipoCuenta,Moneda,SaldoActual,FechaApertura,Estado")] Cuentum cuenta)
    {
        if (id != cuenta.CuentaId) return NotFound();
        var existente = await _context.Cuenta.FindAsync(id);
        if (existente == null) return NotFound();
        await ValidarCuenta(cuenta);
        if (ModelState.IsValid)
        {
            existente.NumeroCuenta = cuenta.NumeroCuenta;
            existente.TipoCuenta = cuenta.TipoCuenta;
            existente.Moneda = cuenta.Moneda;
            existente.SaldoActual = cuenta.SaldoActual;
            existente.FechaApertura = cuenta.FechaApertura;
            existente.Estado = cuenta.Estado;
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Cuenta.AnyAsync(c => c.CuentaId == id)) return NotFound();
                ModelState.AddModelError("", "La cuenta cambió durante la edición. Recarga e inténtalo nuevamente.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar la cuenta. Revisa los datos y el número de cuenta.");
            }
        }
        return View(cuenta);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var cuenta = await _context.Cuenta.AsNoTracking().FirstOrDefaultAsync(c => c.CuentaId == id);
        return cuenta == null ? NotFound() : View(cuenta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cuenta = await _context.Cuenta.Include(c => c.CuentaSocios).FirstOrDefaultAsync(c => c.CuentaId == id);
        if (cuenta == null) return NotFound();
        if (await _context.Entry(cuenta).Collection(c => c.MovimientoCuenta).Query().AnyAsync()
            || await _context.Entry(cuenta).Collection(c => c.Prestamos).Query().AnyAsync())
        {
            ModelState.AddModelError("", "No se puede eliminar una cuenta con movimientos o préstamos relacionados.");
            return View(cuenta);
        }
        try
        {
            _context.CuentaSocios.RemoveRange(cuenta.CuentaSocios);
            _context.Cuenta.Remove(cuenta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo eliminar la cuenta. Revisa sus registros relacionados.");
            return View(cuenta);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vincular(int id, int socioId, string rol, bool puedeOperar)
    {
        if (!await _context.Cuenta.AnyAsync(c => c.CuentaId == id)) return NotFound();
        if (!await _context.Socios.AnyAsync(s => s.SocioId == socioId))
            ModelState.AddModelError("", "Selecciona un socio existente.");
        if (rol != "TITULAR" && rol != "COTITULAR")
            ModelState.AddModelError("", "Selecciona titular o cotitular.");
        if (await _context.CuentaSocios.AnyAsync(cs => cs.CuentaId == id && cs.SocioId == socioId))
            ModelState.AddModelError("", "El socio ya está asociado a esta cuenta.");
        if (!ModelState.IsValid) return await MostrarDetalle(id);
        _context.CuentaSocios.Add(new CuentaSocio
        {
            CuentaId = id,
            SocioId = socioId,
            Rol = rol,
            PuedeOperar = puedeOperar,
            FechaVinculacion = DateOnly.FromDateTime(DateTime.Today)
        });
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo asociar al socio. Comprueba que siga disponible y no esté duplicado.");
            return await MostrarDetalle(id);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarSocio(int id, int socioId, string rol, bool puedeOperar)
    {
        var vinculo = await _context.CuentaSocios.FindAsync(id, socioId);
        if (vinculo == null) return NotFound();
        if (rol != "TITULAR" && rol != "COTITULAR")
            ModelState.AddModelError("", "Selecciona titular o cotitular.");
        if (!ModelState.IsValid) return await MostrarDetalle(id);
        vinculo.Rol = rol;
        vinculo.PuedeOperar = puedeOperar;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo actualizar la asociación. Recarga la cuenta e inténtalo nuevamente.");
            return await MostrarDetalle(id);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desvincular(int id, int socioId)
    {
        var vinculo = await _context.CuentaSocios.FindAsync(id, socioId);
        if (vinculo == null) return NotFound();
        _context.CuentaSocios.Remove(vinculo);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo quitar la asociación. Recarga la cuenta e inténtalo nuevamente.");
            return await MostrarDetalle(id);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IActionResult> MostrarDetalle(int id)
    {
        var cuenta = await _context.Cuenta.AsNoTracking()
            .Include(c => c.CuentaSocios).ThenInclude(cs => cs.Socio).ThenInclude(s => s.Persona)
            .FirstOrDefaultAsync(c => c.CuentaId == id);
        if (cuenta == null) return NotFound();
        var socios = await _context.Socios.AsNoTracking()
            .Where(s => !s.CuentaSocios.Any(cs => cs.CuentaId == id))
            .OrderBy(s => s.CodigoSocio)
            .Select(s => new { s.SocioId, Nombre = s.CodigoSocio + " - " + s.Persona.Nombres + " " + s.Persona.Apellidos })
            .ToListAsync();
        ViewBag.Socios = new SelectList(socios, "SocioId", "Nombre");
        return View("Details", cuenta);
    }

    private async Task ValidarCuenta(Cuentum cuenta)
    {
        var saldoTexto = Request.Form[nameof(cuenta.SaldoActual)].ToString();
        ModelState.Remove(nameof(cuenta.SaldoActual));
        if (decimal.TryParse(saldoTexto, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint,
            System.Globalization.CultureInfo.InvariantCulture, out var saldo))
            cuenta.SaldoActual = saldo;
        else
            ModelState.AddModelError(nameof(cuenta.SaldoActual), "Ingresa un saldo válido.");
        if (string.IsNullOrWhiteSpace(cuenta.NumeroCuenta) || cuenta.NumeroCuenta.Length > 30)
            ModelState.AddModelError(nameof(cuenta.NumeroCuenta), "El número es obligatorio y admite hasta 30 caracteres.");
        else if (await _context.Cuenta.AnyAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta && c.CuentaId != cuenta.CuentaId))
            ModelState.AddModelError(nameof(cuenta.NumeroCuenta), "El número de cuenta ya existe.");
        if (cuenta.TipoCuenta != "AHORRO" && cuenta.TipoCuenta != "CORRIENTE")
            ModelState.AddModelError(nameof(cuenta.TipoCuenta), "Selecciona ahorro o corriente.");
        if (cuenta.Moneda != "BOB" && cuenta.Moneda != "USD")
            ModelState.AddModelError(nameof(cuenta.Moneda), "Selecciona BOB o USD.");
        if (cuenta.Estado != "ACTIVA" && cuenta.Estado != "BLOQUEADA" && cuenta.Estado != "CERRADA")
            ModelState.AddModelError(nameof(cuenta.Estado), "Selecciona un estado válido.");
        if (cuenta.SaldoActual < 0 || cuenta.SaldoActual > 9999999999999999.99m || decimal.Round(cuenta.SaldoActual, 2) != cuenta.SaldoActual)
            ModelState.AddModelError(nameof(cuenta.SaldoActual), "El saldo debe ser no negativo, con hasta 16 enteros y 2 decimales.");
        if (cuenta.FechaApertura == default)
            ModelState.AddModelError(nameof(cuenta.FechaApertura), "Ingresa una fecha válida.");
    }
}
