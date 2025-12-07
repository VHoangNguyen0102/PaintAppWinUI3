using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PaintApp.ViewModels;
using PaintApp.Models;
using CanvasModel = PaintApp.Models.Canvas;
using ProfileModel = PaintApp.Models.Profile;
using ShapeModel = PaintApp.Models.Shape;

namespace PaintApp.Views;

public sealed partial class ManagePage : Page
{
    public ManagePageViewModel ViewModel { get; }

    public ManagePage()
    {
        InitializeComponent();
        ViewModel = App.ServiceProvider.GetRequiredService<ManagePageViewModel>();
        DataContext = ViewModel;
        
        Loaded += ManagePage_Loaded;
    }

    private void ManagePage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetXamlRoot(this.XamlRoot);
    }

    private void CanvasGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        // Disable ItemClick to avoid conflict with button clicks
        // Navigation is now handled by OpenCanvasButton_Click
    }

    private void OpenCanvasButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CanvasModel canvas && ViewModel.SelectedProfile != null)
        {
            // Navigate to DrawPage with canvas and profile
            Frame.Navigate(typeof(DrawPage), new DrawPageNavigationParameter
            {
                Canvas = canvas,
                Profile = ViewModel.SelectedProfile
            });
        }
    }

    private async void DeleteCanvasButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CanvasModel canvas)
        {
            await ViewModel.DeleteCanvasCommand.ExecuteAsync(canvas);
        }
    }

    private async void DeleteTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ShapeModel template)
        {
            await ViewModel.DeleteTemplateCommand.ExecuteAsync(template);
        }
    }
}

// Navigation parameter class
public class DrawPageNavigationParameter
{
    public CanvasModel Canvas { get; set; } = null!;
    public ProfileModel Profile { get; set; } = null!;
}
