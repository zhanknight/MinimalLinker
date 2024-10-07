using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MinimalLinker;

public class UrlService
{
    private readonly LinksContext _links;
    public UrlService(LinksContext linksContext)
    {
       _links = linksContext; 
    }

    public async Task<Link?> CheckIfLinkExists(string inputUrl)
    {
        var exists = await _links.Links.FirstOrDefaultAsync(x => x.fullUrl == inputUrl);
        return exists ?? null;
    }
    
    public async Task<string> GenerateShortUrl(string inputUrl)
    {
        string shortHash = Convert.ToBase64String
            (SHA1.HashData(Encoding.UTF8.GetBytes(inputUrl))
            .Take(10).ToArray());   
        
        // var hashCollision = await _links.Links.FindAsync(shortHash);
        // if (hashCollision != null)
        // {
        // }
        
        return shortHash;
    }
    
}