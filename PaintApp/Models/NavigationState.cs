using System;

namespace PaintApp.Models;

/// <summary>
/// Model representing the current navigation state of the application
/// </summary>
public class NavigationState
{
    /// <summary>
    /// Currently selected profile ID
    /// </summary>
    public int? ProfileId { get; set; }
    
    /// <summary>
    /// Currently opened canvas ID
    /// </summary>
    public int? CanvasId { get; set; }
    
    /// <summary>
    /// Current page type name
    /// </summary>
    public string? CurrentPage { get; set; }
    
    /// <summary>
    /// Timestamp of last navigation
    /// </summary>
    public DateTime LastNavigated { get; set; }
    
    /// <summary>
    /// Additional navigation parameters (JSON serialized)
    /// </summary>
    public string? AdditionalData { get; set; }
    
    public NavigationState()
    {
        LastNavigated = DateTime.Now;
    }
}
