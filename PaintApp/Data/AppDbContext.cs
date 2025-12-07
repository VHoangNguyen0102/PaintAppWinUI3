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
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatedAt);
            
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
            
            entity.HasIndex(e => e.ProfileId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Name);
            
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
            entity.Property(e => e.Width).HasDefaultValue(800);
            entity.Property(e => e.Height).HasDefaultValue(600);
            entity.Property(e => e.BackgroundColor).HasDefaultValue("#FFFFFF");
            
            entity.HasIndex(e => e.ProfileId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Name);
            
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
            
            entity.HasIndex(e => e.CanvasId);
            entity.HasIndex(e => e.DrawingId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsTemplate);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasMany(e => e.TemplateShapes)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateShape>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShapeType).IsRequired();
            
            entity.HasIndex(e => e.TemplateId);
        });

        modelBuilder.Entity<Profile>().HasData(
            new Profile
            {
                Id = 1,
                Name = "Default Profile",
                AvatarPath = null,
                CreatedAt = new DateTime(2024, 1, 1),
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
                CreatedAt = new DateTime(2024, 1, 1),
                Theme = "Dark",
                DefaultCanvasWidth = 1024,
                DefaultCanvasHeight = 768,
                DefaultCanvasBackgroundColor = "#1E1E1E",
                DefaultStrokeThickness = 3.0,
                DefaultStrokeColor = "#FFFFFF",
                DefaultFillColor = "#333333"
            }
        );

        modelBuilder.Entity<Canvas>().HasData(
            new Canvas
            {
                Id = 1,
                Name = "My First Canvas",
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF",
                ProfileId = 1,
                CreatedAt = new DateTime(2024, 1, 2),
                UpdatedAt = null
            },
            new Canvas
            {
                Id = 2,
                Name = "Dark Theme Canvas",
                Width = 1024,
                Height = 768,
                BackgroundColor = "#1E1E1E",
                ProfileId = 2,
                CreatedAt = new DateTime(2024, 1, 2),
                UpdatedAt = null
            },
            new Canvas
            {
                Id = 3,
                Name = "Quick Sketch",
                Width = 640,
                Height = 480,
                BackgroundColor = "#F5F5F5",
                ProfileId = 1,
                CreatedAt = new DateTime(2024, 1, 3),
                UpdatedAt = null
            }
        );

        modelBuilder.Entity<Shape>().HasData(
            new Shape
            {
                Id = 1,
                CanvasId = 1,
                DrawingId = null,
                Type = "Rectangle",
                GeometryData = "{\"X\":50,\"Y\":50,\"Width\":200,\"Height\":100}",
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "#FF5733",
                IsTemplate = false,
                CreatedAt = new DateTime(2024, 1, 2),
                UsageCount = 0,
                X = 50,
                Y = 50,
                Width = 200,
                Height = 100,
                ZIndex = 1
            },
            new Shape
            {
                Id = 2,
                CanvasId = 1,
                DrawingId = null,
                Type = "Circle",
                GeometryData = "{\"CenterX\":150,\"CenterY\":150,\"Radius\":50}",
                StrokeColor = "#0000FF",
                StrokeThickness = 3.0,
                FillColor = "#87CEEB",
                IsTemplate = false,
                CreatedAt = new DateTime(2024, 1, 2),
                UsageCount = 0,
                X = 100,
                Y = 100,
                Width = 100,
                Height = 100,
                ZIndex = 2
            },
            new Shape
            {
                Id = 3,
                CanvasId = null,
                DrawingId = null,
                Type = "Line",
                GeometryData = "{\"X1\":10,\"Y1\":10,\"X2\":100,\"Y2\":100}",
                StrokeColor = "#FF0000",
                StrokeThickness = 2.0,
                FillColor = null,
                IsTemplate = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UsageCount = 5,
                X = 10,
                Y = 10,
                Width = 90,
                Height = 90,
                ZIndex = 0
            },
            new Shape
            {
                Id = 4,
                CanvasId = null,
                DrawingId = null,
                Type = "Triangle",
                GeometryData = "{\"Points\":[{\"X\":50,\"Y\":0},{\"X\":100,\"Y\":100},{\"X\":0,\"Y\":100}]}",
                StrokeColor = "#00FF00",
                StrokeThickness = 2.5,
                FillColor = "#90EE90",
                IsTemplate = true,
                CreatedAt = new DateTime(2024, 1, 1),
                UsageCount = 3,
                X = 0,
                Y = 0,
                Width = 100,
                Height = 100,
                ZIndex = 0
            },
            new Shape
            {
                Id = 5,
                CanvasId = 2,
                DrawingId = null,
                Type = "Oval",
                GeometryData = "{\"X\":100,\"Y\":50,\"Width\":150,\"Height\":80}",
                StrokeColor = "#FFFFFF",
                StrokeThickness = 2.0,
                FillColor = "#4B0082",
                IsTemplate = false,
                CreatedAt = new DateTime(2024, 1, 2),
                UsageCount = 0,
                X = 100,
                Y = 50,
                Width = 150,
                Height = 80,
                ZIndex = 1
            }
        );

        modelBuilder.Entity<Template>().HasData(
            new Template
            {
                Id = 1,
                Name = "Basic Shapes Template",
                Description = "A collection of basic geometric shapes",
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF",
                ThumbnailPath = null,
                TemplateData = "{\"version\":\"1.0\",\"shapeCount\":3}",
                CreatedAt = new DateTime(2024, 1, 1),
                IsPublic = true,
                UsageCount = 10
            },
            new Template
            {
                Id = 2,
                Name = "Flowchart Template",
                Description = "Template for creating flowcharts",
                Width = 1024,
                Height = 768,
                BackgroundColor = "#F0F0F0",
                ThumbnailPath = null,
                TemplateData = "{\"version\":\"1.0\",\"shapeCount\":5}",
                CreatedAt = new DateTime(2024, 1, 1),
                IsPublic = true,
                UsageCount = 15
            }
        );
    }
}
