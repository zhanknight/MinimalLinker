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

    public async Task<Link> GetLinkAsync(string shortHash)
    {
        Link? link = await _links.Links.FindAsync(shortHash);
        return link;
    }

    public async Task<Link> CreateLinkAsync(string incomingUrl)
    {
        if (!Uri.TryCreate(incomingUrl, UriKind.Absolute, out var newUrl))
        {
            return null;
        };

        var alreadyShortened = await CheckIfLinkExists(newUrl.AbsoluteUri);
        if (alreadyShortened != null) return alreadyShortened;

        var newShortHash = GenerateShortUrl(newUrl);
        
        Link newLink = new Link { fullUrl = newUrl.AbsoluteUri, id = newShortHash };
        await _links.Links.AddAsync(newLink);
        await _links.SaveChangesAsync();
        return newLink;        
    }
    private async Task<Link?> CheckIfLinkExists(string inputUrl)
    {
        var exists = await _links.Links.FirstOrDefaultAsync(x => x.fullUrl == inputUrl);
        return exists ?? null;
    }
    
    private string GenerateShortUrl(Uri inputUrl)
    {
        string shortHash = Convert.ToHexString
            (SHA1.HashData(Encoding.UTF8.GetBytes(inputUrl.AbsoluteUri)));   
        shortHash = shortHash.Substring(0,10);
        // need to check for collisions here or, more ideally, let the database generate unique ids
        return shortHash;
    }
    
}