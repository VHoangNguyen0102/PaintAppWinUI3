using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCanvasModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DrawingId",
                table: "Shapes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "CanvasId",
                table: "Shapes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Canvases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canvases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Canvases_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_CanvasId",
                table: "Shapes",
                column: "CanvasId");

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_ProfileId",
                table: "Canvases",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shapes_Canvases_CanvasId",
                table: "Shapes",
                column: "CanvasId",
                principalTable: "Canvases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shapes_Canvases_CanvasId",
                table: "Shapes");

            migrationBuilder.DropTable(
                name: "Canvases");

            migrationBuilder.DropIndex(
                name: "IX_Shapes_CanvasId",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "CanvasId",
                table: "Shapes");

            migrationBuilder.AlterColumn<int>(
                name: "DrawingId",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
