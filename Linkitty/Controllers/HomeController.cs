using Linkitty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Text;


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
            string domain = $"{Request.Scheme}://{Request.Host}";
            

            /*check if link is real link*/
            if (!UrlMapping.IsValidUrl(url))
            {
                TempData["Error"] = $"Invalid URL: {url}. Please enter a valid link (e.g., https://example.com).";
                return RedirectToAction("Index");
            }

            /*ensure the short code is unique*/
            string shortCode;
            do {
                shortCode = UrlMapping.GenerateShortCode(6);
            } while (_context.UrlMappings.Any(u => u.ShortUrl == shortCode));

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
            string domain = $"{Request.Scheme}://{Request.Host}";
            if (UrlMapping.IsShortenedUrl(url, domain))
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
            {
                TempData["Error"] = $"{shortCode} was not found in our database";
                return RedirectToAction("Index");
            }

            mapping.ClickCount++;
            _context.SaveChanges();

            return Redirect(mapping.OriginalUrl);
        }

        [Route("CustomLink")]
        public IActionResult CustomLink()
        {
            return View("CustomLink");
        }
    }
}
