using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieSearch;
using System.IO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace MovieSearch.Controllers
{
    public class FilmsController : Controller
    {
        private readonly DBFilmsContext _context;
        private readonly ClaimsPrincipal _user;
        //public string currentControler = "Categories";
        public FilmsController(DBFilmsContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _user = accessor.HttpContext.User;
        }

        // GET: Films

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
            var ganresId = Request.Form["ganres"];
            var actorsId = Request.Form["actors"];
            List<int> gIds = new List<int>();
            foreach (var g in ganresId)
            {
                gIds.Add(int.Parse(g));
            }
            List<int> aIds = new List<int>();
            foreach (var a in actorsId)
            {
                aIds.Add(int.Parse(a));
            }
            if (!IsDuplicate(film, gIds, aIds, 0))
            {
                if (ModelState.IsValid)
                {
                    _context.Add(film);

                    foreach (var gId in gIds)
                    {
                        var ganre = _context.Ganres.Where(g => g.Id == gId).FirstOrDefault();
                        FilmGanreRelationship fgr = new FilmGanreRelationship();
                        fgr.Film = film;
                        fgr.GanreId = gId;
                        fgr.Ganre = ganre;
                        ganre.FilmGanreRelationships.Add(fgr);
                        film.FilmGanreRelationships.Add(fgr);
                        _context.FilmGanreRelationships.Add(fgr);
                    }

                    foreach (var aId in aIds)
                    {
                        var actor = _context.Actors.Where(a => a.Id == aId).FirstOrDefault();
                        FilmActorRelationship far = new FilmActorRelationship();
                        far.Film = film;
                        far.ActorId = aId;
                        far.Actor = actor;
                        actor.FilmActorRelationships.Add(far);
                        film.FilmActorRelationships.Add(far);
                        _context.FilmActorRelationships.Add(far);
                    }
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
                }
                return View(film);
            }
            else
                ModelState.AddModelError("Name", "Такий фільм уже існує");

            return View(film);
        }


        public async Task<IActionResult> Favourite(int id, int firstId, string? retController)
        {

            FillSelectLists(firstId);
            FillReturnPath(retController);
            FilmUserRelationship fur = new FilmUserRelationship();
            fur.FilmId = id;
            fur.UserName = _user.Identity.Name;
            var films = _context.FilmUserRelationships.Where(f => f.UserName == fur.UserName && f.FilmId == id);
            if (films.Count() == 0) _context.FilmUserRelationships.Add(fur);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
        }
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
        public async Task<IActionResult> Edit([Bind("Id,Name,CategoryId,Year,Info")] Film film, int id, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
            var ganresId = Request.Form["ganres"];
            var actorsId = Request.Form["actors"];
            List<int> gIds = new List<int>();
            foreach (var g in ganresId)
            {
                gIds.Add(int.Parse(g));
            }
            List<int> aIds = new List<int>();
            foreach (var a in actorsId)
            {
                aIds.Add(int.Parse(a));
            }
            if (!IsDuplicate(film, gIds, aIds, id))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(film);
                        film.FilmGanreRelationships.Clear();
                        var GIds = _context.FilmGanreRelationships.Where(r => r.FilmId == id);
                        foreach (var item in GIds)
                        {
                            _context.Remove(item);
                        }
                        foreach (var gId in gIds)
                        {
                            var ganre = _context.Ganres.Where(g => g.Id == gId).FirstOrDefault();
                            FilmGanreRelationship fgr = new FilmGanreRelationship();
                            fgr.Film = film;
                            fgr.GanreId = gId;
                            fgr.Ganre = ganre;
                            foreach (var e in ganre.FilmGanreRelationships)
                            {
                                if (e.FilmId == film.Id) ganre.FilmGanreRelationships.Remove(e);
                            }
                            ganre.FilmGanreRelationships.Add(fgr);
                            film.FilmGanreRelationships.Add(fgr);
                            _context.FilmGanreRelationships.Add(fgr);
                        }
                        film.FilmGanreRelationships.Clear();
                        var AIds = _context.FilmActorRelationships.Where(r => r.FilmId == id);
                        foreach (var item in AIds)
                        {
                            _context.Remove(item);
                        }
                        foreach (var aId in aIds)
                        {
                            var actor = _context.Actors.Where(a => a.Id == aId).FirstOrDefault();
                            FilmActorRelationship far = new FilmActorRelationship();
                            far.Film = film;
                            far.ActorId = aId;
                            far.Actor = actor;
                            foreach (var e in actor.FilmActorRelationships)
                            {
                                if (e.FilmId == film.Id) actor.FilmActorRelationships.Remove(e);
                            }
                            actor.FilmActorRelationships.Add(far);
                            film.FilmActorRelationships.Add(far);
                            _context.FilmActorRelationships.Add(far);
                        }
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
            else
                ModelState.AddModelError("Name", "Такий фільм уже існує");

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
            var ganresIds = _context.FilmGanreRelationships.Where(g => g.FilmId == id);
            foreach (var item in ganresIds)
            {
                _context.FilmGanreRelationships.Remove(item);
            }

            var actorsIds = _context.FilmActorRelationships.Where(g => g.FilmId == id);
            foreach (var item in actorsIds)
            {
                _context.FilmActorRelationships.Remove(item);
            }
            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
        public IActionResult Report(int firstId, string? retController)
        {
            FillReturnPath(retController);
            FillSelectLists(firstId);
            return View();
        }

        [HttpPost, ActionName("Import")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel, int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);
            if (ModelState.IsValid)
            {
                if (fileExcel != null)
                {
                    using (var stream = new FileStream(fileExcel.FileName, FileMode.Create))
                    {
                        await fileExcel.CopyToAsync(stream);
                        using (XLWorkbook workBook = new XLWorkbook(stream, XLEventTracking.Disabled))
                        {
                            foreach (IXLWorksheet worksheet in workBook.Worksheets)
                            {

                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    try
                                    {
                                        Film film = new Film();
                                        film.Name = row.Cell(1).Value.ToString();
                                        film.Info = row.Cell(15).Value.ToString();
                                        film.Year = int.Parse(row.Cell(16).Value.ToString());
                                        Category newcat;
                                        var c = (from cat in _context.Categories
                                                 where cat.Name.Contains(row.Cell(14).Value.ToString())
                                                 select cat).ToList();
                                        if (c.Count > 0)
                                        {
                                            newcat = c[0];
                                        }
                                        else
                                        {
                                            newcat = new Category();
                                            newcat.Name = worksheet.Name;
                                            _context.Categories.Add(newcat);
                                        }
                                        film.Category = newcat;
                                        film.CategoryId = newcat.Id;
                                        List<int> gIds = new List<int>();
                                        List<int> aIds = new List<int>();
                                        
                                        for (int i = 2; i < 8; i++)
                                        {
                                            if (row.Cell(i).Value.ToString().Length > 0)
                                            {
                                                Actor actor;

                                                var a = (from act in _context.Actors
                                                         where act.Name.Contains(row.Cell(i).Value.ToString())
                                                         select act).ToList();
                                                if (a.Count > 0)
                                                {
                                                    actor = a[0];
                                                }
                                                else
                                                {
                                                    actor = new Actor();
                                                    actor.Name = row.Cell(i).Value.ToString();
                                                    _context.Add(actor);
                                                }
                                                aIds.Add(actor.Id);
                                                
                                            }

                                        }
                                        for (int i = 8; i < 14; i++)
                                        {
                                            if (row.Cell(i).Value.ToString().Length > 0)
                                            {
                                                Ganre ganre;

                                                var g = (from gan in _context.Ganres
                                                         where gan.Name.Contains(row.Cell(i).Value.ToString())
                                                         select gan).ToList();
                                                if (g.Count > 0)
                                                {
                                                    ganre = g[0];
                                                }
                                                else
                                                {
                                                    ganre = new Ganre();
                                                    ganre.Name = row.Cell(i).Value.ToString();
                                                    _context.Add(ganre);
                                                }
                                                gIds.Add(ganre.Id);
                                            }
                                        }
                                        if (!IsDuplicate(film, gIds, aIds, 0))
                                        {
                                            _context.Films.Add(film);
                                            foreach (var aId in aIds)
                                            {
                                                FilmActorRelationship far = new FilmActorRelationship();
                                                var actor = _context.Actors.Where(a => a.Id == aId).FirstOrDefault();
                                                far.Film = film;
                                                far.Actor = actor;
                                                _context.FilmActorRelationships.Add(far);

                                            }

                                            foreach (var gId in gIds)
                                            {
                                                FilmGanreRelationship fgr = new FilmGanreRelationship();
                                                var ganre = _context.Ganres.Where(g => g.Id == gId).FirstOrDefault();

                                                fgr.Film = film;
                                                fgr.Ganre = ganre;
                                                _context.FilmGanreRelationships.Add(fgr);
                                            }
                                        }
                                        
                                    }
                                    catch (Exception e)
                                    {
                                        return View();
                                    }
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Films", new { firstId = firstId, retController = retController });
        }
        public ActionResult Export(int firstId, string? retController)
        {
            FillSelectLists(firstId);
            FillReturnPath(retController);

            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Films");

                worksheet.Cell("A1").Value = "Назва";
                worksheet.Cell("B1").Value = "Актор 1";
                worksheet.Cell("C1").Value = "Актор 2";
                worksheet.Cell("D1").Value = "Актор 3";
                worksheet.Cell("E1").Value = "Актор 4";
                worksheet.Cell("F1").Value = "Актор 5";
                worksheet.Cell("G1").Value = "Актор 6";
                worksheet.Cell("H1").Value = "Жанр 1";
                worksheet.Cell("I1").Value = "Жанр 2";
                worksheet.Cell("J1").Value = "Жанр 3";
                worksheet.Cell("K1").Value = "Жанр 4";
                worksheet.Cell("L1").Value = "Жанр 5";
                worksheet.Cell("M1").Value = "Жанр 6";
                worksheet.Cell("N1").Value = "Категорія";
                worksheet.Cell("O1").Value = "Інформація";
                worksheet.Cell("P1").Value = "Рік";
                worksheet.Row(1).Style.Font.Bold = true;
                if (retController == "Categories")
                {
                    var filmsByCategory = _context.Films.Where(f => f.CategoryId == firstId)
                        .Include(f => f.Category).ToList();
                    for (int i = 0; i < filmsByCategory.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = filmsByCategory[i].Name;
                        worksheet.Cell(i + 2, 15).Value = filmsByCategory[i].Info;
                        worksheet.Cell(i + 2, 14).Value = (_context.Categories.Where(c => c.Id == filmsByCategory[i].CategoryId).FirstOrDefault()).Name;
                        worksheet.Cell(i + 2, 16).Value = filmsByCategory[i].Year;

                        var far = _context.FilmActorRelationships.Where(a => a.FilmId == filmsByCategory[i].Id).Include("Actor").ToList();
                        int j = 2;
                        foreach (var r in far)
                        {
                            if (j <= 7)
                            {
                                worksheet.Cell(i + 2, j).Value = r.Actor.Name;
                                j++;
                            }
                        }
                        var fgr = _context.FilmGanreRelationships.Where(a => a.FilmId == filmsByCategory[i].Id).Include("Ganre").ToList();
                        j = 8;
                        foreach (var r in fgr)
                        {
                            if (j < 14)
                            {
                                worksheet.Cell(i + 2, j).Value = r.Ganre.Name;
                                j++;
                            }
                        }

                    }
                }
                else if (retController == "Ganres")
                {
                    var filmsIds = _context.FilmGanreRelationships.Where(r => r.GanreId == firstId).Select(r => r.FilmId).ToList();
                    var filmsByGanre = _context.Films.Where(f => filmsIds.Contains(f.Id))
                        .Include(f => f.Category).ToList();
                    for (int i = 0; i < filmsByGanre.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = filmsByGanre[i].Name;
                        worksheet.Cell(i + 2, 15).Value = filmsByGanre[i].Info;
                        worksheet.Cell(i + 2, 14).Value = (_context.Categories.Where(c => c.Id == filmsByGanre[i].CategoryId).FirstOrDefault()).Name;
                        worksheet.Cell(i + 2, 16).Value = filmsByGanre[i].Year;

                        var far = _context.FilmActorRelationships.Where(a => a.FilmId == filmsByGanre[i].Id).Include("Actor").ToList();
                        int j = 2;
                        foreach (var r in far)
                        {
                            if (j <= 7)
                            {
                                worksheet.Cell(i + 2, j).Value = r.Actor.Name;
                                j++;
                            }
                        }
                        var fgr = _context.FilmGanreRelationships.Where(a => a.FilmId == filmsByGanre[i].Id).Include("Ganre").ToList();
                        j = 8;
                        foreach (var r in fgr)
                        {
                            if (j < 14)
                            {
                                worksheet.Cell(i + 2, j).Value = r.Ganre.Name;
                                j++;
                            }
                        }

                    }
                }
                else if (retController == "Actors")
                {
                    var filmsIds = _context.FilmActorRelationships.Where(r => r.ActorId == firstId).Select(r => r.FilmId).ToList();
                    var filmsByActor = _context.Films.Where(f => filmsIds.Contains(f.Id))
                        .Include(f => f.Category).ToList();
                    for (int i = 0; i < filmsByActor.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = filmsByActor[i].Name;
                        worksheet.Cell(i + 2, 15).Value = filmsByActor[i].Info;
                        worksheet.Cell(i + 2, 16).Value = filmsByActor[i].Year;
                        worksheet.Cell(i + 2, 14).Value = (_context.Categories.Where(c => c.Id == filmsByActor[i].CategoryId).FirstOrDefault()).Name;

                        var far = _context.FilmActorRelationships.Where(a => a.FilmId == filmsByActor[i].Id).Include("Actor").ToList();
                        int j = 2;
                        foreach (var r in far)
                        {
                            if (j <= 7)
                            {
                                worksheet.Cell(i + 2, j + 2).Value = r.Actor.Name;
                                j++;
                            }
                        }
                        var fgr = _context.FilmGanreRelationships.Where(a => a.FilmId == filmsByActor[i].Id).Include("Ganre").ToList();
                        j = 8;
                        foreach (var r in fgr)
                        {
                            if (j < 14)
                            {
                                worksheet.Cell(i + 2, j).Value = r.Ganre.Name;
                                j++;
                            }
                        }

                    }

                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"films_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
        
        private bool IsDuplicate(Film model, List<int> gIds, List<int> aIds, int id)
        {


            var films = _context.Films.Where(f => f.Name.Equals(model.Name)
                                               && f.Year.Equals(model.Year)
                                               && f.CategoryId.Equals(model.CategoryId)
                                               && f.Id != id)
                                            .Include(f => f.Category)
                                            .Include(f => f.FilmGanreRelationships)
                                            .ThenInclude(f => f.Ganre)
                                            .Include(f => f.FilmActorRelationships)
                                            .ThenInclude(f => f.Actor);
            foreach (var f in films)
            {
                foreach (var fg in f.FilmGanreRelationships)
                {
                    if (!gIds.Contains(fg.GanreId)) return true;
                }
                foreach (var fa in f.FilmActorRelationships)
                {
                    if (!aIds.Contains(fa.ActorId)) return true;
                }
                if (f.Info.Equals(model.Info)) return true;
            }
            return false;
        }
    }
}
