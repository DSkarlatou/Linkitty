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
            if (TempData["ShortUrl"] != null)
                ViewBag.ShortUrl = TempData["ShortUrl"];

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        public IActionResult ShortenUrl(string url) 
        {
            /*CHECK IF LINK IS A REAL LINK*/
            if (!IsValidUrl(url))
            {
                TempData["Error"] = "Invalid URL. Please enter a valid link (e.g., https://example.com).";
                return RedirectToAction("Index");
            }

            //ensure the short code is unique
            string shortCode;
            do { 
                shortCode = UrlMapping.GenerateShortCode(6);
            } while (_context.UrlMappings
                     .Any(u => u.ShortUrl == shortCode));

            var mapping = new UrlMapping
            {
                OriginalUrl = url,
                ShortUrl = shortCode,
                ClickCount = 0
            };

            _context.UrlMappings.Add(mapping);
            _context.SaveChanges();

            TempData["ShortUrl"] = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return RedirectToAction("Index");

        }

        [HttpPost]
        public IActionResult FollowLink(string url)
        {
            
            string shortcode;

            if (IsValidUrl(url))
                shortcode = url.Substring(url.LastIndexOf('/') + 1);
            else
                shortcode = url;

            return RedirectToOriginal(shortcode); 
        }

        [Route("AllLinks")]
        public IActionResult AllLinks()
        {
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

        [HttpGet("/{shortCode}")]
        public IActionResult RedirectToOriginal(string shortCode)
        {
            var mapping = _context.UrlMappings
                .FirstOrDefault(m => m.ShortUrl == shortCode);
            
            if (mapping == null)
                return NotFound();

            mapping.ClickCount++;
            _context.SaveChanges();

            return Redirect(mapping.OriginalUrl);
        }

        private bool IsValidUrl(string url)
        {
            // If no scheme, prepend "http://"
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;
            
            return Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)
                && (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps)
                && !string.IsNullOrEmpty(validatedUri.Host); // Ensures there's a valid domain
        }


        /*TODO: these 2 should be moved to their own controller, one named AllLinksController*/
        public IActionResult DeleteAll()
        {
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE UrlMappings");
            _context.SaveChanges();
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

        public IActionResult DeleteEntry(int id)
        {

            var entry = _context.UrlMappings.Find(id);
            if (entry != null)
            {
                _context.UrlMappings.Remove(entry);
                _context.SaveChanges();
            }
            var allLinks = _context.UrlMappings.ToList();
            return View("AllLinks", allLinks);
        }

    }
}
