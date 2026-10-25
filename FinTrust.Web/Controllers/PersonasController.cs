
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTrust.Web.Models;
using FinTrust.Web.Data;

public class PersonasController : Controller
{
    private readonly FinTrustDbContext _context;

    public PersonasController(FinTrustDbContext context)
    {
        _context = context;
    }

    // GET: PERSONAS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Personas.ToListAsync());
    }

    // GET: PERSONAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var persona = await _context.Personas
            .FirstOrDefaultAsync(m => m.PersonaId == id);

        if (persona == null)
        {
            return NotFound();
        }

        return View(persona);
    }

    // GET: PERSONAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PERSONAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("DocumentoIdentidad,Nombres,Apellidos,Telefono,Correo")] Persona persona)
    {
        await ValidarPersona(persona);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Persona registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar la persona. Comprueba que el documento no esté duplicado.");
            }
        }

        return View(persona);
    }



    // GET: PERSONAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var persona = await _context.Personas.FindAsync(id);

        if (persona == null)
        {
            return NotFound();
        }

        return View(persona);
    }


    // POST: PERSONAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("PersonaId,DocumentoIdentidad,Nombres,Apellidos,Telefono,Correo")] Persona persona)
    {
        if (id != persona.PersonaId)
        {
            return NotFound();
        }

        await ValidarPersona(persona);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(persona);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonaExists(persona.PersonaId))
                {
                    return NotFound();
                }

                ModelState.AddModelError("", "La persona cambió durante la edición. Recarga e inténtalo nuevamente.");
                return View(persona);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar la persona. Comprueba que el documento no esté duplicado.");
                return View(persona);
            }

            TempData["Mensaje"] = "Persona actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        return View(persona);
    }



    // GET: PERSONAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var persona = await _context.Personas
            .FirstOrDefaultAsync(m => m.PersonaId == id);

        if (persona == null)
        {
            return NotFound();
        }

        return View(persona);
    }

    // POST: PERSONAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var persona = await _context.Personas.FindAsync(id);

        if (persona == null)
        {
            return NotFound();
        }

        _context.Personas.Remove(persona);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "",
                "No se puede eliminar esta persona porque tiene registros relacionados."
            );

            return View("Delete", persona);
        }

        return RedirectToAction(nameof(Index));
    }
    private async Task ValidarPersona(Persona persona)
    {
        persona.DocumentoIdentidad = persona.DocumentoIdentidad?.Trim()!;
        persona.Nombres = persona.Nombres?.Trim()!;
        persona.Apellidos = persona.Apellidos?.Trim()!;
        if (!string.IsNullOrWhiteSpace(persona.DocumentoIdentidad)
            && await _context.Personas.AnyAsync(p => p.DocumentoIdentidad == persona.DocumentoIdentidad && p.PersonaId != persona.PersonaId))
            ModelState.AddModelError(nameof(Persona.DocumentoIdentidad), "El documento de identidad ya está registrado.");
    }

    //Comprueba si una persona existe en la base de datos mediante su PersonaId
    //Lo utilizamos en Edit cuando Entity Framework Core detecta un conflicto de concurrencia
    private bool PersonaExists(int id)
    {
        return _context.Personas.Any(e => e.PersonaId == id);
    }
}
