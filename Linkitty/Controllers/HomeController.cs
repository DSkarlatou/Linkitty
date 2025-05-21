using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using Linkitty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Linkitty.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UrlDbContext _context;

        public HomeController(ILogger<HomeController> logger, UrlDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            Console.WriteLine("Index Action");
            return View();
        }

        [HttpPost]
        public IActionResult ShortenUrl(string url) 
        {

            /*TODO: CHECK IF LINK IS A REAL LINK*/

            if (!IsValidUrl(url))
            {
                ViewBag.Error = "Invalid URL. Please enter a valid link (e.g., https://example.com).";
                return View("Index");
            }

            string shortCode = UrlMapping.GenerateShortCode(6);
            var mapping = new UrlMapping
            {
                OriginalUrl = url,
                ShortUrl = shortCode,
                ClickCount = 0
            };

            _context.UrlMappings.Add(mapping);
            _context.SaveChanges();

            Console.WriteLine(shortCode);

            ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return View("Index");
        }
        public IActionResult AllLinks()
        {
            Console.WriteLine("AllLinks Action");
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

        [HttpGet("/{shortCode}")]
        public IActionResult RedirectToOriginal(string shortCode)
        {
            Console.WriteLine("RedirectToOriginal Action");
            var mapping = _context.UrlMappings
                .FirstOrDefault(m => m.ShortUrl == shortCode);
            
            if (mapping == null)
                return NotFound();

            mapping.ClickCount++;
            _context.SaveChanges();

            return Redirect(mapping.OriginalUrl);
        }

        public IActionResult DeleteAll()
        {
            Console.WriteLine("DeleteAll Action");
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE UrlMappings");
            _context.SaveChanges();
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

        public IActionResult DeleteEntry(int id)
        {
            Console.WriteLine("DeleteEntry Action");

            var entry = _context.UrlMappings.Find(id);
            if (entry != null) 
            {
                _context.UrlMappings.Remove(entry);
                _context.SaveChanges();
            }
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)
                && (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
