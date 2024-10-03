using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalLinker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LinksContext>
    (o => o.UseSqlite(builder.Configuration.GetConnectionString("LinksContext")));

var app = builder.Build();

// need an endpoint that returns the link object itself
// need an endpoint that interprets and redirects to original link
// need an endpoint that accepts POSTs of new links to be minimized

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
    });

app.Run();