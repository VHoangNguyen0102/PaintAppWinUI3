using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaintApp.Models;
using PaintApp.Data;
using Microsoft.EntityFrameworkCore;

namespace PaintApp.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDrawingCommand))]
    private Profile? selectedProfile;

    [ObservableProperty]
    private bool canStartDrawing;

    public HomePageViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _ = LoadProfilesAsync();
    }

    partial void OnSelectedProfileChanged(Profile? value)
    {
        CanStartDrawing = value != null;
    }

    private async Task LoadProfilesAsync()
    {
        var profilesList = await _dbContext.Profiles.ToListAsync();
        Profiles.Clear();
        foreach (var profile in profilesList)
        {
            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private void CreateNewProfile()
    {
    }

    [RelayCommand]
    private void StartDrawing()
    {
    }
}
