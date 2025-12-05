using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PaintApp.ViewModels;

namespace PaintApp.Views;

public sealed partial class HomePage : Page
{
    public HomePageViewModel ViewModel { get; }

    public HomePage()
    {
        InitializeComponent();
        ViewModel = App.ServiceProvider.GetRequiredService<HomePageViewModel>();
        DataContext = ViewModel;
    }

    private async void StartDrawingButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProfile == null)
        {
            var dialog = new ContentDialog
            {
                Title = "No Profile Selected",
                Content = "Please select a profile before starting to draw.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync().AsTask();
            return;
        }

        var confirmDialog = new ContentDialog
        {
            Title = "Start Drawing",
            Content = $"Start drawing session with profile '{ViewModel.SelectedProfile.Name}'?",
            PrimaryButtonText = "Start",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync().AsTask();
        if (result == ContentDialogResult.Primary)
        {
            Frame.Navigate(typeof(DrawPage), ViewModel.SelectedProfile);
        }
    }
}
