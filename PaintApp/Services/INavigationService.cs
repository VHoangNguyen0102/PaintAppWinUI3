using System;
using System.Collections.Generic;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Interface for managing application navigation state
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the current navigation state
    /// </summary>
    NavigationState CurrentState { get; }
    
    /// <summary>
    /// Gets the navigation history
    /// </summary>
    IReadOnlyList<NavigationHistoryEntry> History { get; }
    
    /// <summary>
    /// Gets the currently selected profile ID
    /// </summary>
    int? CurrentProfileId { get; }
    
    /// <summary>
    /// Gets the currently opened canvas ID
    /// </summary>
    int? CurrentCanvasId { get; }
    
    /// <summary>
    /// Gets the current page name
    /// </summary>
    string? CurrentPage { get; }
    
    /// <summary>
    /// Sets the current profile ID
    /// </summary>
    /// <param name="profileId">The profile ID to set</param>
    void SetProfileId(int? profileId);
    
    /// <summary>
    /// Sets the current canvas ID
    /// </summary>
    /// <param name="canvasId">The canvas ID to set</param>
    void SetCanvasId(int? canvasId);
    
    /// <summary>
    /// Records a navigation event
    /// </summary>
    /// <param name="pageType">The page type navigated to</param>
    /// <param name="parameter">Optional navigation parameter</param>
    void RecordNavigation(string pageType, object? parameter = null);
    
    /// <summary>
    /// Clears the current canvas state
    /// </summary>
    void ClearCanvas();
    
    /// <summary>
    /// Clears the current profile state
    /// </summary>
    void ClearProfile();
    
    /// <summary>
    /// Clears all navigation state
    /// </summary>
    void ClearAll();
    
    /// <summary>
    /// Gets the last navigation entry for a specific page
    /// </summary>
    /// <param name="pageType">The page type to search for</param>
    /// <returns>The last navigation entry or null</returns>
    NavigationHistoryEntry? GetLastNavigationTo(string pageType);
    
    /// <summary>
    /// Checks if can navigate back
    /// </summary>
    /// <returns>True if there is history to go back to</returns>
    bool CanGoBack();
    
    /// <summary>
    /// Gets the previous navigation entry
    /// </summary>
    /// <returns>The previous entry or null</returns>
    NavigationHistoryEntry? GetPreviousNavigation();
    
    /// <summary>
    /// Event raised when navigation state changes
    /// </summary>
    event EventHandler<NavigationState>? NavigationStateChanged;
}
