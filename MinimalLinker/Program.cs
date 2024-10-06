using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalLinker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LinksContext>
    (o => o.UseSqlite(builder.Configuration.GetConnectionString("LinksContext")));

var app = builder.Build();

// Return the link object
app.MapGet("/links/{id}", async Task<Results<NotFound, Ok<Link>>>
    ([FromRoute] string id, LinksContext context) =>
    {
        Link? link = await context.Links.FindAsync(id);

        if (link == null) return TypedResults.NotFound();
        
        return TypedResults.Ok<Link>(link);
    });

// Redirect from short link to original url
app.MapGet("/{id}", async Task<Results<NotFound, RedirectHttpResult>>
    ([FromRoute] string id, LinksContext context) =>
    {
        Link? link = await context.Links.FindAsync(id);
        
        if (link == null) return TypedResults.NotFound();
        
        return TypedResults.Redirect(link.fullUrl);
    }).WithName("ResolveShortLink");

// Create a new short link
app.MapPost("/", async Task<Results<BadRequest, CreatedAtRoute<Link>>>
    ([FromBody] string newUrl, LinksContext context) =>
    { 
        // add validation to input here
        string newId = UrlService.GenerateShortUrl(newUrl);
        Link newLink = new Link { fullUrl = newUrl, id = UrlService.GenerateShortUrl(newId) };
        // current method has a small chance for short hash / id collisions, should add checks.
        var created = await context.Links.AddAsync(newLink);
        return TypedResults.CreatedAtRoute(created.Entity, "ResolveShortLink", new { id = created.Entity.id }); 
    });

app.Run();