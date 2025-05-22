using Linkitty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Linkitty.Controllers
{
    public class CustomLinkController : Controller
    {
        private readonly UrlDbContext _context;

        public CustomLinkController(UrlDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View("CustomLink");
        }


        [HttpPost("CustomLink")]
        public IActionResult ShortenCustomLink(string url, string shortCode)
        {
            /* check url validity */
            if (!UrlMapping.IsValidUrl(url))
            {
                TempData["Error"] = $"Invalid URL: {url}. Please enter a valid link (e.g., https://example.com).";
                return RedirectToAction("Index");
            }

            /* check if short code already exists */
            if (_context.UrlMappings.AsEnumerable().Any(u => u.ShortUrl.Equals(shortCode, StringComparison.Ordinal)))
            {
                TempData["Error"] = $"This code: {shortCode} already exists in our database, please select another";
                return RedirectToAction("Index");
            }




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
    }
}
