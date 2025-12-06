using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI;
using PaintApp.ViewModels;
using PaintApp.Models;
using Windows.Foundation;
using Windows.UI;
using XamlShape = Microsoft.UI.Xaml.Shapes.Shape;
using XamlCanvas = Microsoft.UI.Xaml.Controls.Canvas;
using CanvasModel = PaintApp.Models.Canvas;

namespace PaintApp.Views;

public sealed partial class DrawPage : Page
{
    public DrawPageViewModel ViewModel { get; }
    private Point _startPoint;
    private XamlShape? _currentShape;
    
    // Polygon/Triangle drawing
    private List<Line> _polygonLines = new List<Line>();
    private List<Ellipse> _polygonPointMarkers = new List<Ellipse>();

    public DrawPage()
    {
        InitializeComponent();
        ViewModel = App.ServiceProvider.GetRequiredService<DrawPageViewModel>();
        DataContext = ViewModel;
        
        Loaded += DrawPage_Loaded;
        ViewModel.CanvasLoaded += ViewModel_CanvasLoaded;
    }

    private void DrawPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetXamlRoot(this.XamlRoot);
    }

    private void ViewModel_CanvasLoaded(object? sender, CanvasModel canvas)
    {
        // Update the drawing canvas size and background
        DrawingCanvas.Width = canvas.Width;
        DrawingCanvas.Height = canvas.Height;
        DrawingCanvas.Background = new SolidColorBrush(ParseColor(canvas.BackgroundColor));
        
        // Clear existing shapes
        DrawingCanvas.Children.Clear();
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is Profile profile)
        {
            ViewModel.SetProfile(profile);
        }
    }

    private void ToolButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is AppBarButton button && button.Tag is string tool)
        {
            ViewModel.SelectedTool = tool;
        }
    }

    private void StrokeColorPicker_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Color color)
        {
            ViewModel.StrokeColor = color;
        }
    }

    private void FillColorPicker_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Color color)
        {
            ViewModel.FillColor = color;
        }
    }

    private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;

        // Handle Triangle và Polygon
        if (ViewModel.SelectedTool == "Triangle" || ViewModel.SelectedTool == "Polygon")
        {
            HandlePolygonClick(currentPoint);
            return;
        }

        // Regular shapes
        _startPoint = currentPoint;
        
        _currentShape = ViewModel.SelectedTool switch
        {
            "Line" => new Line
            {
                Stroke = new SolidColorBrush(ViewModel.StrokeColor),
                StrokeThickness = ViewModel.StrokeThickness,
                X1 = _startPoint.X,
                Y1 = _startPoint.Y,
                X2 = _startPoint.X,
                Y2 = _startPoint.Y
            },
            "Rectangle" => new Rectangle
            {
                Stroke = new SolidColorBrush(ViewModel.StrokeColor),
                Fill = new SolidColorBrush(ViewModel.FillColor),
                StrokeThickness = ViewModel.StrokeThickness
            },
            "Oval" or "Circle" => new Ellipse
            {
                Stroke = new SolidColorBrush(ViewModel.StrokeColor),
                Fill = new SolidColorBrush(ViewModel.FillColor),
                StrokeThickness = ViewModel.StrokeThickness
            },
            _ => null
        };

        if (_currentShape != null && _currentShape is not Line)
        {
            XamlCanvas.SetLeft(_currentShape, _startPoint.X);
            XamlCanvas.SetTop(_currentShape, _startPoint.Y);
        }

        if (_currentShape != null)
        {
            DrawingCanvas.Children.Add(_currentShape);
        }

        DrawingCanvas.CapturePointer(e.Pointer);
    }

    private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_currentShape == null) return;

        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;

        switch (_currentShape)
        {
            case Line line:
                line.X2 = currentPoint.X;
                line.Y2 = currentPoint.Y;
                break;

            case Rectangle rect:
                var width = Math.Abs(currentPoint.X - _startPoint.X);
                var height = Math.Abs(currentPoint.Y - _startPoint.Y);
                rect.Width = width;
                rect.Height = height;
                XamlCanvas.SetLeft(rect, Math.Min(_startPoint.X, currentPoint.X));
                XamlCanvas.SetTop(rect, Math.Min(_startPoint.Y, currentPoint.Y));
                break;

            case Ellipse ellipse when ViewModel.SelectedTool == "Circle":
                var radius = Math.Max(
                    Math.Abs(currentPoint.X - _startPoint.X),
                    Math.Abs(currentPoint.Y - _startPoint.Y));
                ellipse.Width = radius;
                ellipse.Height = radius;
                XamlCanvas.SetLeft(ellipse, Math.Min(_startPoint.X, currentPoint.X));
                XamlCanvas.SetTop(ellipse, Math.Min(_startPoint.Y, currentPoint.Y));
                break;

            case Ellipse ellipse:
                var w = Math.Abs(currentPoint.X - _startPoint.X);
                var h = Math.Abs(currentPoint.Y - _startPoint.Y);
                ellipse.Width = w;
                ellipse.Height = h;
                XamlCanvas.SetLeft(ellipse, Math.Min(_startPoint.X, currentPoint.X));
                XamlCanvas.SetTop(ellipse, Math.Min(_startPoint.Y, currentPoint.Y));
                break;
        }
    }

    private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _currentShape = null;
        DrawingCanvas.ReleasePointerCapture(e.Pointer);
    }

    private void Canvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // Hoàn thành polygon khi right-click
        if (ViewModel.SelectedTool == "Polygon" && ViewModel.IsDrawingPolygon && ViewModel.PolygonPoints.Count >= 3)
        {
            CompletePolygon();
        }
    }

    private void HandlePolygonClick(Point point)
    {
        if (!ViewModel.IsDrawingPolygon)
        {
            // B?t ??u v? polygon m?i
            ViewModel.StartPolygonDrawing();
            ClearTemporaryPolygonDrawing();
        }

        // Thêm ?i?m vào polygon
        ViewModel.AddPolygonPoint(point);

        // V? marker cho ?i?m
        DrawPointMarker(point);

        // N?u có ?i?m tr??c ?ó, v? line n?i
        if (ViewModel.PolygonPoints.Count > 1)
        {
            var previousPoint = ViewModel.PolygonPoints[ViewModel.PolygonPoints.Count - 2];
            DrawTemporaryLine(previousPoint, point);
        }

        // Auto-complete triangle khi có 3 ?i?m
        if (ViewModel.SelectedTool == "Triangle" && ViewModel.PolygonPoints.Count == 3)
        {
            CompletePolygon();
        }
    }

    private void DrawPointMarker(Point point)
    {
        var marker = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = new SolidColorBrush(Colors.Red),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 2
        };

        XamlCanvas.SetLeft(marker, point.X - 4);
        XamlCanvas.SetTop(marker, point.Y - 4);

        DrawingCanvas.Children.Add(marker);
        _polygonPointMarkers.Add(marker);
    }

    private void DrawTemporaryLine(Point start, Point end)
    {
        var line = new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            Stroke = new SolidColorBrush(ViewModel.StrokeColor),
            StrokeThickness = ViewModel.StrokeThickness,
            StrokeDashArray = new DoubleCollection { 5, 2 } // Dashed line
        };

        DrawingCanvas.Children.Add(line);
        _polygonLines.Add(line);
    }

    private void CompletePolygon()
    {
        if (ViewModel.PolygonPoints.Count < 3)
            return;

        // N?i ?i?m cu?i v?i ?i?m ??u
        var firstPoint = ViewModel.PolygonPoints[0];
        var lastPoint = ViewModel.PolygonPoints[ViewModel.PolygonPoints.Count - 1];
        DrawTemporaryLine(lastPoint, firstPoint);

        // T?o polygon shape chính th?c
        var polygon = new Polygon
        {
            Stroke = new SolidColorBrush(ViewModel.StrokeColor),
            Fill = new SolidColorBrush(ViewModel.FillColor),
            StrokeThickness = ViewModel.StrokeThickness
        };

        foreach (var point in ViewModel.PolygonPoints)
        {
            polygon.Points.Add(point);
        }

        // Xóa các temporary drawings
        ClearTemporaryPolygonDrawing();

        // Thêm polygon vào canvas
        DrawingCanvas.Children.Add(polygon);

        // Reset state
        ViewModel.ClearPolygonDrawing();
    }

    private void ClearTemporaryPolygonDrawing()
    {
        // Xóa temporary lines
        foreach (var line in _polygonLines)
        {
            DrawingCanvas.Children.Remove(line);
        }
        _polygonLines.Clear();

        // Xóa point markers
        foreach (var marker in _polygonPointMarkers)
        {
            DrawingCanvas.Children.Remove(marker);
        }
        _polygonPointMarkers.Clear();
    }
}
