using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using PaintApp.Views;
using PaintApp.Services;
using PaintApp.Models;

namespace PaintApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly ApplicationDataContainer _localSettings;
        private readonly IProfileManager _profileManager;
        private readonly INavigationService _navigationService;
        private NavigationViewItem? _canvasNavItem;
        private NavigationViewItem? _profilesNavItem;

        public MainWindow()
        {
            InitializeComponent();
            _localSettings = ApplicationData.Current.LocalSettings;
            _profileManager = App.ServiceProvider.GetRequiredService<IProfileManager>();
            _navigationService = App.ServiceProvider.GetRequiredService<INavigationService>();
            
            SetupCustomTitleBar();
            SetupBackdrop();
            LoadTheme();
            
            // Subscribe to profile changes
            _profileManager.CurrentProfileChanged += ProfileManager_CurrentProfileChanged;
            
            // Subscribe to navigation state changes
            _navigationService.NavigationStateChanged += NavigationService_NavigationStateChanged;
            
            // Find nav items by name
            _canvasNavItem = CanvasNavItem;
            _profilesNavItem = ProfilesNavItem;
            
            // Set initial state - disable Canvas and Profiles until profile selected
            UpdateNavItemsState();
            
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(HomePage));
        }

        private void NavigationService_NavigationStateChanged(object? sender, NavigationState state)
        {
            // Update UI when navigation state changes
            DispatcherQueue.TryEnqueue(() =>
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: Navigation state changed - Page: {state.CurrentPage}, Profile: {state.ProfileId}, Canvas: {state.CanvasId}");
            });
        }

        private void ProfileManager_CurrentProfileChanged(object? sender, Models.Profile? profile)
        {
            // Update nav items when profile changes
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateNavItemsState();
                System.Diagnostics.Debug.WriteLine($"MainWindow: Nav items updated - HasProfile: {_profileManager.HasProfile()}");
            });
        }

        private void UpdateNavItemsState()
        {
            bool hasProfile = _profileManager.HasProfile();
            
            // Update Canvas nav item
            if (_canvasNavItem != null)
            {
                _canvasNavItem.IsEnabled = hasProfile;
                
                if (CanvasRequiresProfileBadge != null)
                {
                    CanvasRequiresProfileBadge.Visibility = hasProfile 
                        ? Visibility.Collapsed 
                        : Visibility.Visible;
                }
                
                if (!hasProfile)
                {
                    ToolTipService.SetToolTip(_canvasNavItem, 
                        new ToolTip { Content = "Select a profile from Home page first" });
                }
                else
                {
                    ToolTipService.SetToolTip(_canvasNavItem, 
                        new ToolTip { Content = "Draw and manage your canvas" });
                }
            }
            
            // Update Profiles nav item (ManagePage)
            if (_profilesNavItem != null)
            {
                _profilesNavItem.IsEnabled = hasProfile;
                
                if (!hasProfile)
                {
                    ToolTipService.SetToolTip(_profilesNavItem, 
                        new ToolTip { Content = "Select a profile from Home page first" });
                }
                else
                {
                    ToolTipService.SetToolTip(_profilesNavItem, 
                        new ToolTip { Content = "Manage profiles, canvases, and templates" });
                }
            }
        }

        private void SetupCustomTitleBar()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
        }

        private void SetupBackdrop()
        {
            SystemBackdrop = new MicaBackdrop();
        }

        private void LoadTheme()
        {
            if (_localSettings.Values["AppTheme"] is string themeName)
            {
                if (Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = themeName switch
                    {
                        "Light" => ElementTheme.Light,
                        "Dark" => ElementTheme.Dark,
                        _ => ElementTheme.Default
                    };
                }
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Content is FrameworkElement rootElement)
            {
                var currentTheme = rootElement.ActualTheme;
                var newTheme = currentTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
                rootElement.RequestedTheme = newTheme;
                
                _localSettings.Values["AppTheme"] = newTheme.ToString();
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
            
            // Record navigation in NavigationService
            var pageType = e.SourcePageType.Name;
            _navigationService.RecordNavigation(pageType, e.Parameter);
            
            // Update selected nav item to match current page
            UpdateSelectedNavItem(e.SourcePageType);
            
            System.Diagnostics.Debug.WriteLine($"MainWindow: Navigated to {pageType}");
        }

        private void UpdateSelectedNavItem(Type pageType)
        {
            // Find and select the correct nav item based on page type
            NavigationViewItem? itemToSelect = null;

            if (pageType == typeof(HomePage))
            {
                itemToSelect = NavView.MenuItems.OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == "Home");
            }
            else if (pageType == typeof(DrawPage))
            {
                itemToSelect = _canvasNavItem;
            }
            else if (pageType == typeof(ManagePage))
            {
                itemToSelect = _profilesNavItem;
            }

            if (itemToSelect != null && NavView.SelectedItem != itemToSelect)
            {
                NavView.SelectedItem = itemToSelect;
                System.Diagnostics.Debug.WriteLine($"MainWindow: Nav item synced to {itemToSelect.Content}");
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Page));
                return;
            }
            
            if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag?.ToString() ?? string.Empty;
                
                // Check profile before navigating to Canvas/DrawPage or Profiles
                if ((tag == "Canvas" || tag == "Profiles") && !_profileManager.HasProfile())
                {
                    System.Diagnostics.Debug.WriteLine($"MainWindow: Cannot navigate to {tag} - no profile selected");
                    
                    // Show error dialog
                    _ = ShowNoProfileDialogAsync();
                    
                    // Reset selection to Home
                    var homeItem = NavView.MenuItems.OfType<NavigationViewItem>()
                        .FirstOrDefault(item => item.Tag?.ToString() == "Home");
                    
                    if (homeItem != null)
                    {
                        NavView.SelectedItem = homeItem;
                    }
                    
                    // Navigate to Home if not already there
                    if (ContentFrame.CurrentSourcePageType != typeof(HomePage))
                    {
                        ContentFrame.Navigate(typeof(HomePage));
                    }
                    return;
                }
                
                Type? pageType = tag switch
                {
                    "Home" => typeof(HomePage),
                    "Canvas" => typeof(DrawPage),
                    "Profiles" => typeof(ManagePage),
                    _ => null
                };

                if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
                {
                    System.Diagnostics.Debug.WriteLine($"MainWindow: Navigating to {pageType.Name}");
                    ContentFrame.Navigate(pageType);
                }
            }
        }

        private async System.Threading.Tasks.Task ShowNoProfileDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Profile Required",
                Content = "Please select a profile from the Home page first.\n\n" +
                         "A profile is required to:\n" +
                         "• Access the Canvas for drawing\n" +
                         "• Manage your canvases and templates\n" +
                         "• Save your drawing preferences\n\n" +
                         "Go to Home page and select a profile to continue.",
                PrimaryButtonText = "Go to Home",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: User chose to go to Home page");
            }
        }
    }
}
