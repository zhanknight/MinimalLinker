using System.Security.Cryptography;
using System.Text;

namespace MinimalLinker;

public static class UrlService
{
    public static string GenerateShortUrl(string inputUrl)
    {
        string shortHash = Convert.ToBase64String
            (SHA1.HashData(Encoding.UTF8.GetBytes(inputUrl))
            .Take(10).ToArray());     
        
        return shortHash;
    }
}