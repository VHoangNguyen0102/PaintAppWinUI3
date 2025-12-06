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
}
