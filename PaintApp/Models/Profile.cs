using System;
using System.Collections.Generic;

namespace PaintApp.Models;

public class Profile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string? Theme { get; set; }
    public int DefaultCanvasWidth { get; set; } = 800;
    public int DefaultCanvasHeight { get; set; } = 600;
    public string DefaultCanvasBackgroundColor { get; set; } = "#FFFFFF";
    public double DefaultStrokeThickness { get; set; } = 2.0;
    public string DefaultStrokeColor { get; set; } = "#000000";
    public string DefaultFillColor { get; set; } = "#FFFFFF";
    
    public ICollection<Drawing> Drawings { get; set; } = new List<Drawing>();
}
