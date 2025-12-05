using System;
using Microsoft.EntityFrameworkCore;
using PaintApp.Models;

namespace PaintApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Drawing> Drawings { get; set; }
    public DbSet<Canvas> Canvases { get; set; }
    public DbSet<Shape> Shapes { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<TemplateShape> TemplateShapes { get; set; }

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
            entity.Property(e => e.DefaultCanvasWidth).HasDefaultValue(800);
            entity.Property(e => e.DefaultCanvasHeight).HasDefaultValue(600);
            entity.Property(e => e.DefaultCanvasBackgroundColor).HasDefaultValue("#FFFFFF");
            entity.Property(e => e.DefaultStrokeThickness).HasDefaultValue(2.0);
            entity.Property(e => e.DefaultStrokeColor).HasDefaultValue("#000000");
            entity.Property(e => e.DefaultFillColor).HasDefaultValue("#FFFFFF");
            
            entity.HasMany(e => e.Drawings)
                .WithOne(e => e.Profile)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Canvases)
                .WithOne(e => e.Profile)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Drawing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("datetime('now')");
            
            entity.HasMany(e => e.Shapes)
                .WithOne(e => e.Drawing)
                .HasForeignKey(e => e.DrawingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Canvas>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            entity.HasMany(e => e.Shapes)
                .WithOne(e => e.Canvas)
                .HasForeignKey(e => e.CanvasId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Shape>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.GeometryData).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.IsTemplate).HasDefaultValue(false);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            entity.HasMany(e => e.TemplateShapes)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateShape>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShapeType).IsRequired();
        });

        modelBuilder.Entity<Profile>().HasData(
            new Profile
            {
                Id = 1,
                Name = "Default Profile",
                AvatarPath = null,
                CreatedAt = DateTime.Now,
                Theme = "Light",
                DefaultCanvasWidth = 800,
                DefaultCanvasHeight = 600,
                DefaultCanvasBackgroundColor = "#FFFFFF",
                DefaultStrokeThickness = 2.0,
                DefaultStrokeColor = "#000000",
                DefaultFillColor = "#FFFFFF"
            },
            new Profile
            {
                Id = 2,
                Name = "Guest User",
                AvatarPath = null,
                CreatedAt = DateTime.Now,
                Theme = "Dark",
                DefaultCanvasWidth = 1024,
                DefaultCanvasHeight = 768,
                DefaultCanvasBackgroundColor = "#1E1E1E",
                DefaultStrokeThickness = 3.0,
                DefaultStrokeColor = "#FFFFFF",
                DefaultFillColor = "#333333"
            }
        );
    }
}
