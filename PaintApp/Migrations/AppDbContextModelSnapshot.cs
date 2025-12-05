using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaintApp.Data;

#nullable disable

namespace PaintApp.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("PaintApp.Models.Canvas", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackgroundColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfileId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ProfileId");

                    b.ToTable("Canvases");
                });

            modelBuilder.Entity("PaintApp.Models.Drawing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackgroundColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<string>("FilePath")
                        .HasColumnType("TEXT");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastModifiedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfileId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ThumbnailPath")
                        .HasColumnType("TEXT");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ProfileId");

                    b.ToTable("Drawings");
                });

            modelBuilder.Entity("PaintApp.Models.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AvatarPath")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<string>("DefaultCanvasBackgroundColor")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("#FFFFFF");

                    b.Property<int>("DefaultCanvasHeight")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(600);

                    b.Property<int>("DefaultCanvasWidth")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(800);

                    b.Property<string>("DefaultFillColor")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("#FFFFFF");

                    b.Property<string>("DefaultStrokeColor")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("#000000");

                    b.Property<double>("DefaultStrokeThickness")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("REAL")
                        .HasDefaultValue(2.0);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Theme")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Profiles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            DefaultCanvasBackgroundColor = "#FFFFFF",
                            DefaultCanvasHeight = 600,
                            DefaultCanvasWidth = 800,
                            DefaultFillColor = "#FFFFFF",
                            DefaultStrokeColor = "#000000",
                            DefaultStrokeThickness = 2.0,
                            Name = "Default Profile",
                            Theme = "Light"
                        },
                        new
                        {
                            Id = 2,
                            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            DefaultCanvasBackgroundColor = "#1E1E1E",
                            DefaultCanvasHeight = 768,
                            DefaultCanvasWidth = 1024,
                            DefaultFillColor = "#333333",
                            DefaultStrokeColor = "#FFFFFF",
                            DefaultStrokeThickness = 3.0,
                            Name = "Guest User",
                            Theme = "Dark"
                        });
                });

            modelBuilder.Entity("PaintApp.Models.Shape", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CanvasId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<int?>("DrawingId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FillColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Height")
                        .HasColumnType("REAL");

                    b.Property<string>("ShapeData")
                        .HasColumnType("TEXT");

                    b.Property<string>("ShapeType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("StrokeColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("StrokeThickness")
                        .HasColumnType("REAL");

                    b.Property<double>("Width")
                        .HasColumnType("REAL");

                    b.Property<double>("X")
                        .HasColumnType("REAL");

                    b.Property<double>("Y")
                        .HasColumnType("REAL");

                    b.Property<int>("ZIndex")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CanvasId");

                    b.HasIndex("DrawingId");

                    b.ToTable("Shapes");
                });

            modelBuilder.Entity("PaintApp.Models.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackgroundColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TemplateData")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailPath")
                        .HasColumnType("TEXT");

                    b.Property<int>("UsageCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("PaintApp.Models.TemplateShape", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FillColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Height")
                        .HasColumnType("REAL");

                    b.Property<string>("ShapeData")
                        .HasColumnType("TEXT");

                    b.Property<string>("ShapeType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("StrokeColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("StrokeThickness")
                        .HasColumnType("REAL");

                    b.Property<int>("TemplateId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Width")
                        .HasColumnType("REAL");

                    b.Property<double>("X")
                        .HasColumnType("REAL");

                    b.Property<double>("Y")
                        .HasColumnType("REAL");

                    b.Property<int>("ZIndex")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TemplateId");

                    b.ToTable("TemplateShapes");
                });

            modelBuilder.Entity("PaintApp.Models.Canvas", b =>
                {
                    b.HasOne("PaintApp.Models.Profile", "Profile")
                        .WithMany("Canvases")
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("PaintApp.Models.Drawing", b =>
                {
                    b.HasOne("PaintApp.Models.Profile", "Profile")
                        .WithMany("Drawings")
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("PaintApp.Models.Shape", b =>
                {
                    b.HasOne("PaintApp.Models.Canvas", "Canvas")
                        .WithMany("Shapes")
                        .HasForeignKey("CanvasId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaintApp.Models.Drawing", "Drawing")
                        .WithMany("Shapes")
                        .HasForeignKey("DrawingId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Canvas");

                    b.Navigation("Drawing");
                });

            modelBuilder.Entity("PaintApp.Models.TemplateShape", b =>
                {
                    b.HasOne("PaintApp.Models.Template", "Template")
                        .WithMany("TemplateShapes")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");
                });

            modelBuilder.Entity("PaintApp.Models.Canvas", b =>
                {
                    b.Navigation("Shapes");
                });

            modelBuilder.Entity("PaintApp.Models.Drawing", b =>
                {
                    b.Navigation("Shapes");
                });

            modelBuilder.Entity("PaintApp.Models.Profile", b =>
                {
                    b.Navigation("Canvases");

                    b.Navigation("Drawings");
                });

            modelBuilder.Entity("PaintApp.Models.Template", b =>
                {
                    b.Navigation("TemplateShapes");
                });
#pragma warning restore 612, 618
        }
    }
}
