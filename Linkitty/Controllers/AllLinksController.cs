using Linkitty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Linkitty.Controllers
{
    public class AllLinksController : Controller
    {
        private readonly UrlDbContext _context;

        public AllLinksController(UrlDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

        [HttpPost]
        public IActionResult DeleteAll()
        {
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE UrlMappings");
            _context.SaveChanges();

            var allLinks = _context.UrlMappings.ToList();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteEntry(int id)
        {
            var entry = _context.UrlMappings.Find(id);
            if (entry != null)
            {
                _context.UrlMappings.Remove(entry);
                _context.SaveChanges();
            }

            var allLinks = _context.UrlMappings.ToList();
            return RedirectToAction("Index");
        }
    }
}
