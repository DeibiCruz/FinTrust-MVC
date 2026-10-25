
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTrust.Web.Models;
using FinTrust.Web.Data;

public class SociosController : Controller
{
    private readonly FinTrustDbContext _context;

    public SociosController(FinTrustDbContext context)
    {
        _context = context;
    }

    // GET: SOCIOS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Socios.ToListAsync());
    }

    // GET: SOCIOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var socio = await _context.Socios
            .FirstOrDefaultAsync(m => m.SocioId == id);
        if (socio == null)
        {
            return NotFound();
        }

        return View(socio);
    }

    // GET: SOCIOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SOCIOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PersonaId,CodigoSocio,FechaIngreso,Estado")] Socio socio)
    {
        await ValidarSocio(socio);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(socio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar. Verifica la persona, el código y el estado del socio.");
            }
        }
        return View(socio);
    }

    // GET: SOCIOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var socio = await _context.Socios.FindAsync(id);
        if (socio == null)
        {
            return NotFound();
        }
        return View(socio);
    }

    // POST: SOCIOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SocioId,PersonaId,CodigoSocio,FechaIngreso,Estado")] Socio socio)
    {
        if (id != socio.SocioId)
        {
            return NotFound();
        }

        await ValidarSocio(socio);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(socio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SocioExists(socio.SocioId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar. Verifica la persona, el código y el estado del socio.");
                return View(socio);
            }
            return RedirectToAction(nameof(Index));
        }
        return View(socio);
    }

    // GET: SOCIOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var socio = await _context.Socios
            .FirstOrDefaultAsync(m => m.SocioId == id);
        if (socio == null)
        {
            return NotFound();
        }

        return View(socio);
    }

    // POST: SOCIOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var socio = await _context.Socios.FindAsync(id);
        if (socio == null)
        {
            return NotFound();
        }

        try
        {
            _context.Socios.Remove(socio);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo eliminar el socio. Puede tener cuentas, préstamos u operaciones relacionadas.");
            return View("Delete", socio);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarSocio(Socio socio)
    {
        
        ModelState.Remove(nameof(Socio.Persona));

        if (!await _context.Personas.AnyAsync(p => p.PersonaId == socio.PersonaId))
            ModelState.AddModelError(nameof(Socio.PersonaId), "Selecciona el ID de una persona existente.");

        if (await _context.Socios.AnyAsync(s => s.PersonaId == socio.PersonaId && s.SocioId != socio.SocioId))
            ModelState.AddModelError(nameof(Socio.PersonaId), "Esta persona ya está registrada como socio.");

        if (string.IsNullOrWhiteSpace(socio.CodigoSocio) || socio.CodigoSocio.Length > 25)
            ModelState.AddModelError(nameof(Socio.CodigoSocio), "El código es obligatorio y admite hasta 25 caracteres.");
        else if (await _context.Socios.AnyAsync(s => s.CodigoSocio == socio.CodigoSocio && s.SocioId != socio.SocioId))
            ModelState.AddModelError(nameof(Socio.CodigoSocio), "El código de socio ya existe.");

        if (string.IsNullOrWhiteSpace(socio.Estado) || socio.Estado.Length > 15)
            ModelState.AddModelError(nameof(Socio.Estado), "El estado es obligatorio y admite hasta 15 caracteres.");

        if (socio.FechaIngreso == default)
            ModelState.AddModelError(nameof(Socio.FechaIngreso), "Ingresa una fecha válida.");
    }

    private bool SocioExists(int socioid)
    {
        return _context.Socios.Any(e => e.SocioId == socioid);
    }
}
