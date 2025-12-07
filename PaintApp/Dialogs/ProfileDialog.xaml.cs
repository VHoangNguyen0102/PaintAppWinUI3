using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using PaintApp.Models;
using WinRT.Interop;
using Microsoft.UI;

namespace PaintApp.Dialogs
{
    public sealed partial class ProfileDialog : ContentDialog
    {
        public Profile? Profile { get; private set; }
        private bool _isEditMode;

        public ProfileDialog()
        {
            InitializeComponent();
            _isEditMode = false;
            InitializeDefaultColors();
        }

        public ProfileDialog(Profile profile) : this()
        {
            _isEditMode = true;
            Profile = profile;
            LoadProfileData(profile);
        }

        private void InitializeDefaultColors()
        {
            CanvasBackgroundColorPicker.Color = Colors.White;
            StrokeColorPicker.Color = Colors.Black;
            FillColorPicker.Color = Colors.White;
        }

        private void LoadProfileData(Profile profile)
        {
            Title = "Edit Profile";
            
            NameTextBox.Text = profile.Name;
            AvatarPathTextBox.Text = profile.AvatarPath ?? string.Empty;
            
            ThemeComboBox.SelectedIndex = profile.Theme switch
            {
                "Light" => 0,
                "Dark" => 1,
                _ => 2
            };
            
            CanvasWidthNumberBox.Value = profile.DefaultCanvasWidth;
            CanvasHeightNumberBox.Value = profile.DefaultCanvasHeight;
            
            CanvasBackgroundColorPicker.Color = ParseColor(profile.DefaultCanvasBackgroundColor);
            StrokeColorPicker.Color = ParseColor(profile.DefaultStrokeColor);
            FillColorPicker.Color = ParseColor(profile.DefaultFillColor);
            
            StrokeThicknessSlider.Value = profile.DefaultStrokeThickness;
        }

        private Color ParseColor(string hexColor)
        {
            try
            {
                hexColor = hexColor.TrimStart('#');
                
                if (hexColor.Length == 6)
                {
                    hexColor = "FF" + hexColor;
                }
                
                var a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                var r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                var g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                var b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                
                return Color.FromArgb(a, r, g, b);
            }
            catch
            {
                return Colors.White;
            }
        }

        private string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                NameErrorText.Visibility = Visibility.Collapsed;
            }
            
            return isValid;
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NameErrorText.Visibility = Visibility.Collapsed;
            }
        }

        private void StrokeThicknessSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (StrokeThicknessValue != null)
            {
                StrokeThicknessValue.Text = e.NewValue.ToString("F1");
            }
        }

        private async void BrowseAvatar_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            
            var hwnd = WindowNative.GetWindowHandle(App.ServiceProvider.GetService(typeof(MainWindow)) as MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);
            
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                AvatarPathTextBox.Text = file.Path;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = !ValidateInput();
            
            if (!args.Cancel)
            {
                var selectedTheme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "System";
                
                if (_isEditMode && Profile != null)
                {
                    Profile.Name = NameTextBox.Text.Trim();
                    Profile.AvatarPath = string.IsNullOrWhiteSpace(AvatarPathTextBox.Text) 
                        ? null 
                        : AvatarPathTextBox.Text;
                    Profile.Theme = selectedTheme;
                    Profile.DefaultCanvasWidth = (int)CanvasWidthNumberBox.Value;
                    Profile.DefaultCanvasHeight = (int)CanvasHeightNumberBox.Value;
                    Profile.DefaultCanvasBackgroundColor = ColorToHex(CanvasBackgroundColorPicker.Color);
                    Profile.DefaultStrokeColor = ColorToHex(StrokeColorPicker.Color);
                    Profile.DefaultFillColor = ColorToHex(FillColorPicker.Color);
                    Profile.DefaultStrokeThickness = StrokeThicknessSlider.Value;
                }
                else
                {
                    Profile = new Profile
                    {
                        Name = NameTextBox.Text.Trim(),
                        AvatarPath = string.IsNullOrWhiteSpace(AvatarPathTextBox.Text) 
                            ? null 
                            : AvatarPathTextBox.Text,
                        Theme = selectedTheme,
                        DefaultCanvasWidth = (int)CanvasWidthNumberBox.Value,
                        DefaultCanvasHeight = (int)CanvasHeightNumberBox.Value,
                        DefaultCanvasBackgroundColor = ColorToHex(CanvasBackgroundColorPicker.Color),
                        DefaultStrokeColor = ColorToHex(StrokeColorPicker.Color),
                        DefaultFillColor = ColorToHex(FillColorPicker.Color),
                        DefaultStrokeThickness = StrokeThicknessSlider.Value,
                        CreatedAt = DateTime.Now
                    };
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Profile = null;
        }
    }
}
