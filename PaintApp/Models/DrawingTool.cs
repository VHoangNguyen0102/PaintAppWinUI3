namespace PaintApp.Models;

/// <summary>
/// Enum ??nh ngh?a các công c? v? có s?n trong ?ng d?ng
/// </summary>
public enum DrawingTool
{
    /// <summary>
    /// Không có công c? nào ???c ch?n
    /// </summary>
    None,
    
    /// <summary>
    /// Công c? v? ???ng th?ng
    /// </summary>
    Line,
    
    /// <summary>
    /// Công c? v? hình ch? nh?t
    /// </summary>
    Rectangle,
    
    /// <summary>
    /// Công c? v? hình ellipse (oval)
    /// </summary>
    Oval,
    
    /// <summary>
    /// Công c? v? hình tròn
    /// </summary>
    Circle,
    
    /// <summary>
    /// Công c? v? tam giác
    /// </summary>
    Triangle,
    
    /// <summary>
    /// Công c? v? ?a giác
    /// </summary>
    Polygon,
    
    /// <summary>
    /// Công c? ch?n và di chuy?n shapes
    /// </summary>
    Select
}
