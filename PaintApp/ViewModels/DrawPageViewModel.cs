using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using PaintApp.Helpers;
using CanvasModel = PaintApp.Models.Canvas;
using ShapeModel = PaintApp.Models.Shape;
using PaintApp.Models;

namespace PaintApp.ViewModels;

public partial class DrawPageViewModel : ViewModelBase
{
    private readonly ICanvasService _canvasService;
    private readonly IShapeService _shapeService;
    private readonly IProfileService _profileService;
    private XamlRoot? _xamlRoot;
    private Profile? _currentProfile;

    [ObservableProperty]
    private CanvasModel? currentCanvas;

    [ObservableProperty]
    private DrawingTool currentTool = DrawingTool.Line;

    [ObservableProperty]
    private string selectedTool = "Line";

    [ObservableProperty]
    private Color currentStrokeColor = Colors.Black;

    [ObservableProperty]
    private Color? currentFillColor = Colors.Transparent;

    [ObservableProperty]
    private double currentStrokeThickness = 2.0;

    [ObservableProperty]
    private bool isFillEnabled = true;

    [ObservableProperty]
    private string selectedDashStyle = "Solid";

    [ObservableProperty]
    private bool isShapeSelected;

    [ObservableProperty]
    private ShapeModel? selectedShape;

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

    // Drawing state
    public Point? StartPoint { get; set; }
    public Point? EndPoint { get; set; }
    public bool IsDrawing { get; set; }

    public event EventHandler<CanvasModel>? CanvasLoaded;
    public event EventHandler<ShapeModel>? ShapeCreated;

    // Collections
    public ObservableCollection<ShapeModel> Shapes { get; } = new();
    public List<Point> PolygonPoints { get; } = new List<Point>();

    public ObservableCollection<string> Tools { get; } = new()
    {
        "Select",
        "Line",
        "Rectangle",
        "Oval",
        "Circle",
        "Triangle",
        "Polygon"
    };

    public ObservableCollection<string> DashStyles { get; } = new()
    {
        "Solid",
        "Dash",
        "Dot",
        "DashDot",
        "DashDotDot"
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

    public DrawPageViewModel(ICanvasService canvasService, IShapeService shapeService, IProfileService profileService)
    {
        _canvasService = canvasService;
        _shapeService = shapeService;
        _profileService = profileService;
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
            CurrentStrokeColor = ParseColor(profile.DefaultStrokeColor);
            CurrentFillColor = ParseColor(profile.DefaultFillColor);
            CurrentStrokeThickness = profile.DefaultStrokeThickness;
        }
    }

    public async void LoadCanvas(CanvasModel canvas)
    {
        CurrentCanvas = canvas;
        IsCanvasLoaded = true;
        CanvasInfo = $"{canvas.Name} - {canvas.Width} × {canvas.Height}";
        CanvasWidth = canvas.Width;
        CanvasHeight = canvas.Height;
        CanvasBackgroundColor = canvas.BackgroundColor;
        
        // Load shapes from database
        await LoadShapesAsync(canvas.Id);
        
        CanvasLoaded?.Invoke(this, canvas);
    }

    private async Task LoadShapesAsync(int canvasId)
    {
        try
        {
            var shapes = await _shapeService.GetShapesByCanvasIdAsync(canvasId);
            
            Shapes.Clear();
            foreach (var shape in shapes)
            {
                Shapes.Add(shape);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Load Shapes Error", $"Failed to load shapes: {ex.Message}");
        }
    }

    public async Task SaveShapeAsync(ShapeModel shape)
    {
        try
        {
            if (CurrentCanvas == null)
            {
                await ShowErrorDialogAsync("Error", "No canvas loaded.");
                return;
            }

            shape.CanvasId = CurrentCanvas.Id;
            var createdShape = await _shapeService.CreateShapeAsync(shape);
            
            Shapes.Add(createdShape);
            ShapeCreated?.Invoke(this, createdShape);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Save Shape Error", $"Failed to save shape: {ex.Message}");
        }
    }

    public async Task UpdateShapeAsync(ShapeModel shape)
    {
        try
        {
            await _shapeService.UpdateShapeAsync(shape);
            
            // Find and update in collection
            var existingShape = Shapes.FirstOrDefault(s => s.Id == shape.Id);
            if (existingShape != null)
            {
                var index = Shapes.IndexOf(existingShape);
                Shapes[index] = shape;
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Update Shape Error", $"Failed to update shape: {ex.Message}");
        }
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

    private string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    [RelayCommand]
    private void SelectTool(string tool)
    {
        SelectedTool = tool;
        CurrentTool = tool switch
        {
            "Select" => DrawingTool.Select,
            "Line" => DrawingTool.Line,
            "Rectangle" => DrawingTool.Rectangle,
            "Oval" => DrawingTool.Oval,
            "Circle" => DrawingTool.Circle,
            "Triangle" => DrawingTool.Triangle,
            "Polygon" => DrawingTool.Polygon,
            _ => DrawingTool.None
        };
        
        // Clear selection when switching to drawing tools
        if (tool != "Select")
        {
            SelectedShape = null;
            IsShapeSelected = false;
            SelectedShapeInfo = "No shape selected";
            ClearPolygonDrawing();
        }
    }

    public void SelectShapeCommand(ShapeModel shape)
    {
        SelectedShape = shape;
        IsShapeSelected = true;
        SelectedShapeInfo = $"{shape.Type} - {shape.StrokeColor}";
    }

    public void ClearSelection()
    {
        SelectedShape = null;
        IsShapeSelected = false;
        SelectedShapeInfo = "No shape selected";
    }

    [RelayCommand]
    private void SelectStrokeColor(Color color)
    {
        CurrentStrokeColor = color;
    }

    [RelayCommand]
    private void SelectFillColor(Color color)
    {
        CurrentFillColor = color;
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

    [RelayCommand]
    private async Task SaveAsDefaultAsync()
    {
        if (_currentProfile == null)
        {
            await ShowErrorDialogAsync("Error", "No profile loaded.");
            return;
        }

        try
        {
            // Update profile with current settings
            _currentProfile.DefaultStrokeColor = ColorToHex(CurrentStrokeColor);
            _currentProfile.DefaultFillColor = ColorToHex(CurrentFillColor ?? Colors.Transparent);
            _currentProfile.DefaultStrokeThickness = CurrentStrokeThickness;

            // Save to database
            await _profileService.UpdateProfileAsync(_currentProfile);

            await ShowSuccessDialogAsync("Settings Saved", 
                "Current drawing settings have been saved as default for this profile.");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Save Settings Error", 
                $"Failed to save settings: {ex.Message}");
        }
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

    // Compatibility properties (backward compatibility)
    [ObservableProperty]
    private Color strokeColor = Colors.Black;

    [ObservableProperty]
    private Color fillColor = Colors.Transparent;

    [ObservableProperty]
    private double strokeThickness = 2.0;

    partial void OnCurrentStrokeColorChanged(Color value)
    {
        StrokeColor = value;
    }

    partial void OnCurrentFillColorChanged(Color? value)
    {
        FillColor = value ?? Colors.Transparent;
    }

    partial void OnCurrentStrokeThicknessChanged(double value)
    {
        StrokeThickness = value;
    }
}
