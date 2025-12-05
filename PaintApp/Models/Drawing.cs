using System;
using System.Collections.Generic;

namespace PaintApp.Models;

public class Drawing
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string? ThumbnailPath { get; set; }
    public string? FilePath { get; set; }
    
    public ICollection<Shape> Shapes { get; set; } = new List<Shape>();
}
