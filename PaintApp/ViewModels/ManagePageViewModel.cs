using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IShapeService _shapeService;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    private ObservableCollection<Canvas> canvases = new();

    [ObservableProperty]
    private ObservableCollection<Shape> templateShapes = new();

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
    private bool isLoadingTemplates;

    [ObservableProperty]
    private Microsoft.UI.Xaml.XamlRoot? xamlRoot;

    public ManagePageViewModel(AppDbContext dbContext, ICanvasService canvasService, IShapeService shapeService)
    {
        _dbContext = dbContext;
        _canvasService = canvasService;
        _shapeService = shapeService;
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
        
        // Load templates
        await LoadTemplatesAsync();
        TotalTemplates = TemplateShapes.Count;
        
        // Load total canvases
        TotalCanvases = await _dbContext.Canvases.CountAsync();
        
        // Select first profile by default
        if (Profiles.Count > 0)
        {
            SelectedProfile = Profiles[0];
        }
    }

    private async Task LoadTemplatesAsync()
    {
        IsLoadingTemplates = true;
        try
        {
            System.Diagnostics.Debug.WriteLine("ManagePage: Loading templates...");
            
            var templates = await _shapeService.GetTemplateShapesAsync();
            
            System.Diagnostics.Debug.WriteLine($"ManagePage: Loaded {templates.Count} templates");
            
            // Remove duplicates
            var uniqueTemplates = templates
                .GroupBy(t => new { t.Type, t.GeometryData, t.StrokeColor, t.FillColor, t.StrokeThickness })
                .Select(g => g.OrderByDescending(t => t.UsageCount).ThenBy(t => t.Id).First())
                .ToList();
            
            TemplateShapes.Clear();
            foreach (var template in uniqueTemplates)
            {
                TemplateShapes.Add(template);
            }
            
            System.Diagnostics.Debug.WriteLine($"ManagePage: Showing {TemplateShapes.Count} unique templates");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ManagePage: Error loading templates: {ex.Message}");
            await ShowErrorDialogAsync("Load Templates Error", $"Failed to load templates: {ex.Message}");
        }
        finally
        {
            IsLoadingTemplates = false;
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
    private async Task RefreshTemplatesAsync()
    {
        await LoadTemplatesAsync();
        TotalTemplates = TemplateShapes.Count;
    }

    [RelayCommand]
    private async Task DeleteTemplateAsync(Shape? template)
    {
        if (template == null || XamlRoot == null)
            return;

        System.Diagnostics.Debug.WriteLine($"ManagePage: Deleting template {template.Type} (ID: {template.Id})");

        // Show confirmation dialog
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Delete Template?",
            Content = $"Are you sure you want to delete the '{template.Type}' template?\n\n" +
                     $"This template has been used {template.UsageCount} time(s).\n\n" +
                     $"?? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            IsLoadingTemplates = true;
            try
            {
                var deleted = await _shapeService.DeleteShapeAsync(template.Id);

                if (deleted)
                {
                    // Remove from collection
                    TemplateShapes.Remove(template);
                    
                    // Update total count
                    TotalTemplates--;

                    System.Diagnostics.Debug.WriteLine($"ManagePage: Template {template.Id} deleted successfully");

                    await ShowSuccessDialogAsync("Template Deleted", 
                        $"Template '{template.Type}' has been deleted successfully.");
                }
                else
                {
                    await ShowErrorDialogAsync("Delete Failed", 
                        "Template not found or could not be deleted.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ManagePage: Error deleting template: {ex.Message}");
                await ShowErrorDialogAsync("Delete Error", 
                    $"An error occurred while deleting the template:\n\n{ex.Message}");
            }
            finally
            {
                IsLoadingTemplates = false;
            }
        }
    }

    [RelayCommand]
    private async Task CleanupDuplicateTemplatesAsync()
    {
        if (XamlRoot == null) return;

        System.Diagnostics.Debug.WriteLine("ManagePage: Starting cleanup duplicates...");

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Cleanup Duplicate Templates?",
            Content = "This will remove duplicate templates from the database.\n\n" +
                     "Templates with higher usage count will be kept.\n\n" +
                     "Do you want to continue?",
            PrimaryButtonText = "Cleanup",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

        IsLoadingTemplates = true;
        try
        {
            var allTemplates = await _shapeService.GetTemplateShapesAsync();
            
            System.Diagnostics.Debug.WriteLine($"ManagePage: Found {allTemplates.Count} total templates");
            
            // Group by similarity
            var groups = allTemplates
                .GroupBy(t => new { t.Type, t.GeometryData, t.StrokeColor, t.FillColor, t.StrokeThickness })
                .Where(g => g.Count() > 1)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"ManagePage: Found {groups.Count} groups with duplicates");

            int deletedCount = 0;
            foreach (var group in groups)
            {
                // Keep the one with highest usage or oldest
                var toKeep = group.OrderByDescending(t => t.UsageCount).ThenBy(t => t.Id).First();
                var toDelete = group.Where(t => t.Id != toKeep.Id).ToList();

                System.Diagnostics.Debug.WriteLine($"ManagePage: Keeping template {toKeep.Id}, deleting {toDelete.Count} duplicates");

                foreach (var template in toDelete)
                {
                    await _shapeService.DeleteShapeAsync(template.Id);
                    deletedCount++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"ManagePage: Cleanup complete, deleted {deletedCount} templates");

            await LoadTemplatesAsync();
            TotalTemplates = TemplateShapes.Count;

            await ShowSuccessDialogAsync("Cleanup Complete", 
                $"Removed {deletedCount} duplicate template(s).\n\n" +
                $"You now have {TemplateShapes.Count} unique templates.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ManagePage: Cleanup error: {ex.Message}");
            await ShowErrorDialogAsync("Cleanup Error", 
                $"Failed to cleanup templates: {ex.Message}");
        }
        finally
        {
            IsLoadingTemplates = false;
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
