using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MovieSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly DBFilmsContext _context;
        public ChartsController(DBFilmsContext context)
        {
            _context = context;
        }
        [HttpGet("JsonDataForCategories")]
        public JsonResult JsonDataForCategories()
        {
            var categories = _context.Categories.Include(f => f.Films).ToList();
            List<object> catFilm = new List<object>();
            catFilm.Add(new[] { "Категорія", "Кількість книжок" });
            foreach(var c in categories)
            {
                catFilm.Add(new object[] { c.Name, c.Films.Count() });
            }
            return new JsonResult(catFilm);
        }
        [HttpGet("JsonDataForGanres")]
        public JsonResult JsonDataForGanres()
        {
            var ganres = _context.Ganres.Include(f => f.FilmGanreRelationships).ThenInclude(f => f.Film).ToList();
            List<object> ganFilm = new List<object>();
            ganFilm.Add(new[] { "Жанр", "Кількість книжок" });
            foreach (var c in ganres)
            {
                ganFilm.Add(new object[] { c.Name, c.FilmGanreRelationships.Count() });
            }
            return new JsonResult(ganFilm);
        }
    }
}
