using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;
using PaintApp.Services;
using PaintApp.Dialogs;
using CanvasModel = PaintApp.Models.Canvas;
using PaintApp.Models;

namespace PaintApp.ViewModels;

public partial class DrawPageViewModel : ViewModelBase
{
    private readonly ICanvasService _canvasService;
    private XamlRoot? _xamlRoot;
    private Profile? _currentProfile;

    [ObservableProperty]
    private CanvasModel? currentCanvas;

    [ObservableProperty]
    private string selectedTool = "Line";

    [ObservableProperty]
    private Color strokeColor = Colors.Black;

    [ObservableProperty]
    private Color fillColor = Colors.Transparent;

    [ObservableProperty]
    private double strokeThickness = 2.0;

    [ObservableProperty]
    private bool isShapeSelected;

    [ObservableProperty]
    private string selectedShapeInfo = "No shape selected";

    [ObservableProperty]
    private string canvasInfo = "No canvas loaded";

    [ObservableProperty]
    private bool isCanvasLoaded;

    [ObservableProperty]
    private int canvasWidth = 800;

    [ObservableProperty]
    private int canvasHeight = 600;

    [ObservableProperty]
    private string canvasBackgroundColor = "#FFFFFF";

    [ObservableProperty]
    private bool isDrawingPolygon;

    [ObservableProperty]
    private int polygonPointCount;

    public event EventHandler<CanvasModel>? CanvasLoaded;

    // Polygon drawing state
    public List<Point> PolygonPoints { get; } = new List<Point>();

    public ObservableCollection<string> Tools { get; } = new()
    {
        "Line",
        "Rectangle",
        "Oval",
        "Circle",
        "Triangle",
        "Polygon"
    };

    public ObservableCollection<Color> ColorPalette { get; } = new()
    {
        Colors.Black,
        Colors.White,
        Colors.Red,
        Colors.Green,
        Colors.Blue,
        Colors.Yellow,
        Colors.Orange,
        Colors.Purple,
        Colors.Pink,
        Colors.Brown,
        Colors.Gray,
        Colors.Cyan
    };

    public DrawPageViewModel(ICanvasService canvasService)
    {
        _canvasService = canvasService;
    }

    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public void SetProfile(Profile profile)
    {
        _currentProfile = profile;
        
        if (!IsCanvasLoaded)
        {
            CanvasWidth = profile.DefaultCanvasWidth;
            CanvasHeight = profile.DefaultCanvasHeight;
            CanvasBackgroundColor = profile.DefaultCanvasBackgroundColor;
            StrokeColor = ParseColor(profile.DefaultStrokeColor);
            FillColor = ParseColor(profile.DefaultFillColor);
            StrokeThickness = profile.DefaultStrokeThickness;
        }
    }

    public void LoadCanvas(CanvasModel canvas)
    {
        CurrentCanvas = canvas;
        IsCanvasLoaded = true;
        CanvasInfo = $"{canvas.Name} - {canvas.Width} × {canvas.Height}";
        CanvasWidth = canvas.Width;
        CanvasHeight = canvas.Height;
        CanvasBackgroundColor = canvas.BackgroundColor;
        
        CanvasLoaded?.Invoke(this, canvas);
    }

    public void StartPolygonDrawing()
    {
        IsDrawingPolygon = true;
        PolygonPoints.Clear();
        PolygonPointCount = 0;
    }

    public void AddPolygonPoint(Point point)
    {
        PolygonPoints.Add(point);
        PolygonPointCount = PolygonPoints.Count;
    }

    public void ClearPolygonDrawing()
    {
        IsDrawingPolygon = false;
        PolygonPoints.Clear();
        PolygonPointCount = 0;
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

    [RelayCommand]
    private void SelectTool(string tool)
    {
        SelectedTool = tool;
        
        // Reset polygon drawing khi ??i tool
        if (tool != "Triangle" && tool != "Polygon")
        {
            ClearPolygonDrawing();
        }
    }

    [RelayCommand]
    private void SelectStrokeColor(Color color)
    {
        StrokeColor = color;
    }

    [RelayCommand]
    private void SelectFillColor(Color color)
    {
        FillColor = color;
    }

    [RelayCommand]
    private async Task NewCanvasAsync()
    {
        if (_currentProfile == null)
        {
            await ShowErrorDialogAsync("Error", "Please select a profile first.");
            return;
        }

        try
        {
            var dialog = new NewCanvasDialog(_currentProfile);
            dialog.XamlRoot = _xamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Canvas != null)
            {
                var createdCanvas = await _canvasService.CreateCanvasAsync(dialog.Canvas);
                LoadCanvas(createdCanvas);

                await ShowSuccessDialogAsync("Success", $"Canvas '{createdCanvas.Name}' created successfully!");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Create Canvas Error", $"Failed to create canvas: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ClearCanvas()
    {
    }

    [RelayCommand]
    private void Undo()
    {
    }

    [RelayCommand]
    private void Redo()
    {
    }

    [RelayCommand]
    private void Save()
    {
    }

    private async Task ShowSuccessDialogAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    private async Task ShowErrorDialogAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }
}
