using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalLinker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LinksContext>
    (o => o.UseSqlite(builder.Configuration.GetConnectionString("LinksConnection")));

var app = builder.Build();

app.MapGet("/{id}", async Task<Results<NotFound, Ok<Link>>>
    ([FromRoute] string id, LinksContext context) =>
    {
        Link? link = await context.Links.FindAsync(id);

        if (link == null)
        {
            return TypedResults.NotFound();
        }
        
        return TypedResults.Ok<Link>(link);
    });

app.Run();