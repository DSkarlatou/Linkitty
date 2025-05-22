using Microsoft.EntityFrameworkCore;
using System.Text;

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

    }
}
