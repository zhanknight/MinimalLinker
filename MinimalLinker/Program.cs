using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MinimalLinker;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LinksContext>
    (o => o.UseSqlite(builder.Configuration.GetConnectionString("LinksContext")));

builder.Services.AddScoped<UrlService>();  
builder.Services.AddMemoryCache(o => { o.SizeLimit = 64; o.CompactionPercentage = .5; }); 

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
    ([FromRoute] int id, UrlService urlService) =>
    {
        Link? link = await urlService.GetLinkAsync(id);
        if (link == null) return TypedResults.NotFound();
        return TypedResults.Ok<Link>(link);
    });

// Redirect from short link to original url
app.MapGet("/{id}", async Task<Results<NotFound, RedirectHttpResult>>
    ([FromRoute] int id, UrlService urlService) =>
    {
        Link? link = await urlService.GetLinkAsync(id);
        if (link == null) return TypedResults.NotFound();
        return TypedResults.Redirect(link.fullUrl);
    }).WithName("ResolveShortLink");

// Create a new short link
app.MapPost("/", async Task<Results<BadRequest, CreatedAtRoute<Link>>>
    ([FromBody] string incomingUrl, UrlService urlService) =>
    {
        var created = await urlService.CreateLinkAsync(incomingUrl);
        if (created == null) return TypedResults.BadRequest();
        return TypedResults.CreatedAtRoute(created, "ResolveShortLink", new { id = created.id }); 
    }).RequireAuthorization("RequireCreate");

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();