using Microsoft.EntityFrameworkCore;

namespace EduGestor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Placeholder — entity configurations added by Specs 2-10
    }
}
