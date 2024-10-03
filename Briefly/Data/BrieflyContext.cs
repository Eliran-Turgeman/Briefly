using Microsoft.EntityFrameworkCore;

namespace Briefly.Data;

public class BrieflyContext : DbContext
{
    public BrieflyContext (DbContextOptions<BrieflyContext> options)
        : base(options)
    {
    }

    public DbSet<BlogPost> BlogPost { get; set; } = default!;
    public DbSet<PublishedMessage> PublishedMessages { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>()
            .Property(b => b.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
