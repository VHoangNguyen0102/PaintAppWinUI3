using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShapeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShapeType",
                table: "Shapes",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ShapeData",
                table: "Shapes",
                newName: "GeometryData");

            migrationBuilder.AlterColumn<string>(
                name: "GeometryData",
                table: "Shapes",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FillColor",
                table: "Shapes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "Shapes");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Shapes",
                newName: "ShapeType");

            migrationBuilder.RenameColumn(
                name: "GeometryData",
                table: "Shapes",
                newName: "ShapeData");

            migrationBuilder.AlterColumn<string>(
                name: "ShapeData",
                table: "Shapes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "FillColor",
                table: "Shapes",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
