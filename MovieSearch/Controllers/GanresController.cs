using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieSearch;
using Microsoft.AspNetCore.Authorization;

namespace MovieSearch.Controllers
{
    public class GanresController : Controller
    {
        private readonly DBFilmsContext _context;

        public GanresController(DBFilmsContext context)
        {
            _context = context;
        }

        // GET: Ganres
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ganres.ToListAsync());
        }

       

        // GET: Ganres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ganres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Ganre ganre)
        {
            if (!IsDuplicate(ganre))
            {
                if (ModelState.IsValid)
                {
                    _context.Add(ganre);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(ganre);
            }
            else
                ModelState.AddModelError("Name", "Такий жанр уже існує");

            return View(ganre);
        }

        // GET: Ganres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganre = await _context.Ganres.FindAsync(id);
            if (ganre == null)
            {
                return NotFound();
            }
            return View(ganre);
        }

        // POST: Ganres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Ganre ganre)
        {
            if (id != ganre.Id)
            {
                return NotFound();
            }
            var model = _context.Ganres.FirstOrDefault(g => g.Name.Equals(ganre.Name) && g.Id != id);
            if (model == null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(ganre);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!GanreExists(ganre.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(ganre);
            }
            else
                ModelState.AddModelError("Name", "Такий жанр уже існує");

            return View(ganre);
        }

        // GET: Ganres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganre = await _context.Ganres
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ganre == null)
            {
                return NotFound();
            }

            return View(ganre);
        }

        // POST: Ganres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ganre = await _context.Ganres.FindAsync(id);
            var filmGanreRelationships = _context.FilmGanreRelationships.Where(r => r.GanreId == id).ToList();
            foreach(var item in filmGanreRelationships)
            {
                _context.FilmGanreRelationships.Remove(item);
                await _context.SaveChangesAsync();
            }
            _context.Ganres.Remove(ganre);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GanreExists(int id)
        {
            return _context.Ganres.Any(e => e.Id == id);
        }
        private bool IsDuplicate(Ganre model)
        {
            var ganre = _context.Ganres.FirstOrDefault(g => g.Name.Equals(model.Name));

            return ganre == null ? false : true;
        }
    }
}
