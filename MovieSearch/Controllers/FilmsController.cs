using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieSearch;
using Microsoft.AspNetCore.Http;

namespace MovieSearch.Controllers
{
    public class FilmsController : Controller
    {
        private readonly DBFilmsContext _context;
        //public string currentControler = "Categories";
        public FilmsController(DBFilmsContext context)
        {
            _context = context;
        }

        // GET: Films
        private bool isCorrectGanre( Film f, int? id)
        {
            foreach(var item in f.FilmGanreRelationships)
            {
                if (item.GanreId == id) return true;
            }
            return false;
        }
        private bool isCorrectActor(Film f, int? id)
        {
            foreach (var item in f.FilmActorRelationships)
            {
                if (item.ActorId == id) return true;
            }
            return false;
        }
        private void FillReturnPath(string? returnController)
        {
            ViewBag.ReturnController = returnController;
        }
        private void FillSelectLists(int id)
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            SelectList ganres = new SelectList(_context.Ganres, "Id", "Name");
            ViewBag.GanreList = new List<SelectListItem>();
            FilmGanreRelationship r = new FilmGanreRelationship();
            
            foreach (var g in ganres)
            {
                ViewBag.GanreList.Add(new SelectListItem { Value = g.Value, Text = g.Text });
            }
            SelectList actors = new SelectList(_context.Actors, "Id", "Name");
            ViewBag.ActorList = new List<SelectListItem>();
            foreach (var a in actors)
            {
                ViewBag.ActorList.Add(new SelectListItem { Value = a.Value, Text = a.Text });
            }
            ViewBag.Id = id;

        }
        public async Task<IActionResult> Index(int firstId, string? retController)
        {
            FillReturnPath(retController);
            FillSelectLists(firstId);
            if (retController == "Categories")
            {
                var filmsByCategory = _context.Films.Where(f => f.CategoryId == firstId)
                    .Include(f => f.Category)
                    .Include(f => f.FilmGanreRelationships)
                    .ThenInclude(f => f.Ganre)
                    .Include(f => f.FilmActorRelationships)
                    .ThenInclude(f => f.Actor);
                return View(await filmsByCategory.ToListAsync());
            }
            else if (retController == "Ganres")
            {
                var filmsIds = _context.FilmGanreRelationships.Where(r => r.GanreId == firstId).Select(r => r.FilmId).ToList();
                var filmsByGanre = _context.Films.Where(f => filmsIds.Contains(f.Id))
                    .Include(f => f.Category)
                    .Include(f => f.FilmGanreRelationships)
                    .ThenInclude(f => f.Ganre)
                    .Include(f => f.FilmActorRelationships)
                    .ThenInclude(f => f.Actor);
                return View(await filmsByGanre.ToListAsync());
            }
            else if (retController == "Actors")
            {
                var filmsIds = _context.FilmActorRelationships.Where(r => r.ActorId == firstId).Select(r => r.FilmId).ToList();
                var filmsByActor = _context.Films.Where(f => filmsIds.Contains(f.Id))
                    .Include(f => f.Category)
                    .Include(f => f.FilmGanreRelationships)
                    .ThenInclude(f => f.Ganre)
                    .Include(f => f.FilmActorRelationships)
                    .ThenInclude(f => f.Actor);
                return View(await filmsByActor.ToListAsync());
            }
            else
            {
                return View(await _context.Films.Include(f => f.Category)
                    .Include(f => f.FilmGanreRelationships)
                    .ThenInclude(f => f.Ganre)
                    .Include(f => f.FilmActorRelationships)
                    .ThenInclude(f => f.Actor).ToListAsync());
            }
        }

       

       

        // GET: Films/Create
        public IActionResult Create(int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
            //ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            //ViewBag.CategoryId = categoryId;
            //ViewBag.CategoryName = _context.Categories.Where(c => c.Id == categoryId).FirstOrDefault().Name;
            return View();
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CategoryId,Year,Info")] Film film, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            
            FillReturnPath(retController);
            if (ModelState.IsValid)
            {
                _context.Add(film);
                await _context.SaveChangesAsync();
                foreach (var item in film.FilmGanreRelationships)
                {
                    _context.FilmGanreRelationships.Add(item);
                    await _context.SaveChangesAsync();
                }
                
                foreach (var item in film.FilmActorRelationships)
                {
                    _context.FilmActorRelationships.Add(item);
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
            }

            return View(film);
        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(int? id, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            
            return View(film);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( [Bind("Id,Name,CategoryId,Year,Info")] Film film, int id, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(film);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(film.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
            }
            return View(film);
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(int id, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
           

            var film = await _context.Films
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
            var film = await _context.Films.FindAsync(id);
            
            foreach (var item in film.FilmActorRelationships)
            {
                _context.FilmActorRelationships.Remove(item);
            }
            await _context.SaveChangesAsync();
            foreach (var item in film.FilmGanreRelationships)
            {
                _context.FilmGanreRelationships.Remove(item);
            }
            await _context.SaveChangesAsync();
            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
    }
}
