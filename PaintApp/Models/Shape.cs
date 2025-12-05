using System;

namespace PaintApp.Models;

public class Shape
{
    public int Id { get; set; }
    public int DrawingId { get; set; }
    public Drawing Drawing { get; set; } = null!;
    public string ShapeType { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string StrokeColor { get; set; } = "#000000";
    public double StrokeThickness { get; set; } = 2.0;
    public string FillColor { get; set; } = "#FFFFFF";
    public string? ShapeData { get; set; }
    public int ZIndex { get; set; }
    public DateTime CreatedAt { get; set; }
}
