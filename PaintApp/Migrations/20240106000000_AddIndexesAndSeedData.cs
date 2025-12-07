using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace PaintApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Templates_CreatedAt",
                table: "Templates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_IsPublic",
                table: "Templates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Name",
                table: "Templates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateShapes_TemplateId",
                table: "TemplateShapes",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_CreatedAt",
                table: "Shapes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_IsTemplate",
                table: "Shapes",
                column: "IsTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_Type",
                table: "Shapes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_CreatedAt",
                table: "Profiles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Name",
                table: "Profiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_CreatedAt",
                table: "Drawings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_Name",
                table: "Drawings",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_CreatedAt",
                table: "Canvases",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_Name",
                table: "Canvases",
                column: "Name");

            migrationBuilder.InsertData(
                table: "Canvases",
                columns: new[] { "Id", "BackgroundColor", "CreatedAt", "Height", "Name", "ProfileId", "UpdatedAt", "Width" },
                values: new object[,]
                {
                    { 1, "#FFFFFF", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 600, "My First Canvas", 1, null, 800 },
                    { 2, "#1E1E1E", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 768, "Dark Theme Canvas", 2, null, 1024 },
                    { 3, "#F5F5F5", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 480, "Quick Sketch", 1, null, 640 }
                });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "BackgroundColor", "CreatedAt", "Description", "Height", "IsPublic", "Name", "TemplateData", "ThumbnailPath", "UsageCount", "Width" },
                values: new object[,]
                {
                    { 1, "#FFFFFF", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A collection of basic geometric shapes", 600, true, "Basic Shapes Template", "{\"version\":\"1.0\",\"shapeCount\":3}", null, 10, 800 },
                    { 2, "#F0F0F0", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Template for creating flowcharts", 768, true, "Flowchart Template", "{\"version\":\"1.0\",\"shapeCount\":5}", null, 15, 1024 }
                });

            migrationBuilder.InsertData(
                table: "Shapes",
                columns: new[] { "Id", "CanvasId", "CreatedAt", "DrawingId", "FillColor", "GeometryData", "Height", "IsTemplate", "StrokeColor", "StrokeThickness", "Type", "UsageCount", "Width", "X", "Y", "ZIndex" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "#FF5733", "{\"X\":50,\"Y\":50,\"Width\":200,\"Height\":100}", 100.0, false, "#000000", 2.0, "Rectangle", 0, 200.0, 50.0, 50.0, 1 },
                    { 2, 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "#87CEEB", "{\"CenterX\":150,\"CenterY\":150,\"Radius\":50}", 100.0, false, "#0000FF", 3.0, "Circle", 0, 100.0, 100.0, 100.0, 2 },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "{\"X1\":10,\"Y1\":10,\"X2\":100,\"Y2\":100}", 90.0, true, "#FF0000", 2.0, "Line", 5, 90.0, 10.0, 10.0, 0 },
                    { 4, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "#90EE90", "{\"Points\":[{\"X\":50,\"Y\":0},{\"X\":100,\"Y\":100},{\"X\":0,\"Y\":100}]}", 100.0, true, "#00FF00", 2.5, "Triangle", 3, 100.0, 0.0, 0.0, 0 },
                    { 5, 2, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "#4B0082", "{\"X\":100,\"Y\":50,\"Width\":150,\"Height\":80}", 80.0, false, "#FFFFFF", 2.0, "Oval", 0, 150.0, 100.0, 50.0, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Canvases",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Shapes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Shapes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Shapes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Shapes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Shapes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Canvases",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Canvases",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropIndex(
                name: "IX_Templates_CreatedAt",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_IsPublic",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Name",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_TemplateShapes_TemplateId",
                table: "TemplateShapes");

            migrationBuilder.DropIndex(
                name: "IX_Shapes_CreatedAt",
                table: "Shapes");

            migrationBuilder.DropIndex(
                name: "IX_Shapes_IsTemplate",
                table: "Shapes");

            migrationBuilder.DropIndex(
                name: "IX_Shapes_Type",
                table: "Shapes");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_CreatedAt",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_Name",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_CreatedAt",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_Name",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_Canvases_CreatedAt",
                table: "Canvases");

            migrationBuilder.DropIndex(
                name: "IX_Canvases_Name",
                table: "Canvases");
        }
    }
}
