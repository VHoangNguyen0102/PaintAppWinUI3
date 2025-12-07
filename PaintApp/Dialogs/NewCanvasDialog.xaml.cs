using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Windows.UI;
using CanvasModel = PaintApp.Models.Canvas;
using PaintApp.Models;

namespace PaintApp.Dialogs;

public sealed partial class NewCanvasDialog : ContentDialog
{
    public CanvasModel? Canvas { get; private set; }
    private readonly Profile? _profile;

    public NewCanvasDialog()
    {
        InitializeComponent();
        InitializeDefaults();
    }

    public NewCanvasDialog(Profile profile) : this()
    {
        _profile = profile;
        LoadProfileDefaults(profile);
        UpdatePreview();
    }

    private void InitializeDefaults()
    {
        BackgroundColorPicker.Color = Colors.White;
        WidthNumberBox.Value = 800;
        HeightNumberBox.Value = 600;
    }

    private void LoadProfileDefaults(Profile profile)
    {
        WidthNumberBox.Value = profile.DefaultCanvasWidth;
        HeightNumberBox.Value = profile.DefaultCanvasHeight;
        BackgroundColorPicker.Color = ParseColor(profile.DefaultCanvasBackgroundColor);
        
        NameTextBox.PlaceholderText = $"Canvas - {DateTime.Now:yyyy-MM-dd HH:mm}";
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
        
        // Validate Name
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            if (NameErrorText != null)
                NameErrorText.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            if (NameErrorText != null)
                NameErrorText.Visibility = Visibility.Collapsed;
        }
        
        // Validate Width
        if (WidthNumberBox.Value < 400 || WidthNumberBox.Value > 2000)
        {
            if (WidthErrorText != null)
                WidthErrorText.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            if (WidthErrorText != null)
                WidthErrorText.Visibility = Visibility.Collapsed;
        }
        
        // Validate Height
        if (HeightNumberBox.Value < 400 || HeightNumberBox.Value > 2000)
        {
            if (HeightErrorText != null)
                HeightErrorText.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            if (HeightErrorText != null)
                HeightErrorText.Visibility = Visibility.Collapsed;
        }
        
        return isValid;
    }

    private void UpdatePreview()
    {
        if (PreviewName == null || PreviewSize == null || PreviewAspect == null)
            return;

        PreviewName.Text = string.IsNullOrWhiteSpace(NameTextBox.Text) 
            ? "—" 
            : NameTextBox.Text;
        
        var width = (int)WidthNumberBox.Value;
        var height = (int)HeightNumberBox.Value;
        
        PreviewSize.Text = $"{width} × {height} px";
        PreviewAspect.Text = CalculateAspectRatio(width, height);
    }

    private string CalculateAspectRatio(int width, int height)
    {
        int gcd = GCD(width, height);
        int ratioW = width / gcd;
        int ratioH = height / gcd;
        
        // Common aspect ratios
        if (ratioW == 16 && ratioH == 9) return "16:9 (Widescreen)";
        if (ratioW == 4 && ratioH == 3) return "4:3 (Standard)";
        if (ratioW == 1 && ratioH == 1) return "1:1 (Square)";
        if (ratioW == 21 && ratioH == 9) return "21:9 (Ultrawide)";
        
        return $"{ratioW}:{ratioH}";
    }

    private int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            if (NameErrorText != null)
                NameErrorText.Visibility = Visibility.Collapsed;
        }
        UpdatePreview();
    }

    private void SizeNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (double.IsNaN(args.NewValue))
        {
            return;
        }

        if (sender == WidthNumberBox)
        {
            if (args.NewValue >= 400 && args.NewValue <= 2000)
            {
                if (WidthErrorText != null)
                    WidthErrorText.Visibility = Visibility.Collapsed;
            }
        }
        else if (sender == HeightNumberBox)
        {
            if (args.NewValue >= 400 && args.NewValue <= 2000)
            {
                if (HeightErrorText != null)
                    HeightErrorText.Visibility = Visibility.Collapsed;
            }
        }

        UpdatePreview();
    }

    private void PresetHD_Click(object sender, RoutedEventArgs e)
    {
        WidthNumberBox.Value = 1280;
        HeightNumberBox.Value = 720;
    }

    private void PresetFullHD_Click(object sender, RoutedEventArgs e)
    {
        WidthNumberBox.Value = 1920;
        HeightNumberBox.Value = 1080;
    }

    private void PresetSquare_Click(object sender, RoutedEventArgs e)
    {
        WidthNumberBox.Value = 1000;
        HeightNumberBox.Value = 1000;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = !ValidateInput();
        
        if (!args.Cancel)
        {
            Canvas = new CanvasModel
            {
                Name = NameTextBox.Text.Trim(),
                Width = (int)WidthNumberBox.Value,
                Height = (int)HeightNumberBox.Value,
                BackgroundColor = ColorToHex(BackgroundColorPicker.Color),
                ProfileId = _profile?.Id ?? 0,
                CreatedAt = DateTime.Now
            };
        }
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Canvas = null;
    }
}
