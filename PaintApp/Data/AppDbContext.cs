using System;
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

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<Profile>().HasData(
            new Profile
            {
                Id = 1,
                Name = "Default Profile",
                AvatarPath = null,
                CreatedAt = DateTime.Now
            },
            new Profile
            {
                Id = 2,
                Name = "Guest User",
                AvatarPath = null,
                CreatedAt = DateTime.Now
            }
        );
    }
}
