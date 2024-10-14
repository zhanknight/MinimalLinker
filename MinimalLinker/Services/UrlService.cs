using Microsoft.EntityFrameworkCore;

namespace MinimalLinker;

public class UrlService
{
    private readonly LinksContext _links;
    public UrlService(LinksContext linksContext)
    {
       _links = linksContext; 
    }

    public async Task<Link> GetLinkAsync(int id)
    {
        Link? link = await _links.Links.FindAsync(id);
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
        
        Link newLink = new Link { fullUrl = newUrl.AbsoluteUri, id = 0};
        await _links.Links.AddAsync(newLink);
        await _links.SaveChangesAsync();
        return newLink;        
    }
    private async Task<Link?> CheckIfLinkExists(string inputUrl)
    {
        var exists = await _links.Links.FirstOrDefaultAsync(x => x.fullUrl == inputUrl);
        return exists ?? null;
    }   
}