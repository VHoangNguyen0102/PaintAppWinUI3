using System;

namespace PaintApp.Models;

public class Shape
{
    public int Id { get; set; }
    public int? DrawingId { get; set; }
    public Drawing? Drawing { get; set; }
    public int? CanvasId { get; set; }
    public Canvas? Canvas { get; set; }
    
    public string Type { get; set; } = string.Empty;
    public string GeometryData { get; set; } = string.Empty;
    public string StrokeColor { get; set; } = "#000000";
    public double StrokeThickness { get; set; } = 2.0;
    public string? FillColor { get; set; }
    
    public bool IsTemplate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UsageCount { get; set; }
    
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int ZIndex { get; set; }
}
