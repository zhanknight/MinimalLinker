using Microsoft.EntityFrameworkCore;

namespace MinimalLinker;

public class LinksContext : DbContext
{
    public LinksContext(DbContextOptions<LinksContext> options) : base(options) { }
    
    public DbSet<Link> Links { get; set; }
    
}