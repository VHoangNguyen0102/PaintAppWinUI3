using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;
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

    // Selected Shape Properties (for editing)
    [ObservableProperty]
    private double selectedShapeX;

    [ObservableProperty]
    private double selectedShapeY;

    [ObservableProperty]
    private double selectedShapeWidth;

    [ObservableProperty]
    private double selectedShapeHeight;

    [ObservableProperty]
    private Color selectedShapeStrokeColor = Colors.Black;

    [ObservableProperty]
    private double selectedShapeStrokeThickness = 2.0;

    [ObservableProperty]
    private Color? selectedShapeFillColor;

    [ObservableProperty]
    private bool selectedShapeHasFill;

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

    [ObservableProperty]
    private bool isAutoSaving;

    [ObservableProperty]
    private string autoSaveStatus = "Auto-save enabled";

    [ObservableProperty]
    private DateTime? lastAutoSavedAt;

    // Auto-save timer
    private DispatcherTimer? _autoSaveTimer;
    private bool _hasUnsavedChanges;

    // Drawing state
    public Point? StartPoint { get; set; }
    public Point? EndPoint { get; set; }
    public bool IsDrawing { get; set; }

    public event EventHandler<CanvasModel>? CanvasLoaded;
    public event EventHandler<ShapeModel>? ShapeCreated;
    public event EventHandler<ShapeModel>? ShapeUpdated;
    public event EventHandler<ShapeModel>? ShapeDeleted;

    // Collections
    public ObservableCollection<ShapeModel> Shapes { get; } = new();
    public ObservableCollection<ShapeModel> TemplateShapes { get; } = new();
    public List<Point> PolygonPoints { get; } = new List<Point>();

    [ObservableProperty]
    private ShapeModel? selectedTemplate;

    [ObservableProperty]
    private bool isLoadingTemplates;

    [ObservableProperty]
    private bool hasTemplates;

    [ObservableProperty]
    private bool isSavingTemplate;

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
        
        InitializeAutoSave();
    }

    private void InitializeAutoSave()
    {
        _autoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;
    }

    private async void AutoSaveTimer_Tick(object? sender, object e)
    {
        await PerformAutoSaveAsync();
    }

    private async Task PerformAutoSaveAsync()
    {
        if (!_hasUnsavedChanges || CurrentCanvas == null || IsAutoSaving)
            return;

        try
        {
            IsAutoSaving = true;
            AutoSaveStatus = "Saving...";

            // Update canvas timestamp
            CurrentCanvas.UpdatedAt = DateTime.Now;
            
            // Save canvas to database
            await _canvasService.UpdateCanvasAsync(CurrentCanvas);
            
            // Update status
            LastAutoSavedAt = DateTime.Now;
            AutoSaveStatus = $"Saved at {LastAutoSavedAt:HH:mm:ss}";
            _hasUnsavedChanges = false;
            
            // Update canvas info display
            CanvasInfo = $"{CurrentCanvas.Name} - {CurrentCanvas.Width} × {CurrentCanvas.Height} (Auto-saved: {LastAutoSavedAt:HH:mm:ss})";
        }
        catch (Exception ex)
        {
            AutoSaveStatus = "Auto-save failed";
            System.Diagnostics.Debug.WriteLine($"Auto-save error: {ex.Message}");
        }
        finally
        {
            IsAutoSaving = false;
        }
    }

    public void StartAutoSave()
    {
        _autoSaveTimer?.Start();
        AutoSaveStatus = "Auto-save enabled";
    }

    public void StopAutoSave()
    {
        _autoSaveTimer?.Stop();
        AutoSaveStatus = "Auto-save disabled";
    }

    private void MarkAsUnsaved()
    {
        _hasUnsavedChanges = true;
        if (LastAutoSavedAt.HasValue)
        {
            AutoSaveStatus = "Unsaved changes";
        }
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
        
        // Load templates
        await LoadTemplatesAsync();
        
        // Start auto-save
        StartAutoSave();
        
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

    public async Task LoadTemplatesAsync()
    {
        if (IsLoadingTemplates)
        {
            System.Diagnostics.Debug.WriteLine("LoadTemplates: Already loading, skipping...");
            return;
        }

        IsLoadingTemplates = true;
        try
        {
            System.Diagnostics.Debug.WriteLine("LoadTemplates: Starting load...");
            
            var templates = await _shapeService.GetTemplateShapesAsync();
            
            System.Diagnostics.Debug.WriteLine($"LoadTemplates: Loaded {templates.Count} templates from database");
            
            // Remove duplicates based on Type, GeometryData, and colors
            var uniqueTemplates = templates
                .GroupBy(t => new { t.Type, t.GeometryData, t.StrokeColor, t.FillColor, t.StrokeThickness })
                .Select(g => g.OrderByDescending(t => t.UsageCount).ThenBy(t => t.Id).First())
                .ToList();
            
            if (uniqueTemplates.Count < templates.Count)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTemplates: Found {templates.Count - uniqueTemplates.Count} duplicate templates");
            }
            
            // Clear and reload on UI thread
            TemplateShapes.Clear();
            foreach (var template in uniqueTemplates)
            {
                TemplateShapes.Add(template);
            }
            
            HasTemplates = TemplateShapes.Count > 0;
            
            System.Diagnostics.Debug.WriteLine($"LoadTemplates: UI updated with {TemplateShapes.Count} unique templates");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadTemplates ERROR: {ex.Message}");
            await ShowErrorDialogAsync("Load Templates Error", $"Failed to load templates: {ex.Message}");
        }
        finally
        {
            IsLoadingTemplates = false;
        }
    }

    [RelayCommand]
    private async Task CleanupDuplicateTemplatesAsync()
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = "Cleanup Duplicate Templates?",
            Content = "This will remove duplicate templates from the database.\n\n" +
                     "Templates with higher usage count will be kept.\n\n" +
                     "Do you want to continue?",
            PrimaryButtonText = "Cleanup",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            var allTemplates = await _shapeService.GetTemplateShapesAsync();
            
            // Group by similarity
            var groups = allTemplates
                .GroupBy(t => new { t.Type, t.GeometryData, t.StrokeColor, t.FillColor, t.StrokeThickness })
                .Where(g => g.Count() > 1)
                .ToList();

            int deletedCount = 0;
            foreach (var group in groups)
            {
                // Keep the one with highest usage or oldest
                var toKeep = group.OrderByDescending(t => t.UsageCount).ThenBy(t => t.Id).First();
                var toDelete = group.Where(t => t.Id != toKeep.Id).ToList();

                foreach (var template in toDelete)
                {
                    await _shapeService.DeleteShapeAsync(template.Id);
                    deletedCount++;
                }
            }

            await LoadTemplatesAsync();

            await ShowSuccessDialogAsync("Cleanup Complete", 
                $"Removed {deletedCount} duplicate template(s).");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Cleanup Error", 
                $"Failed to cleanup templates: {ex.Message}");
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
            
            // Mark as unsaved for auto-save
            MarkAsUnsaved();
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
            
            // Mark as unsaved for auto-save
            MarkAsUnsaved();
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Update Shape Error", $"Failed to update shape: {ex.Message}");
        }
    }

    public async Task DeleteShapeAsync(ShapeModel shape)
    {
        try
        {
            await _shapeService.DeleteShapeAsync(shape.Id);
            
            Shapes.Remove(shape);
            ShapeDeleted?.Invoke(this, shape);
            
            // Mark as unsaved for auto-save
            MarkAsUnsaved();
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Delete Shape Error", $"Failed to delete shape: {ex.Message}");
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
        
        // Populate editable properties
        SelectedShapeX = shape.X;
        SelectedShapeY = shape.Y;
        SelectedShapeWidth = shape.Width;
        SelectedShapeHeight = shape.Height;
        SelectedShapeStrokeColor = ParseColor(shape.StrokeColor);
        SelectedShapeStrokeThickness = shape.StrokeThickness;
        
        if (!string.IsNullOrEmpty(shape.FillColor))
        {
            SelectedShapeFillColor = ParseColor(shape.FillColor);
            SelectedShapeHasFill = true;
        }
        else
        {
            SelectedShapeFillColor = Colors.Transparent;
            SelectedShapeHasFill = false;
        }
    }

    public void ClearSelection()
    {
        SelectedShape = null;
        IsShapeSelected = false;
        SelectedShapeInfo = "No shape selected";
        
        // Reset editable properties
        SelectedShapeX = 0;
        SelectedShapeY = 0;
        SelectedShapeWidth = 0;
        SelectedShapeHeight = 0;
        SelectedShapeStrokeColor = Colors.Black;
        SelectedShapeStrokeThickness = 2.0;
        SelectedShapeFillColor = Colors.Transparent;
        SelectedShapeHasFill = false;
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
    private async Task SaveAsync()
    {
        if (CurrentCanvas == null)
        {
            await ShowErrorDialogAsync("Save Error", "No canvas loaded to save.");
            return;
        }

        try
        {
            // Update canvas timestamp
            CurrentCanvas.UpdatedAt = DateTime.Now;
            
            // Save canvas to database
            await _canvasService.UpdateCanvasAsync(CurrentCanvas);
            
            // Reset unsaved changes flag
            _hasUnsavedChanges = false;
            LastAutoSavedAt = DateTime.Now;
            AutoSaveStatus = $"Saved at {LastAutoSavedAt:HH:mm:ss}";
            
            // Update canvas info display
            CanvasInfo = $"{CurrentCanvas.Name} - {CurrentCanvas.Width} × {CurrentCanvas.Height} (Saved: {CurrentCanvas.UpdatedAt:HH:mm:ss})";
            
            await ShowSuccessDialogAsync("Canvas Saved", 
                $"Canvas '{CurrentCanvas.Name}' has been saved successfully.\n" +
                $"Total shapes: {Shapes.Count}\n" +
                $"Last saved: {CurrentCanvas.UpdatedAt:HH:mm:ss}");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Save Error", 
                $"Failed to save canvas: {ex.Message}");
        }
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

    // Property change handlers for selected shape (update and save)
    partial void OnSelectedShapeStrokeColorChanged(Color value)
    {
        if (SelectedShape != null && ColorToHex(value) != SelectedShape.StrokeColor)
        {
            SelectedShape.StrokeColor = ColorToHex(value);
            _ = UpdateAndSaveSelectedShapeAsync();
        }
    }

    partial void OnSelectedShapeStrokeThicknessChanged(double value)
    {
        if (SelectedShape != null && Math.Abs(SelectedShape.StrokeThickness - value) > 0.001)
        {
            SelectedShape.StrokeThickness = value;
            _ = UpdateAndSaveSelectedShapeAsync();
        }
    }

    partial void OnSelectedShapeFillColorChanged(Color? value)
    {
        if (SelectedShape != null)
        {
            var newFillColor = value.HasValue && SelectedShapeHasFill ? ColorToHex(value.Value) : null;
            if (newFillColor != SelectedShape.FillColor)
            {
                SelectedShape.FillColor = newFillColor;
                _ = UpdateAndSaveSelectedShapeAsync();
            }
        }
    }

    partial void OnSelectedShapeHasFillChanged(bool value)
    {
        if (SelectedShape != null)
        {
            var newFillColor = value && SelectedShapeFillColor.HasValue ? ColorToHex(SelectedShapeFillColor.Value) : null;
            if (newFillColor != SelectedShape.FillColor)
            {
                SelectedShape.FillColor = newFillColor;
                _ = UpdateAndSaveSelectedShapeAsync();
            }
        }
    }

    private async Task UpdateAndSaveSelectedShapeAsync()
    {
        if (SelectedShape == null) return;

        try
        {
            // Save to database (no need to update geometry for color/thickness changes)
            await UpdateShapeAsync(SelectedShape);
            
            // Notify UI to update visual
            ShapeUpdated?.Invoke(this, SelectedShape);
            
            // Mark as unsaved for auto-save
            MarkAsUnsaved();
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Update Error", $"Failed to update shape: {ex.Message}");
        }
    }

    private async Task UpdateShapeGeometryDataAsync(ShapeModel shape)
    {
        try
        {
            switch (shape.Type)
            {
                case "Line":
                    // Line: update endpoints based on position and size
                    var linePoints = new List<Point>
                    {
                        new Point(shape.X, shape.Y),
                        new Point(shape.X + shape.Width, shape.Y + shape.Height)
                    };
                    shape.GeometryData = DrawingHelper.PointsToJson(linePoints);
                    break;

                case "Rectangle":
                    // Rectangle: update rect based on position and size
                    var rect = new Windows.Foundation.Rect(shape.X, shape.Y, shape.Width, shape.Height);
                    shape.GeometryData = DrawingHelper.RectToJson(rect);
                    break;

                case "Circle":
                    // Circle: update center and radius
                    var circleCenterX = shape.X + shape.Width / 2;
                    var circleCenterY = shape.Y + shape.Height / 2;
                    var radius = Math.Max(shape.Width, shape.Height) / 2;
                    
                    shape.GeometryData = JsonSerializer.Serialize(new
                    {
                        centerX = circleCenterX,
                        centerY = circleCenterY,
                        radius = radius
                    });
                    break;

                case "Oval":
                    // Oval: update center and radii
                    var ovalCenterX = shape.X + shape.Width / 2;
                    var ovalCenterY = shape.Y + shape.Height / 2;
                    var radiusX = shape.Width / 2;
                    var radiusY = shape.Height / 2;
                    
                    shape.GeometryData = JsonSerializer.Serialize(new
                    {
                        centerX = ovalCenterX,
                        centerY = ovalCenterY,
                        radiusX = radiusX,
                        radiusY = radiusY
                    });
                    break;

                case "Triangle":
                case "Polygon":
                    // For polygons, we need to scale points proportionally
                    var doc = JsonDocument.Parse(shape.GeometryData);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("points", out var pointsArray))
                    {
                        var points = new List<Point>();
                        var originalMinX = double.MaxValue;
                        var originalMinY = double.MaxValue;
                        var originalMaxX = double.MinValue;
                        var originalMaxY = double.MinValue;

                        // Get original bounds
                        foreach (var pointElement in pointsArray.EnumerateArray())
                        {
                            var x = pointElement.GetProperty("x").GetDouble();
                            var y = pointElement.GetProperty("y").GetDouble();
                            originalMinX = Math.Min(originalMinX, x);
                            originalMinY = Math.Min(originalMinY, y);
                            originalMaxX = Math.Max(originalMaxX, x);
                            originalMaxY = Math.Max(originalMaxY, y);
                            points.Add(new Point(x, y));
                        }

                        var originalWidth = originalMaxX - originalMinX;
                        var originalHeight = originalMaxY - originalMinY;

                        // Scale and reposition points
                        var scaledPoints = new List<object>();
                        foreach (var point in points)
                        {
                            var normalizedX = originalWidth > 0 ? (point.X - originalMinX) / originalWidth : 0;
                            var normalizedY = originalHeight > 0 ? (point.Y - originalMinY) / originalHeight : 0;
                            
                            var newX = shape.X + normalizedX * shape.Width;
                            var newY = shape.Y + normalizedY * shape.Height;
                            
                            scaledPoints.Add(new { x = newX, y = newY });
                        }

                        shape.GeometryData = JsonSerializer.Serialize(new { points = scaledPoints });
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating geometry data: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedShapeAsync()
    {
        if (SelectedShape == null)
        {
            await ShowErrorDialogAsync("Error", "No shape selected.");
            return;
        }

        try
        {
            // Delete from database
            await _shapeService.DeleteShapeAsync(SelectedShape.Id);
            
            // Remove from collection
            Shapes.Remove(SelectedShape);
            
            // Notify UI
            ShapeDeleted?.Invoke(this, SelectedShape);
            
            // Clear selection
            ClearSelection();
            
            await ShowSuccessDialogAsync("Success", "Shape deleted successfully.");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Delete Error", $"Failed to delete shape: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveShapeToTemplateAsync()
    {
        // Prevent duplicate calls
        if (isSavingTemplate)
        {
            System.Diagnostics.Debug.WriteLine("SaveShapeToTemplate: Already in progress, skipping...");
            return;
        }

        if (SelectedShape == null)
        {
            await ShowErrorDialogAsync("Error", "No shape selected.");
            return;
        }

        // Check if this shape is already a template
        if (SelectedShape.IsTemplate)
        {
            await ShowErrorDialogAsync("Already Template", 
                "This shape is already saved as a template.");
            return;
        }

        isSavingTemplate = true;
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"SaveShapeToTemplate: Starting save for shape {SelectedShape.Type}");
            
            // Create a deep copy of the SELECTED shape ONLY
            var templateShape = new ShapeModel
            {
                Type = SelectedShape.Type,
                GeometryData = SelectedShape.GeometryData,
                StrokeColor = SelectedShape.StrokeColor,
                StrokeThickness = SelectedShape.StrokeThickness,
                FillColor = SelectedShape.FillColor,
                X = SelectedShape.X,
                Y = SelectedShape.Y,
                Width = SelectedShape.Width,
                Height = SelectedShape.Height,
                ZIndex = 0,
                IsTemplate = true,        // Mark as template
                CanvasId = null,          // Detach from canvas
                DrawingId = null,         // Detach from drawing
                CreatedAt = DateTime.Now,
                UsageCount = 0
            };

            // Save ONCE to database
            var savedTemplate = await _shapeService.CreateShapeAsync(templateShape);
            
            System.Diagnostics.Debug.WriteLine($"SaveShapeToTemplate: Template saved with ID {savedTemplate.Id}");

            // Reload templates on UI thread
            await LoadTemplatesAsync();

            await ShowSuccessDialogAsync("Template Saved", 
                $"Shape '{savedTemplate.Type}' (ID: {savedTemplate.Id}) has been saved as a template!\n\n" +
                $"You can now reuse this shape in other canvases.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveShapeToTemplate ERROR: {ex.Message}");
            await ShowErrorDialogAsync("Save Template Error", 
                $"Failed to save shape as template: {ex.Message}");
        }
        finally
        {
            isSavingTemplate = false;
        }
    }

    [RelayCommand]
    private async Task UseTemplateAsync(ShapeModel? template)
    {
        if (template == null)
        {
            await ShowErrorDialogAsync("Error", "No template selected.");
            return;
        }

        if (CurrentCanvas == null)
        {
            await ShowErrorDialogAsync("Error", "No canvas loaded.");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"UseTemplate: Using template {template.Type} (ID: {template.Id})");
            
            // Create a new shape from template
            // Position it at center of canvas
            var newShape = new ShapeModel
            {
                Type = template.Type,
                GeometryData = template.GeometryData,
                StrokeColor = template.StrokeColor,
                StrokeThickness = template.StrokeThickness,
                FillColor = template.FillColor,
                X = (CanvasWidth - template.Width) / 2,  // Center horizontally
                Y = (CanvasHeight - template.Height) / 2, // Center vertically
                Width = template.Width,
                Height = template.Height,
                ZIndex = Shapes.Count,
                IsTemplate = false,
                CanvasId = CurrentCanvas.Id,
                DrawingId = null,
                CreatedAt = DateTime.Now,
                UsageCount = 0
            };

            // Update geometry data to new position
            newShape = UpdateShapeGeometryForPosition(newShape);

            System.Diagnostics.Debug.WriteLine($"UseTemplate: Saving new shape to canvas {CurrentCanvas.Id}");

            // Save to database
            var savedShape = await _shapeService.CreateShapeAsync(newShape);
            
            System.Diagnostics.Debug.WriteLine($"UseTemplate: Shape created with ID {savedShape.Id}");
            
            // Add to collection on UI thread
            Shapes.Add(savedShape);
            ShapeCreated?.Invoke(this, savedShape);

            // Update template usage count
            System.Diagnostics.Debug.WriteLine($"UseTemplate: Updating template usage count");
            template.UsageCount++;
            await _shapeService.UpdateShapeAsync(template);

            // Reload templates to show updated usage count
            await LoadTemplatesAsync();

            // Mark as unsaved
            MarkAsUnsaved();

            // Select the new shape
            SelectedTool = "Select";
            SelectShapeCommand(savedShape);
            
            System.Diagnostics.Debug.WriteLine($"UseTemplate: Completed successfully");
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"UseTemplate ERROR (InvalidOperation): {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            await ShowErrorDialogAsync("Use Template Error", 
                $"Operation failed: {ex.Message}\n\nPlease try again.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UseTemplate ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            await ShowErrorDialogAsync("Use Template Error", 
                $"Failed to use template: {ex.Message}");
        }
    }

    private ShapeModel UpdateShapeGeometryForPosition(ShapeModel shape)
    {
        try
        {
            var centerX = shape.X + shape.Width / 2;
            var centerY = shape.Y + shape.Height / 2;

            switch (shape.Type)
            {
                case "Line":
                    var linePoints = new List<Point>
                    {
                        new Point(shape.X, shape.Y),
                        new Point(shape.X + shape.Width, shape.Y + shape.Height)
                    };
                    shape.GeometryData = DrawingHelper.PointsToJson(linePoints);
                    break;

                case "Rectangle":
                    var rect = new Windows.Foundation.Rect(shape.X, shape.Y, shape.Width, shape.Height);
                    shape.GeometryData = DrawingHelper.RectToJson(rect);
                    break;

                case "Circle":
                    var radius = Math.Max(shape.Width, shape.Height) / 2;
                    shape.GeometryData = JsonSerializer.Serialize(new
                    {
                        centerX = centerX,
                        centerY = centerY,
                        radius = radius
                    });
                    break;

                case "Oval":
                    var radiusX = shape.Width / 2;
                    var radiusY = shape.Height / 2;
                    shape.GeometryData = JsonSerializer.Serialize(new
                    {
                        centerX = centerX,
                        centerY = centerY,
                        radiusX = radiusX,
                        radiusY = radiusY
                    });
                    break;

                case "Triangle":
                case "Polygon":
                    // Parse existing geometry and reposition
                    var doc = JsonDocument.Parse(shape.GeometryData);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("points", out var pointsArray))
                    {
                        var points = new List<Point>();
                        var originalMinX = double.MaxValue;
                        var originalMinY = double.MaxValue;
                        var originalMaxX = double.MinValue;
                        var originalMaxY = double.MinValue;

                        foreach (var pointElement in pointsArray.EnumerateArray())
                        {
                            var x = pointElement.GetProperty("x").GetDouble();
                            var y = pointElement.GetProperty("y").GetDouble();
                            originalMinX = Math.Min(originalMinX, x);
                            originalMinY = Math.Min(originalMinY, y);
                            originalMaxX = Math.Max(originalMaxX, x);
                            originalMaxY = Math.Max(originalMaxY, y);
                            points.Add(new Point(x, y));
                        }

                        var originalWidth = originalMaxX - originalMinX;
                        var originalHeight = originalMaxY - originalMinY;

                        // Reposition points
                        var repositionedPoints = new List<object>();
                        foreach (var point in points)
                        {
                            var normalizedX = originalWidth > 0 ? (point.X - originalMinX) / originalWidth : 0;
                            var normalizedY = originalHeight > 0 ? (point.Y - originalMinY) / originalHeight : 0;
                            
                            var newX = shape.X + normalizedX * shape.Width;
                            var newY = shape.Y + normalizedY * shape.Height;
                            
                            repositionedPoints.Add(new { x = newX, y = newY });
                        }

                        shape.GeometryData = JsonSerializer.Serialize(new { points = repositionedPoints });
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating geometry for position: {ex.Message}");
        }

        return shape;
    }

    [RelayCommand]
    private async Task DeleteTemplateAsync(ShapeModel? template)
    {
        if (template == null)
        {
            await ShowErrorDialogAsync("Error", "No template selected.");
            return;
        }

        if (_xamlRoot == null) return;

        // Show confirmation dialog
        var dialog = new ContentDialog
        {
            Title = "Delete Template?",
            Content = $"Are you sure you want to delete the '{template.Type}' template?\n\n" +
                     $"This template has been used {template.UsageCount} time(s).\n\n" +
                     $"?? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                await _shapeService.DeleteShapeAsync(template.Id);
                TemplateShapes.Remove(template);
                HasTemplates = TemplateShapes.Count > 0;

                await ShowSuccessDialogAsync("Template Deleted", 
                    $"Template '{template.Type}' has been deleted successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Delete Template Error", 
                    $"Failed to delete template: {ex.Message}");
            }
        }
    }
}
