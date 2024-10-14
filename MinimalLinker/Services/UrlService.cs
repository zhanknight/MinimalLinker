using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MinimalLinker;

public class UrlService
{
    private readonly LinksContext _links;
    private readonly IMemoryCache _cache;

    public UrlService(LinksContext linksContext, IMemoryCache cache)
    {
        _links = linksContext;
        _cache = cache;
    }

    public async Task<Link> GetLinkAsync(int id)
    {
        if (!_cache.TryGetValue(id, out Link cachedLink))
        {
            cachedLink = await _links.Links.FindAsync(id);

            if (cachedLink == null)
            {
                return null;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetSlidingExpiration(TimeSpan.FromDays(1));

            _cache.Set(id, cachedLink, cacheEntryOptions);
        }

        return cachedLink;
    }

    public async Task<Link> CreateLinkAsync(string incomingUrl)
    {
        if (!Uri.TryCreate(incomingUrl, UriKind.Absolute, out var newUrl))
        {
            return null;
        };

        var alreadyShortened = await CheckIfLinkExists(newUrl.AbsoluteUri);
        if (alreadyShortened != null) return alreadyShortened;

        Link newLink = new Link { fullUrl = newUrl.AbsoluteUri, id = 0 };
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
