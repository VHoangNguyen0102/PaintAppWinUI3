using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaintApp.Models;
using PaintApp.Data;
using Microsoft.EntityFrameworkCore;

namespace PaintApp.ViewModels;

public partial class ManagePageViewModel : ViewModelBase
{
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    private int totalProfiles;

    [ObservableProperty]
    private int totalDrawings;

    [ObservableProperty]
    private int totalTemplates;

    [ObservableProperty]
    private int selectedTabIndex;

    [ObservableProperty]
    private string currentBreadcrumb = "Dashboard";

    public ManagePageViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _ = LoadDataAsync();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        CurrentBreadcrumb = value switch
        {
            0 => "Dashboard",
            1 => "Manage Drawings",
            2 => "Manage Templates",
            _ => "Dashboard"
        };
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
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadDataAsync();
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
