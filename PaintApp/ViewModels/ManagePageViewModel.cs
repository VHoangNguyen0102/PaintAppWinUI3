using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaintApp.Models;
using PaintApp.Data;
using PaintApp.Services;
using Microsoft.EntityFrameworkCore;

namespace PaintApp.ViewModels;

public partial class ManagePageViewModel : ViewModelBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICanvasService _canvasService;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    private ObservableCollection<Canvas> canvases = new();

    [ObservableProperty]
    private Profile? selectedProfile;

    [ObservableProperty]
    private int totalProfiles;

    [ObservableProperty]
    private int totalDrawings;

    [ObservableProperty]
    private int totalTemplates;

    [ObservableProperty]
    private int totalCanvases;

    [ObservableProperty]
    private int selectedTabIndex;

    [ObservableProperty]
    private string currentBreadcrumb = "Dashboard";

    [ObservableProperty]
    private bool isLoadingCanvases;

    [ObservableProperty]
    private Microsoft.UI.Xaml.XamlRoot? xamlRoot;

    public ManagePageViewModel(AppDbContext dbContext, ICanvasService canvasService)
    {
        _dbContext = dbContext;
        _canvasService = canvasService;
        _ = LoadDataAsync();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        CurrentBreadcrumb = value switch
        {
            0 => "Dashboard",
            1 => "Canvases",
            2 => "Manage Drawings",
            3 => "Manage Templates",
            _ => "Dashboard"
        };
    }

    partial void OnSelectedProfileChanged(Profile? value)
    {
        if (value != null)
        {
            _ = LoadCanvasesForProfileAsync(value.Id);
        }
    }

    private async Task LoadDataAsync()
    {
        var profilesList = await _dbContext.Profiles.ToListAsync();
        Profiles.Clear();
        foreach (var profile in profilesList)
        {
            Profiles.Add(profile);
        }

        TotalProfiles = profilesList.Count;
        TotalDrawings = 0;
        TotalTemplates = 0;
        
        // Load total canvases
        TotalCanvases = await _dbContext.Canvases.CountAsync();
        
        // Select first profile by default
        if (Profiles.Count > 0)
        {
            SelectedProfile = Profiles[0];
        }
    }

    private async Task LoadCanvasesForProfileAsync(int profileId)
    {
        IsLoadingCanvases = true;
        try
        {
            var canvasesList = await _canvasService.GetCanvasesByProfileIdAsync(profileId);
            
            Canvases.Clear();
            foreach (var canvas in canvasesList)
            {
                Canvases.Add(canvas);
            }
        }
        finally
        {
            IsLoadingCanvases = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RefreshCanvasesAsync()
    {
        if (SelectedProfile != null)
        {
            await LoadCanvasesForProfileAsync(SelectedProfile.Id);
        }
    }

    [RelayCommand]
    private void AddNewProfile()
    {
    }

    [RelayCommand]
    private void DeleteProfile(Profile profile)
    {
    }

    [RelayCommand]
    private async Task DeleteCanvasAsync(Canvas canvas)
    {
        if (canvas == null || XamlRoot == null)
            return;

        // Count shapes in canvas (if available)
        var shapeCount = canvas.Shapes?.Count ?? 0;
        var shapeText = shapeCount > 0 
            ? $"\n\nThis canvas contains {shapeCount} shape(s) that will also be deleted." 
            : "";

        // Show confirmation dialog
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Delete Canvas?",
            Content = $"Are you sure you want to delete '{canvas.Name}'?{shapeText}\n\n?? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            IsLoadingCanvases = true;
            try
            {
                // Delete canvas (cascade will delete all shapes)
                var deleted = await _canvasService.DeleteCanvasAsync(canvas.Id);

                if (deleted)
                {
                    // Remove from collection
                    Canvases.Remove(canvas);
                    
                    // Update total count
                    TotalCanvases--;

                    // Show success message
                    var successMessage = shapeCount > 0
                        ? $"Canvas '{canvas.Name}' and {shapeCount} shape(s) have been deleted successfully."
                        : $"Canvas '{canvas.Name}' has been deleted successfully.";

                    await ShowSuccessDialogAsync("Canvas Deleted", successMessage);
                }
                else
                {
                    await ShowErrorDialogAsync("Delete Failed", 
                        "Canvas not found or could not be deleted.");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Delete Error", 
                    $"An error occurred while deleting the canvas:\n\n{ex.Message}");
            }
            finally
            {
                IsLoadingCanvases = false;
            }
        }
    }

    public void SetXamlRoot(Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        XamlRoot = xamlRoot;
    }

    private async Task ShowSuccessDialogAsync(string title, string message)
    {
        if (XamlRoot == null) return;

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async Task ShowErrorDialogAsync(string title, string message)
    {
        if (XamlRoot == null) return;

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
