using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PaintApp.ViewModels;

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
}
