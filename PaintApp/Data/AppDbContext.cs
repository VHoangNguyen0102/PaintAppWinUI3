using Microsoft.EntityFrameworkCore;
using PaintApp.Models;

namespace PaintApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Profile> Profiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
