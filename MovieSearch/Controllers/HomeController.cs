using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MovieSearch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MovieSearch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly DBFilmsContext _context;
        private readonly ClaimsPrincipal _user;
        public HomeController(ILogger<HomeController> logger, DBFilmsContext context, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _context = context;
            _user = accessor.HttpContext.User;
        }

        public IActionResult Index()
        {
            var filmsIds = _context.FilmUserRelationships.Where(r => r.UserName == _user.Identity.Name).Select(r => r.FilmId).ToList();
            
            var films = _context.Films.Where(f => filmsIds.Contains(f.Id))
                .Include(f => f.Category)
                .Include(f => f.FilmGanreRelationships)
                .ThenInclude(f => f.Ganre)
                .Include(f => f.FilmActorRelationships)
                .ThenInclude(f => f.Actor);
            return View(films);
        }
        public async Task<IActionResult> Delete(int id)
        {
            
            var fur = _context.FilmUserRelationships.Where(f => f.FilmId == id && f.UserName == _user.Identity.Name).FirstOrDefault();
            _context.FilmUserRelationships.Remove(fur);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
