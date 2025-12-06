using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
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
    private CanvasModel? _currentCanvas;

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
    }

    public void SetCanvas(CanvasModel canvas)
    {
        _currentCanvas = canvas;
        IsCanvasLoaded = true;
        CanvasInfo = $"{canvas.Name} - {canvas.Width} × {canvas.Height}";
    }

    [RelayCommand]
    private void SelectTool(string tool)
    {
        SelectedTool = tool;
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
        try
        {
            var dialog = new NewCanvasDialog(_currentProfile);
            dialog.XamlRoot = _xamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Canvas != null)
            {
                var createdCanvas = await _canvasService.CreateCanvasAsync(dialog.Canvas);
                SetCanvas(createdCanvas);

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
