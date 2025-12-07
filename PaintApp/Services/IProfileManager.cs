using System;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Interface for managing the currently selected profile across the application
/// </summary>
public interface IProfileManager
{
    /// <summary>
    /// Gets the currently selected profile
    /// </summary>
    Profile? CurrentProfile { get; }
    
    /// <summary>
    /// Sets the currently selected profile
    /// </summary>
    /// <param name="profile">The profile to set as current</param>
    void SetCurrentProfile(Profile? profile);
    
    /// <summary>
    /// Clears the current profile
    /// </summary>
    void ClearCurrentProfile();
    
    /// <summary>
    /// Checks if a profile is currently selected
    /// </summary>
    /// <returns>True if a profile is selected, false otherwise</returns>
    bool HasProfile();
    
    /// <summary>
    /// Event raised when the current profile changes
    /// </summary>
    event EventHandler<Profile?>? CurrentProfileChanged;
}
