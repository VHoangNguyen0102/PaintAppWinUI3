using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PaintApp.ViewModels;

public partial class DrawPageViewModel : ViewModelBase
{
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

    public DrawPageViewModel()
    {
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
}
