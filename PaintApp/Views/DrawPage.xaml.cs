using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using PaintApp.Helpers;
using Windows.Foundation;
using Windows.UI;
using XamlShape = Microsoft.UI.Xaml.Shapes.Shape;
using XamlCanvas = Microsoft.UI.Xaml.Controls.Canvas;
using CanvasModel = PaintApp.Models.Canvas;
using ShapeModel = PaintApp.Models.Shape;

namespace PaintApp.Views;

public sealed partial class DrawPage : Page
{
    public DrawPageViewModel ViewModel { get; }
    private Point _startPoint;
    private Point? _endPoint;
    private bool _isDrawing;
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
        ViewModel.ShapeCreated += ViewModel_ShapeCreated;
    }

    private void DrawPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetXamlRoot(this.XamlRoot);
    }

    private void ViewModel_CanvasLoaded(object? sender, CanvasModel canvas)
    {
        DrawingCanvas.Width = canvas.Width;
        DrawingCanvas.Height = canvas.Height;
        DrawingCanvas.Background = new SolidColorBrush(ParseColor(canvas.BackgroundColor));
        DrawingCanvas.Children.Clear();
        
        // Render existing shapes
        RenderShapes();
    }

    private void ViewModel_ShapeCreated(object? sender, ShapeModel shape)
    {
        // Shape already added to collection, just render it
        RenderShape(shape);
    }

    private void RenderShapes()
    {
        foreach (var shape in ViewModel.Shapes)
        {
            RenderShape(shape);
        }
    }

    private void RenderShape(ShapeModel shape)
    {
        XamlShape? xamlShape = null;

        switch (shape.Type)
        {
            case "Line":
                xamlShape = RenderLine(shape);
                break;
            case "Rectangle":
                xamlShape = RenderRectangle(shape);
                break;
            case "Oval":
            case "Circle":
                xamlShape = RenderEllipse(shape);
                break;
            case "Triangle":
            case "Polygon":
                xamlShape = RenderPolygon(shape);
                break;
        }

        if (xamlShape != null)
        {
            DrawingCanvas.Children.Add(xamlShape);
        }
    }

    private Line? RenderLine(ShapeModel shape)
    {
        try
        {
            var points = DrawingHelper.JsonToPoints(shape.GeometryData);
            if (points.Count < 2) return null;

            var line = new Line
            {
                X1 = points[0].X,
                Y1 = points[0].Y,
                X2 = points[1].X,
                Y2 = points[1].Y,
                Stroke = new SolidColorBrush(ParseColor(shape.StrokeColor)),
                StrokeThickness = shape.StrokeThickness
            };

            return line;
        }
        catch
        {
            return null;
        }
    }

    private Rectangle? RenderRectangle(ShapeModel shape)
    {
        try
        {
            var rect = DrawingHelper.JsonToRect(shape.GeometryData);
            
            var rectangle = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Stroke = new SolidColorBrush(ParseColor(shape.StrokeColor)),
                StrokeThickness = shape.StrokeThickness
            };

            if (!string.IsNullOrEmpty(shape.FillColor))
            {
                rectangle.Fill = new SolidColorBrush(ParseColor(shape.FillColor));
            }

            XamlCanvas.SetLeft(rectangle, rect.X);
            XamlCanvas.SetTop(rectangle, rect.Y);

            return rectangle;
        }
        catch
        {
            return null;
        }
    }

    private Ellipse? RenderEllipse(ShapeModel shape)
    {
        try
        {
            var rect = DrawingHelper.JsonToRect(shape.GeometryData);
            
            var ellipse = new Ellipse
            {
                Width = rect.Width,
                Height = rect.Height,
                Stroke = new SolidColorBrush(ParseColor(shape.StrokeColor)),
                StrokeThickness = shape.StrokeThickness
            };

            if (!string.IsNullOrEmpty(shape.FillColor))
            {
                ellipse.Fill = new SolidColorBrush(ParseColor(shape.FillColor));
            }

            XamlCanvas.SetLeft(ellipse, rect.X);
            XamlCanvas.SetTop(ellipse, rect.Y);

            return ellipse;
        }
        catch
        {
            return null;
        }
    }

    private Polygon? RenderPolygon(ShapeModel shape)
    {
        try
        {
            var points = DrawingHelper.JsonToPoints(shape.GeometryData);
            if (points.Count < 3) return null;

            var polygon = new Polygon
            {
                Stroke = new SolidColorBrush(ParseColor(shape.StrokeColor)),
                StrokeThickness = shape.StrokeThickness
            };

            if (!string.IsNullOrEmpty(shape.FillColor))
            {
                polygon.Fill = new SolidColorBrush(ParseColor(shape.FillColor));
            }

            foreach (var point in points)
            {
                polygon.Points.Add(point);
            }

            return polygon;
        }
        catch
        {
            return null;
        }
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
            ViewModel.CurrentStrokeColor = color;
        }
    }

    private void FillColorPicker_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Color color)
        {
            ViewModel.CurrentFillColor = color;
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

        // Start drawing
        _startPoint = currentPoint;
        _isDrawing = true;
        
        _currentShape = ViewModel.SelectedTool switch
        {
            "Line" => new Line
            {
                X1 = _startPoint.X,
                Y1 = _startPoint.Y,
                X2 = _startPoint.X,
                Y2 = _startPoint.Y,
                Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
                StrokeThickness = ViewModel.CurrentStrokeThickness
            },
            "Rectangle" => new Rectangle
            {
                Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
                Fill = new SolidColorBrush(ViewModel.CurrentFillColor ?? Colors.Transparent),
                StrokeThickness = ViewModel.CurrentStrokeThickness
            },
            "Oval" or "Circle" => new Ellipse
            {
                Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
                Fill = new SolidColorBrush(ViewModel.CurrentFillColor ?? Colors.Transparent),
                StrokeThickness = ViewModel.CurrentStrokeThickness
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
        if (!_isDrawing || _currentShape == null) return;

        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;
        _endPoint = currentPoint;

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

    private async void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing) return;

        _isDrawing = false;
        DrawingCanvas.ReleasePointerCapture(e.Pointer);

        // Save shape to database
        if (_endPoint.HasValue)
        {
            await SaveCurrentShapeAsync(_startPoint, _endPoint.Value);
        }

        _currentShape = null;
        _endPoint = null;
    }

    private async Task SaveCurrentShapeAsync(Point startPoint, Point endPoint)
    {
        // Validate minimum size
        if (!DrawingHelper.IsValidLineLength(startPoint, endPoint, minLength: 1))
            return;

        string geometryData = string.Empty;
        string shapeType = ViewModel.SelectedTool;

        switch (ViewModel.SelectedTool)
        {
            case "Line":
                var linePoints = new List<Point> { startPoint, endPoint };
                geometryData = DrawingHelper.PointsToJson(linePoints);
                break;

            case "Rectangle":
            case "Oval":
            case "Circle":
                var rect = DrawingHelper.CalculateBoundingRect(startPoint, endPoint);
                geometryData = DrawingHelper.RectToJson(rect);
                break;
        }

        if (string.IsNullOrEmpty(geometryData))
            return;

        var shape = new ShapeModel
        {
            Type = shapeType,
            StrokeColor = ColorToHex(ViewModel.CurrentStrokeColor),
            FillColor = ViewModel.CurrentFillColor.HasValue ? ColorToHex(ViewModel.CurrentFillColor.Value) : null,
            StrokeThickness = ViewModel.CurrentStrokeThickness,
            GeometryData = geometryData,
            CreatedAt = DateTime.Now
        };

        await ViewModel.SaveShapeAsync(shape);
    }

    private void Canvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedTool == "Polygon" && ViewModel.IsDrawingPolygon && ViewModel.PolygonPoints.Count >= 3)
        {
            CompletePolygon();
        }
    }

    private void HandlePolygonClick(Point point)
    {
        if (!ViewModel.IsDrawingPolygon)
        {
            ViewModel.StartPolygonDrawing();
            ClearTemporaryPolygonDrawing();
        }

        ViewModel.AddPolygonPoint(point);
        DrawPointMarker(point);

        if (ViewModel.PolygonPoints.Count > 1)
        {
            var previousPoint = ViewModel.PolygonPoints[ViewModel.PolygonPoints.Count - 2];
            DrawTemporaryLine(previousPoint, point);
        }

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
            Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
            StrokeThickness = ViewModel.CurrentStrokeThickness,
            StrokeDashArray = new DoubleCollection { 5, 2 }
        };

        DrawingCanvas.Children.Add(line);
        _polygonLines.Add(line);
    }

    private async void CompletePolygon()
    {
        if (ViewModel.PolygonPoints.Count < 3)
            return;

        var firstPoint = ViewModel.PolygonPoints[0];
        var lastPoint = ViewModel.PolygonPoints[ViewModel.PolygonPoints.Count - 1];
        DrawTemporaryLine(lastPoint, firstPoint);

        var polygon = new Polygon
        {
            Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
            Fill = new SolidColorBrush(ViewModel.CurrentFillColor ?? Colors.Transparent),
            StrokeThickness = ViewModel.CurrentStrokeThickness
        };

        foreach (var point in ViewModel.PolygonPoints)
        {
            polygon.Points.Add(point);
        }

        ClearTemporaryPolygonDrawing();
        DrawingCanvas.Children.Add(polygon);

        // Save to database
        var shape = new ShapeModel
        {
            Type = ViewModel.SelectedTool,
            StrokeColor = ColorToHex(ViewModel.CurrentStrokeColor),
            FillColor = ViewModel.CurrentFillColor.HasValue ? ColorToHex(ViewModel.CurrentFillColor.Value) : null,
            StrokeThickness = ViewModel.CurrentStrokeThickness,
            GeometryData = DrawingHelper.PointsToJson(ViewModel.PolygonPoints),
            CreatedAt = DateTime.Now
        };

        await ViewModel.SaveShapeAsync(shape);
        ViewModel.ClearPolygonDrawing();
    }

    private void ClearTemporaryPolygonDrawing()
    {
        foreach (var line in _polygonLines)
        {
            DrawingCanvas.Children.Remove(line);
        }
        _polygonLines.Clear();

        foreach (var marker in _polygonPointMarkers)
        {
            DrawingCanvas.Children.Remove(marker);
        }
        _polygonPointMarkers.Clear();
    }
}
