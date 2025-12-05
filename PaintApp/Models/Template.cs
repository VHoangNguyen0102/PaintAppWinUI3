using System;
using System.Collections.Generic;

namespace PaintApp.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string? ThumbnailPath { get; set; }
    public string? TemplateData { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public int UsageCount { get; set; }
    
    public ICollection<TemplateShape> TemplateShapes { get; set; } = new List<TemplateShape>();
}
