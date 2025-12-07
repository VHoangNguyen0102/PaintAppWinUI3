using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PaintApp.Models;
using PaintApp.Services;
using PaintApp.Dialogs;

namespace PaintApp.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    private readonly IProfileService _profileService;
    private XamlRoot? _xamlRoot;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDrawingCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteProfileCommand))]
    private Profile? selectedProfile;

    [ObservableProperty]
    private bool canStartDrawing;

    [ObservableProperty]
    private bool isLoading;

    public HomePageViewModel(IProfileService profileService)
    {
        _profileService = profileService;
        _ = LoadProfilesAsync();
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    partial void OnSelectedProfileChanged(Profile? value)
    {
        CanStartDrawing = value != null;
    }

    private async Task LoadProfilesAsync()
    {
        try
        {
            IsLoading = true;
            var profilesList = await _profileService.GetAllProfilesAsync();
            
            Profiles.Clear();
            foreach (var profile in profilesList)
            {
                Profiles.Add(profile);
            }

            if (Profiles.Any() && SelectedProfile == null)
            {
                SelectedProfile = Profiles.First();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Load Profiles Error", $"Failed to load profiles: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        try
        {
            var dialog = new ProfileDialog
            {
                XamlRoot = _xamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Profile != null)
            {
                var createdProfile = await _profileService.CreateProfileAsync(dialog.Profile);
                Profiles.Add(createdProfile);
                SelectedProfile = createdProfile;

                await ShowSuccessDialogAsync("Success", "Profile created successfully!");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Create Profile Error", $"Failed to create profile: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditProfileAsync()
    {
        if (SelectedProfile == null) return;

        try
        {
            var dialog = new ProfileDialog(SelectedProfile)
            {
                XamlRoot = _xamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Profile != null)
            {
                var updatedProfile = await _profileService.UpdateProfileAsync(dialog.Profile);
                
                var index = Profiles.IndexOf(SelectedProfile);
                if (index >= 0)
                {
                    Profiles[index] = updatedProfile;
                    SelectedProfile = updatedProfile;
                }

                await ShowSuccessDialogAsync("Success", "Profile updated successfully!");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Edit Profile Error", $"Failed to update profile: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile == null) return;

        try
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete the profile '{SelectedProfile.Name}'?\nThis action cannot be undone.",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = _xamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var deleted = await _profileService.DeleteProfileAsync(SelectedProfile.Id);
                
                if (deleted)
                {
                    var profileToRemove = SelectedProfile;
                    SelectedProfile = null;
                    Profiles.Remove(profileToRemove);

                    if (Profiles.Any())
                    {
                        SelectedProfile = Profiles.First();
                    }

                    await ShowSuccessDialogAsync("Success", "Profile deleted successfully!");
                }
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Delete Profile Error", $"Failed to delete profile: {ex.Message}");
        }
    }

    private bool CanEditOrDelete() => SelectedProfile != null;

    [RelayCommand(CanExecute = nameof(CanStartDrawing))]
    private async Task StartDrawingAsync()
    {
        if (SelectedProfile == null)
        {
            await ShowErrorDialogAsync("Error", "Please select a profile first.");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"HomePageViewModel: StartDrawing command executed for profile '{SelectedProfile.Name}'");
        
        // Command execution logged - actual navigation is handled in code-behind
        // because Frame navigation requires UI context
    }

    private async Task ShowSuccessDialogAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    private async Task ShowErrorDialogAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }
}
