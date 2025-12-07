using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
        
        Loaded += HomePage_Loaded;
    }

    private void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetXamlRoot(this.XamlRoot);
    }

    private async void StartDrawingButton_Click(object sender, RoutedEventArgs e)
    {
        // Step 1: Validate profile selection
        if (ViewModel.SelectedProfile == null)
        {
            var noProfileDialog = new ContentDialog
            {
                Title = "No Profile Selected",
                Content = "Please select a profile before starting to draw.\n\n" +
                         "A profile is required to save your drawing settings and preferences.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await noProfileDialog.ShowAsync();
            return;
        }

        System.Diagnostics.Debug.WriteLine($"HomePage: Starting drawing with profile '{ViewModel.SelectedProfile.Name}' (ID: {ViewModel.SelectedProfile.Id})");

        // Step 2: Show detailed confirmation dialog
        var confirmDialog = new ContentDialog
        {
            Title = "Start Drawing Session",
            XamlRoot = this.XamlRoot,
            PrimaryButtonText = "Start Drawing",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        // Create detailed content with profile info
        var contentPanel = new StackPanel { Spacing = 12 };
        
        // Profile info
        contentPanel.Children.Add(new TextBlock 
        { 
            Text = $"Profile: {ViewModel.SelectedProfile.Name}",
            Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
        });

        // Settings info
        var settingsPanel = new StackPanel { Spacing = 4, Margin = new Thickness(0, 8, 0, 0) };
        settingsPanel.Children.Add(new TextBlock 
        { 
            Text = "Default Settings:",
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        });
        settingsPanel.Children.Add(new TextBlock 
        { 
            Text = $"• Canvas Size: {ViewModel.SelectedProfile.DefaultCanvasWidth} × {ViewModel.SelectedProfile.DefaultCanvasHeight}",
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
        });
        settingsPanel.Children.Add(new TextBlock 
        { 
            Text = $"• Stroke Thickness: {ViewModel.SelectedProfile.DefaultStrokeThickness} px",
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
        });
        settingsPanel.Children.Add(new TextBlock 
        { 
            Text = $"• Theme: {ViewModel.SelectedProfile.Theme ?? "Default"}",
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
        });
        contentPanel.Children.Add(settingsPanel);

        // Confirmation message
        contentPanel.Children.Add(new TextBlock 
        { 
            Text = "\nAre you ready to start creating?",
            Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
            Margin = new Thickness(0, 8, 0, 0)
        });

        confirmDialog.Content = contentPanel;

        var result = await confirmDialog.ShowAsync();
        
        // Step 3: Navigate to DrawPage with profile
        if (result == ContentDialogResult.Primary)
        {
            System.Diagnostics.Debug.WriteLine($"HomePage: Navigating to DrawPage with ProfileId: {ViewModel.SelectedProfile.Id}");
            
            try
            {
                // Pass profile as navigation parameter
                Frame.Navigate(typeof(DrawPage), ViewModel.SelectedProfile);
                
                System.Diagnostics.Debug.WriteLine("HomePage: Navigation successful");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomePage: Navigation error: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Navigation Error",
                    Content = $"Failed to start drawing session:\n\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("HomePage: Drawing session cancelled by user");
        }
    }
}
