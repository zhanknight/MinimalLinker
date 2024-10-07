using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MinimalLinker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LinksContext>
    (o => o.UseSqlite(builder.Configuration.GetConnectionString("LinksContext")));
builder.Services.AddScoped<UrlService>();   
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x => 
    { 
        x.Authority = "https://localhost"; 
        x.Audience = "MinimalLinker";
    });
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireCreate", policy => policy.RequireClaim("permission", "create"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("jwt",
        new()
        {
            Name = "Authorization",
            Description = "JWT Bearer Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header
        });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "jwt"
                }
            },
            new string[] { }
        }
    });
});

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
    ([FromBody] string incomingUrl, LinksContext context, UrlService urlService) =>
    {
        string newId = await urlService.GenerateShortUrl(incomingUrl);
        Link newLink = new Link { fullUrl = incomingUrl, id = newId };
        var created = await context.Links.AddAsync(newLink);
        return TypedResults.CreatedAtRoute(created.Entity, "ResolveShortLink", new { id = created.Entity.id }); 
        
    }).RequireAuthorization("RequireCreate");

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();