using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Linkitty.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; }

        public static string GenerateShortCode(int length)
        {
            string possibleCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < length; i++)
                result.Append(possibleCharacters[random
                    .Next(possibleCharacters.Length)]);

            return result.ToString();
        }

        public static bool IsValidUrl(string url)
        {
            Console.WriteLine($" checking url validity on {url}");
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // If no scheme, prepend "http://"
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
                return false;

            if (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps)
                return false;

            // Now validate host looks like a real domain or IP
            var host = validatedUri.Host;

            // Reject localhost or invalid-looking hosts
            if (string.IsNullOrWhiteSpace(host) || host == "localhost")
                return false;

            // Check for at least one dot (like "example.com")
            if (!host.Contains('.') || host.EndsWith("."))
                return false;

            // Optional: allow IPs
            if (IPAddress.TryParse(host, out _))
                return true;

            // Regex: basic domain format (e.g., domain.com, sub.domain.co.uk)
            var domainRegex = new Regex(@"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z]{2,})+$");
            return domainRegex.IsMatch(host);
        }

        public static bool IsShortenedUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)
                && (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps)
                && !string.IsNullOrEmpty(validatedUri.Host); // Ensures there's a valid domain
        }

    }
}
