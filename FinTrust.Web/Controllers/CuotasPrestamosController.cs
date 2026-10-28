using FinTrust.Web.Data;
using FinTrust.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinTrust.Web.Controllers;

public class CuotasPrestamosController : Controller
{
    private const string EstadoInicial = "PENDIENTE";
    private const decimal ImporteMaximo = 9999999999999999.99m;

    private readonly FinTrustDbContext _context;

    public CuotasPrestamosController(FinTrustDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var cuotas = await _context.CuotaPrestamos
            .Include(c => c.Prestamo)
            .AsNoTracking()
            .OrderBy(c => c.Prestamo.NumeroPrestamo)
            .ThenBy(c => c.NumeroCuota)
            .ToListAsync();

        return View(cuotas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var cuota = await _context.CuotaPrestamos
            .Include(c => c.Prestamo)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CuotaId == id);

        if (cuota == null)
            return NotFound();

        ViewBag.CantidadPagos = await _context.PagoCuota
            .CountAsync(p => p.CuotaId == id);

        return View(cuota);
    }

    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("PrestamoId,NumeroCuota,FechaVencimiento,CapitalProgramado,InteresProgramado")]
        CuotaPrestamo cuota)
    {
        LimpiarModelState();

        await ValidarCuota(cuota, null, true);

        if (ModelState.IsValid)
        {
            var nueva = new CuotaPrestamo
            {
                PrestamoId = cuota.PrestamoId,
                NumeroCuota = cuota.NumeroCuota,
                FechaVencimiento = cuota.FechaVencimiento,
                CapitalProgramado = cuota.CapitalProgramado,
                InteresProgramado = cuota.InteresProgramado,
                Estado = EstadoInicial
            };

            try
            {
                _context.CuotaPrestamos.Add(nueva);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Cuota registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar la cuota. Verifique que el número de cuota no esté repetido en el préstamo y que los datos sean válidos.");
            }
        }

        CargarListas();
        return View(cuota);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cuota = await _context.CuotaPrestamos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CuotaId == id);

        if (cuota == null)
            return NotFound();

        CargarListas();
        return View(cuota);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("CuotaId,PrestamoId,NumeroCuota,FechaVencimiento,CapitalProgramado,InteresProgramado")]
        CuotaPrestamo cuota)
    {
        if (id != cuota.CuotaId)
            return BadRequest();

        var registro = await _context.CuotaPrestamos.FindAsync(id);

        if (registro == null)
            return NotFound();

        LimpiarModelState();

        await ValidarCuota(cuota, id, registro.PrestamoId != cuota.PrestamoId);

        if (registro.PrestamoId != cuota.PrestamoId)
        {
            bool tienePagos = await _context.PagoCuota
                .AnyAsync(p => p.CuotaId == id);

            if (tienePagos)
            {
                ModelState.AddModelError(
                    nameof(CuotaPrestamo.PrestamoId),
                    "No se puede cambiar el préstamo de una cuota que ya tiene pagos registrados.");
            }
        }

        decimal totalPagado = await _context.PagoCuota
            .Where(p => p.CuotaId == id)
            .SumAsync(p => (decimal?)p.MontoAplicado) ?? 0;

        if (cuota.CapitalProgramado + cuota.InteresProgramado < totalPagado)
        {
            ModelState.AddModelError(
                "",
                $"El total de la cuota no puede ser menor a lo ya pagado ({totalPagado:N2} Bs).");
        }

        if (ModelState.IsValid)
        {
            registro.PrestamoId = cuota.PrestamoId;
            registro.NumeroCuota = cuota.NumeroCuota;
            registro.FechaVencimiento = cuota.FechaVencimiento;
            registro.CapitalProgramado = cuota.CapitalProgramado;
            registro.InteresProgramado = cuota.InteresProgramado;

            try
            {
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Cuota actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar la cuota. Verifique que el número de cuota no esté repetido en el préstamo y que los datos sean válidos.");
            }
        }

        cuota.Estado = registro.Estado;

        CargarListas();
        return View(cuota);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var cuota = await _context.CuotaPrestamos
            .Include(c => c.Prestamo)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CuotaId == id);

        if (cuota == null)
            return NotFound();

        ViewBag.TienePagos = await _context.PagoCuota
            .AnyAsync(p => p.CuotaId == id);

        return View(cuota);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cuota = await _context.CuotaPrestamos.FindAsync(id);

        if (cuota == null)
            return NotFound();

        bool tienePagos = await _context.PagoCuota
            .AnyAsync(p => p.CuotaId == id);

        string mensajeError =
            "No se puede eliminar esta cuota porque tiene pagos asociados.";

        if (!tienePagos)
        {
            try
            {
                _context.CuotaPrestamos.Remove(cuota);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Cuota eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                _context.Entry(cuota).State = EntityState.Unchanged;
                mensajeError =
                    "No se pudo eliminar la cuota porque tiene registros relacionados.";
            }
        }

        ModelState.AddModelError("", mensajeError);

        var cuotaVista = await _context.CuotaPrestamos
            .Include(c => c.Prestamo)
            .AsNoTracking()
            .FirstAsync(c => c.CuotaId == id);

        ViewBag.TienePagos = true;
        return View("Delete", cuotaVista);
    }

    private async Task ValidarCuota(CuotaPrestamo cuota, int? cuotaIdActual, bool validarEstadoPrestamo)
    {
        ReemplazarError(nameof(CuotaPrestamo.PrestamoId), "Seleccione un préstamo.");
        ReemplazarError(nameof(CuotaPrestamo.NumeroCuota), "Ingrese un número de cuota válido (entre 1 y 32767).");
        ReemplazarError(nameof(CuotaPrestamo.FechaVencimiento), "Ingrese una fecha de vencimiento válida.");
        ReemplazarError(nameof(CuotaPrestamo.CapitalProgramado), "Ingrese un capital programado válido.");
        ReemplazarError(nameof(CuotaPrestamo.InteresProgramado), "Ingrese un interés programado válido.");

        if (!TieneError(nameof(CuotaPrestamo.NumeroCuota)) && cuota.NumeroCuota <= 0)
        {
            ModelState.AddModelError(
                nameof(CuotaPrestamo.NumeroCuota),
                "El número de cuota debe ser mayor a cero.");
        }

        ValidarImporte(
            cuota.CapitalProgramado,
            nameof(CuotaPrestamo.CapitalProgramado),
            "capital programado");

        ValidarImporte(
            cuota.InteresProgramado,
            nameof(CuotaPrestamo.InteresProgramado),
            "interés programado");

        bool prestamoExiste = await _context.Prestamos
            .AnyAsync(p => p.PrestamoId == cuota.PrestamoId);

        if (!prestamoExiste && !TieneError(nameof(CuotaPrestamo.PrestamoId)))
        {
            ModelState.AddModelError(
                nameof(CuotaPrestamo.PrestamoId),
                "Seleccione un préstamo válido.");
            return;
        }

        if (cuota.NumeroCuota > 0)
        {
            bool repetida = await _context.CuotaPrestamos
                .AnyAsync(c =>
                    c.PrestamoId == cuota.PrestamoId &&
                    c.NumeroCuota == cuota.NumeroCuota &&
                    (cuotaIdActual == null || c.CuotaId != cuotaIdActual));

            if (repetida)
            {
                ModelState.AddModelError(
                    nameof(CuotaPrestamo.NumeroCuota),
                    "Ya existe una cuota con este número en el préstamo seleccionado.");
            }
        }

        var prestamo = await _context.Prestamos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PrestamoId == cuota.PrestamoId);

        if (prestamo == null)
            return;

        if (validarEstadoPrestamo && prestamo.Estado != "DESEMBOLSADO")
        {
            ModelState.AddModelError(
                nameof(CuotaPrestamo.PrestamoId),
                "Solo se pueden registrar cuotas de préstamos DESEMBOLSADOS.");
        }

        if (cuota.NumeroCuota > prestamo.PlazoMeses)
        {
            ModelState.AddModelError(
                nameof(CuotaPrestamo.NumeroCuota),
                $"El número de cuota no puede ser mayor al plazo del préstamo ({prestamo.PlazoMeses} meses).");
        }

        decimal capitalOtrasCuotas = await _context.CuotaPrestamos
            .Where(c => c.PrestamoId == cuota.PrestamoId &&
                        (cuotaIdActual == null || c.CuotaId != cuotaIdActual))
            .SumAsync(c => (decimal?)c.CapitalProgramado) ?? 0;

        if (capitalOtrasCuotas + cuota.CapitalProgramado > prestamo.Capital)
        {
            ModelState.AddModelError(
                nameof(CuotaPrestamo.CapitalProgramado),
                $"El capital de las cuotas superaría el capital del préstamo. Disponible: {prestamo.Capital - capitalOtrasCuotas:N2} Bs.");
        }
    }

    private void ValidarImporte(decimal valor, string campo, string nombre)
    {
        if (valor < 0)
        {
            ModelState.AddModelError(
                campo,
                $"El {nombre} no puede ser negativo.");
        }
        else if (valor > ImporteMaximo)
        {
            ModelState.AddModelError(
                campo,
                $"El {nombre} es demasiado grande.");
        }
        else if (decimal.Round(valor, 2) != valor)
        {
            ModelState.AddModelError(
                campo,
                $"El {nombre} solo puede tener hasta 2 decimales.");
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

    private void LimpiarModelState()
    {
        ModelState.Remove(nameof(CuotaPrestamo.Prestamo));
        ModelState.Remove(nameof(CuotaPrestamo.PagoCuota));
        ModelState.Remove(nameof(CuotaPrestamo.Estado));
    }

    private void CargarListas()
    {
        ViewBag.PrestamoId = new SelectList(
            _context.Prestamos
                .AsNoTracking()
                .OrderBy(p => p.NumeroPrestamo)
                .Select(p => new
                {
                    p.PrestamoId,
                    Texto = p.NumeroPrestamo + " (" + p.Estado + ")"
                })
                .ToList(),
            "PrestamoId",
            "Texto");
    }
}
