using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace PaintApp.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProfileAndAddModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultCanvasBackgroundColor",
                table: "Profiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "#FFFFFF");

            migrationBuilder.AddColumn<int>(
                name: "DefaultCanvasHeight",
                table: "Profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AddColumn<int>(
                name: "DefaultCanvasWidth",
                table: "Profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 800);

            migrationBuilder.AddColumn<string>(
                name: "DefaultFillColor",
                table: "Profiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "#FFFFFF");

            migrationBuilder.AddColumn<string>(
                name: "DefaultStrokeColor",
                table: "Profiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "#000000");

            migrationBuilder.AddColumn<double>(
                name: "DefaultStrokeThickness",
                table: "Profiles",
                type: "REAL",
                nullable: false,
                defaultValue: 2.0);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Profiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Drawings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drawings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drawings_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "TEXT", nullable: true),
                    TemplateData = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shapes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DrawingId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShapeType = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    StrokeColor = table.Column<string>(type: "TEXT", nullable: false),
                    StrokeThickness = table.Column<double>(type: "REAL", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", nullable: false),
                    ShapeData = table.Column<string>(type: "TEXT", nullable: true),
                    ZIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shapes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shapes_Drawings_DrawingId",
                        column: x => x.DrawingId,
                        principalTable: "Drawings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateShapes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShapeType = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    StrokeColor = table.Column<string>(type: "TEXT", nullable: false),
                    StrokeThickness = table.Column<double>(type: "REAL", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", nullable: false),
                    ShapeData = table.Column<string>(type: "TEXT", nullable: true),
                    ZIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateShapes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateShapes_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DefaultCanvasBackgroundColor", "DefaultCanvasHeight", "DefaultCanvasWidth", "DefaultFillColor", "DefaultStrokeColor", "DefaultStrokeThickness", "Theme" },
                values: new object[] { "#FFFFFF", 600, 800, "#FFFFFF", "#000000", 2.0, "Light" });

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DefaultCanvasBackgroundColor", "DefaultCanvasHeight", "DefaultCanvasWidth", "DefaultFillColor", "DefaultStrokeColor", "DefaultStrokeThickness", "Theme" },
                values: new object[] { "#1E1E1E", 768, 1024, "#333333", "#FFFFFF", 3.0, "Dark" });

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ProfileId",
                table: "Drawings",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_DrawingId",
                table: "Shapes",
                column: "DrawingId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateShapes_TemplateId",
                table: "TemplateShapes",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shapes");

            migrationBuilder.DropTable(
                name: "TemplateShapes");

            migrationBuilder.DropTable(
                name: "Drawings");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropColumn(
                name: "DefaultCanvasBackgroundColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DefaultCanvasHeight",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DefaultCanvasWidth",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DefaultFillColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DefaultStrokeColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "DefaultStrokeThickness",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Profiles");
        }
    }
}
