using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Service for managing application navigation state and history
/// </summary>
public class NavigationService : INavigationService
{
    private readonly NavigationState _currentState;
    private readonly List<NavigationHistoryEntry> _history;
    private const int MaxHistorySize = 50; // Limit history to prevent memory issues

    /// <summary>
    /// Event raised when navigation state changes
    /// </summary>
    public event EventHandler<NavigationState>? NavigationStateChanged;

    /// <summary>
    /// Gets the current navigation state
    /// </summary>
    public NavigationState CurrentState => _currentState;

    /// <summary>
    /// Gets the navigation history (read-only)
    /// </summary>
    public IReadOnlyList<NavigationHistoryEntry> History => _history.AsReadOnly();

    /// <summary>
    /// Gets the currently selected profile ID
    /// </summary>
    public int? CurrentProfileId => _currentState.ProfileId;

    /// <summary>
    /// Gets the currently opened canvas ID
    /// </summary>
    public int? CurrentCanvasId => _currentState.CanvasId;

    /// <summary>
    /// Gets the current page name
    /// </summary>
    public string? CurrentPage => _currentState.CurrentPage;

    public NavigationService()
    {
        _currentState = new NavigationState();
        _history = new List<NavigationHistoryEntry>();
        
        System.Diagnostics.Debug.WriteLine("NavigationService: Initialized");
    }

    /// <summary>
    /// Sets the current profile ID
    /// </summary>
    /// <param name="profileId">The profile ID to set</param>
    public void SetProfileId(int? profileId)
    {
        if (_currentState.ProfileId != profileId)
        {
            _currentState.ProfileId = profileId;
            _currentState.LastNavigated = DateTime.Now;
            
            System.Diagnostics.Debug.WriteLine($"NavigationService: ProfileId set to {profileId}");
            
            OnNavigationStateChanged();
        }
    }

    /// <summary>
    /// Sets the current canvas ID
    /// </summary>
    /// <param name="canvasId">The canvas ID to set</param>
    public void SetCanvasId(int? canvasId)
    {
        if (_currentState.CanvasId != canvasId)
        {
            _currentState.CanvasId = canvasId;
            _currentState.LastNavigated = DateTime.Now;
            
            System.Diagnostics.Debug.WriteLine($"NavigationService: CanvasId set to {canvasId}");
            
            OnNavigationStateChanged();
        }
    }

    /// <summary>
    /// Records a navigation event
    /// </summary>
    /// <param name="pageType">The page type navigated to</param>
    /// <param name="parameter">Optional navigation parameter</param>
    public void RecordNavigation(string pageType, object? parameter = null)
    {
        _currentState.CurrentPage = pageType;
        _currentState.LastNavigated = DateTime.Now;

        var entry = new NavigationHistoryEntry
        {
            PageType = pageType,
            ProfileId = _currentState.ProfileId,
            CanvasId = _currentState.CanvasId,
            Timestamp = DateTime.Now
        };

        // Serialize parameter if provided
        if (parameter != null)
        {
            try
            {
                entry.Parameter = JsonSerializer.Serialize(parameter, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationService: Failed to serialize parameter - {ex.Message}");
                entry.Parameter = parameter.ToString();
            }
        }

        _history.Add(entry);
        
        // Trim history if it exceeds max size
        while (_history.Count > MaxHistorySize)
        {
            _history.RemoveAt(0);
        }

        System.Diagnostics.Debug.WriteLine($"NavigationService: Recorded navigation to {pageType} (History: {_history.Count} entries)");
        System.Diagnostics.Debug.WriteLine($"  -> ProfileId: {entry.ProfileId}, CanvasId: {entry.CanvasId}");

        OnNavigationStateChanged();
    }

    /// <summary>
    /// Clears the current canvas state
    /// </summary>
    public void ClearCanvas()
    {
        if (_currentState.CanvasId.HasValue)
        {
            _currentState.CanvasId = null;
            _currentState.LastNavigated = DateTime.Now;
            
            System.Diagnostics.Debug.WriteLine("NavigationService: Canvas state cleared");
            
            OnNavigationStateChanged();
        }
    }

    /// <summary>
    /// Clears the current profile state
    /// </summary>
    public void ClearProfile()
    {
        if (_currentState.ProfileId.HasValue)
        {
            _currentState.ProfileId = null;
            _currentState.CanvasId = null; // Clear canvas too when profile is cleared
            _currentState.LastNavigated = DateTime.Now;
            
            System.Diagnostics.Debug.WriteLine("NavigationService: Profile state cleared");
            
            OnNavigationStateChanged();
        }
    }

    /// <summary>
    /// Clears all navigation state
    /// </summary>
    public void ClearAll()
    {
        _currentState.ProfileId = null;
        _currentState.CanvasId = null;
        _currentState.CurrentPage = null;
        _currentState.LastNavigated = DateTime.Now;
        _history.Clear();
        
        System.Diagnostics.Debug.WriteLine("NavigationService: All state cleared");
        
        OnNavigationStateChanged();
    }

    /// <summary>
    /// Gets the last navigation entry for a specific page
    /// </summary>
    /// <param name="pageType">The page type to search for</param>
    /// <returns>The last navigation entry or null</returns>
    public NavigationHistoryEntry? GetLastNavigationTo(string pageType)
    {
        return _history
            .Where(e => e.PageType == pageType)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefault();
    }

    /// <summary>
    /// Checks if can navigate back
    /// </summary>
    /// <returns>True if there is history to go back to</returns>
    public bool CanGoBack()
    {
        return _history.Count > 1;
    }

    /// <summary>
    /// Gets the previous navigation entry
    /// </summary>
    /// <returns>The previous entry or null</returns>
    public NavigationHistoryEntry? GetPreviousNavigation()
    {
        if (_history.Count < 2)
            return null;

        return _history[_history.Count - 2];
    }

    /// <summary>
    /// Raises the NavigationStateChanged event
    /// </summary>
    private void OnNavigationStateChanged()
    {
        NavigationStateChanged?.Invoke(this, _currentState);
    }

    /// <summary>
    /// Gets navigation statistics for debugging
    /// </summary>
    public string GetStatistics()
    {
        return $"Navigation Statistics:\n" +
               $"  Current Page: {_currentState.CurrentPage ?? "None"}\n" +
               $"  Profile ID: {_currentState.ProfileId?.ToString() ?? "None"}\n" +
               $"  Canvas ID: {_currentState.CanvasId?.ToString() ?? "None"}\n" +
               $"  History Entries: {_history.Count}\n" +
               $"  Last Navigated: {_currentState.LastNavigated:yyyy-MM-dd HH:mm:ss}";
    }
}
