using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PaintApp.ViewModels;
using CanvasModel = PaintApp.Models.Canvas;
using ProfileModel = PaintApp.Models.Profile;

namespace PaintApp.Views;

public sealed partial class ManagePage : Page
{
    public ManagePageViewModel ViewModel { get; }

    public ManagePage()
    {
        InitializeComponent();
        ViewModel = App.ServiceProvider.GetRequiredService<ManagePageViewModel>();
        DataContext = ViewModel;
    }

    private void CanvasGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CanvasModel canvas && ViewModel.SelectedProfile != null)
        {
            // Navigate to DrawPage with canvas and profile
            Frame.Navigate(typeof(DrawPage), new DrawPageNavigationParameter
            {
                Canvas = canvas,
                Profile = ViewModel.SelectedProfile
            });
        }
    }
}

// Navigation parameter class
public class DrawPageNavigationParameter
{
    public CanvasModel Canvas { get; set; } = null!;
    public ProfileModel Profile { get; set; } = null!;
}
