using System;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Service for managing the currently selected profile across the application
/// </summary>
public class ProfileManager : IProfileManager
{
    private Profile? _currentProfile;

    /// <summary>
    /// Gets the currently selected profile
    /// </summary>
    public Profile? CurrentProfile => _currentProfile;

    /// <summary>
    /// Event raised when the current profile changes
    /// </summary>
    public event EventHandler<Profile?>? CurrentProfileChanged;

    /// <summary>
    /// Sets the currently selected profile
    /// </summary>
    /// <param name="profile">The profile to set as current</param>
    public void SetCurrentProfile(Profile? profile)
    {
        if (_currentProfile?.Id != profile?.Id)
        {
            _currentProfile = profile;
            
            System.Diagnostics.Debug.WriteLine(_currentProfile != null 
                ? $"ProfileManager: Current profile set to '{_currentProfile.Name}' (ID: {_currentProfile.Id})"
                : "ProfileManager: Current profile cleared");
            
            CurrentProfileChanged?.Invoke(this, _currentProfile);
        }
    }

    /// <summary>
    /// Clears the current profile
    /// </summary>
    public void ClearCurrentProfile()
    {
        SetCurrentProfile(null);
    }

    /// <summary>
    /// Checks if a profile is currently selected
    /// </summary>
    /// <returns>True if a profile is selected, false otherwise</returns>
    public bool HasProfile()
    {
        return _currentProfile != null;
    }
}
