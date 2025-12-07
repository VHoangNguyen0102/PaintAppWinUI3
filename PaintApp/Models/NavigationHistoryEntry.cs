using System;

namespace PaintApp.Models;

/// <summary>
/// Represents a single entry in the navigation history
/// </summary>
public class NavigationHistoryEntry
{
    /// <summary>
    /// Unique identifier for this history entry
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Page type that was navigated to
    /// </summary>
    public string PageType { get; set; } = string.Empty;
    
    /// <summary>
    /// Profile ID at time of navigation
    /// </summary>
    public int? ProfileId { get; set; }
    
    /// <summary>
    /// Canvas ID at time of navigation (if applicable)
    /// </summary>
    public int? CanvasId { get; set; }
    
    /// <summary>
    /// Timestamp when navigation occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Navigation parameter (serialized JSON)
    /// </summary>
    public string? Parameter { get; set; }
    
    public NavigationHistoryEntry()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.Now;
    }
    
    public override string ToString()
    {
        return $"{PageType} at {Timestamp:HH:mm:ss} (Profile: {ProfileId}, Canvas: {CanvasId})";
    }
}
