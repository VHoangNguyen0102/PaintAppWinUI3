using Microsoft.EntityFrameworkCore;
using PaintApp.Models;

namespace PaintApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=paintapp.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
