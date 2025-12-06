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
using System.Text.Json;
using System.Linq;

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
            double centerX, centerY, radiusX, radiusY;
            
            // Handle Circle with new format
            if (shape.Type == "Circle")
            {
                try
                {
                    var doc = JsonDocument.Parse(shape.GeometryData);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("centerX", out var cx) &&
                        root.TryGetProperty("centerY", out var cy) &&
                        root.TryGetProperty("radius", out var r))
                    {
                        // New circle format
                        centerX = cx.GetDouble();
                        centerY = cy.GetDouble();
                        var radius = r.GetDouble();
                        radiusX = radius;
                        radiusY = radius;
                    }
                    else
                    {
                        // Fallback to rect format for backward compatibility
                        var rect = DrawingHelper.JsonToRect(shape.GeometryData);
                        centerX = rect.X + rect.Width / 2;
                        centerY = rect.Y + rect.Height / 2;
                        radiusX = rect.Width / 2;
                        radiusY = rect.Height / 2;
                    }
                }
                catch
                {
                    // Fallback to rect format
                    var rect = DrawingHelper.JsonToRect(shape.GeometryData);
                    centerX = rect.X + rect.Width / 2;
                    centerY = rect.Y + rect.Height / 2;
                    radiusX = rect.Width / 2;
                    radiusY = rect.Height / 2;
                }
            }
            else // Oval
            {
                // Try to parse as oval format first (centerX, centerY, radiusX, radiusY)
                try
                {
                    var doc = JsonDocument.Parse(shape.GeometryData);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("centerX", out var cx) &&
                        root.TryGetProperty("centerY", out var cy) &&
                        root.TryGetProperty("radiusX", out var rx) &&
                        root.TryGetProperty("radiusY", out var ry))
                    {
                        centerX = cx.GetDouble();
                        centerY = cy.GetDouble();
                        radiusX = rx.GetDouble();
                        radiusY = ry.GetDouble();
                    }
                    else
                    {
                        // Fallback to rect format for backward compatibility
                        var rect = DrawingHelper.JsonToRect(shape.GeometryData);
                        centerX = rect.X + rect.Width / 2;
                        centerY = rect.Y + rect.Height / 2;
                        radiusX = rect.Width / 2;
                        radiusY = rect.Height / 2;
                    }
                }
                catch
                {
                    // Fallback to rect format
                    var rect = DrawingHelper.JsonToRect(shape.GeometryData);
                    centerX = rect.X + rect.Width / 2;
                    centerY = rect.Y + rect.Height / 2;
                    radiusX = rect.Width / 2;
                    radiusY = rect.Height / 2;
                }
            }
            
            var ellipse = new Ellipse
            {
                Width = radiusX * 2,
                Height = radiusY * 2,
                Stroke = new SolidColorBrush(ParseColor(shape.StrokeColor)),
                StrokeThickness = shape.StrokeThickness
            };

            if (!string.IsNullOrEmpty(shape.FillColor))
            {
                ellipse.Fill = new SolidColorBrush(ParseColor(shape.FillColor));
            }

            // Position ellipse so center is at centerX, centerY
            XamlCanvas.SetLeft(ellipse, centerX - radiusX);
            XamlCanvas.SetTop(ellipse, centerY - radiusY);

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
            List<Point> points;
            
            if (shape.Type == "Triangle" || shape.Type == "Polygon")
            {
                // Parse triangle/polygon format: {"points": [{"x": x1, "y": y1}, ...]}
                try
                {
                    var doc = JsonDocument.Parse(shape.GeometryData);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("points", out var pointsArray))
                    {
                        points = new List<Point>();
                        foreach (var pointElement in pointsArray.EnumerateArray())
                        {
                            var x = pointElement.GetProperty("x").GetDouble();
                            var y = pointElement.GetProperty("y").GetDouble();
                            points.Add(new Point(x, y));
                        }
                    }
                    else
                    {
                        // Fallback to old format
                        points = DrawingHelper.JsonToPoints(shape.GeometryData);
                    }
                }
                catch
                {
                    // Fallback to old format
                    points = DrawingHelper.JsonToPoints(shape.GeometryData);
                }
            }
            else
            {
                // Fallback for other polygon types
                points = DrawingHelper.JsonToPoints(shape.GeometryData);
            }
            
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

        // Handle Triangle with direct drawing (not polygon mode)
        if (ViewModel.SelectedTool == "Triangle")
        {
            HandleTriangleClick(currentPoint);
            return;
        }

        // Handle Polygon with multi-click drawing
        if (ViewModel.SelectedTool == "Polygon")
        {
            HandlePolygonClick(currentPoint);
            return;
        }

        // Regular shapes
        _startPoint = currentPoint;
        _isDrawing = true;
        
        _currentShape = ViewModel.SelectedTool switch
        {
            "Line" => CreateLine(_startPoint),
            "Rectangle" => CreateRectangle(),
            "Oval" or "Circle" => CreateEllipse(),
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

    private Line CreateLine(Point startPoint)
    {
        var line = new Line
        {
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = startPoint.X,
            Y2 = startPoint.Y,
            Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
            StrokeThickness = ViewModel.CurrentStrokeThickness
        };
        
        ApplyDashStyle(line);
        return line;
    }

    private Rectangle CreateRectangle()
    {
        var rectangle = new Rectangle
        {
            Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
            Fill = GetFillBrush(),
            StrokeThickness = ViewModel.CurrentStrokeThickness
        };
        
        ApplyDashStyle(rectangle);
        return rectangle;
    }

    private Ellipse CreateEllipse()
    {
        var ellipse = new Ellipse
        {
            Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
            Fill = GetFillBrush(),
            StrokeThickness = ViewModel.CurrentStrokeThickness
        };
        
        ApplyDashStyle(ellipse);
        return ellipse;
    }

    private SolidColorBrush GetFillBrush()
    {
        if (!ViewModel.IsFillEnabled)
            return new SolidColorBrush(Colors.Transparent);
        
        return new SolidColorBrush(ViewModel.CurrentFillColor ?? Colors.Transparent);
    }

    private void ApplyDashStyle(XamlShape shape)
    {
        if (ViewModel.SelectedDashStyle == "Solid")
            return;
        
        shape.StrokeDashArray = ViewModel.SelectedDashStyle switch
        {
            "Dash" => new DoubleCollection { 4, 2 },
            "Dot" => new DoubleCollection { 1, 2 },
            "DashDot" => new DoubleCollection { 4, 2, 1, 2 },
            "DashDotDot" => new DoubleCollection { 4, 2, 1, 2, 1, 2 },
            _ => null
        };
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
                var rectBounds = DrawingHelper.CalculateBoundingRect(_startPoint, currentPoint);
                rect.Width = rectBounds.Width;
                rect.Height = rectBounds.Height;
                XamlCanvas.SetLeft(rect, rectBounds.X);
                XamlCanvas.SetTop(rect, rectBounds.Y);
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

            case Polygon polygon when ViewModel.SelectedTool == "Triangle":
                // Update triangle points based on current mouse position
                var trianglePoints = DrawingHelper.CalculateTrianglePoints(_startPoint, currentPoint);
                polygon.Points.Clear();
                foreach (var point in trianglePoints)
                {
                    polygon.Points.Add(point);
                }
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
        var bounds = DrawingHelper.CalculateBoundingRect(startPoint, endPoint);
        if (!DrawingHelper.IsValidShapeSize(bounds.Width, bounds.Height, minSize: 1))
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
                geometryData = DrawingHelper.RectToJson(bounds);
                break;

            case "Oval":
                // Calculate oval parameters
                var centerX = (startPoint.X + endPoint.X) / 2;
                var centerY = (startPoint.Y + endPoint.Y) / 2;
                var radiusX = Math.Abs(endPoint.X - startPoint.X) / 2;
                var radiusY = Math.Abs(endPoint.Y - startPoint.Y) / 2;
                
                var ovalData = new
                {
                    centerX = centerX,
                    centerY = centerY,
                    radiusX = radiusX,
                    radiusY = radiusY
                };
                geometryData = JsonSerializer.Serialize(ovalData);
                break;

            case "Circle":
                // Calculate circle parameters
                var circleCenterX = (startPoint.X + endPoint.X) / 2;
                var circleCenterY = (startPoint.Y + endPoint.Y) / 2;
                var radius = Math.Max(
                    Math.Abs(endPoint.X - startPoint.X) / 2,
                    Math.Abs(endPoint.Y - startPoint.Y) / 2
                );
                
                var circleData = new
                {
                    centerX = circleCenterX,
                    centerY = circleCenterY,
                    radius = radius
                };
                geometryData = JsonSerializer.Serialize(circleData);
                break;

            case "Triangle":
                // Calculate triangle points from start and end
                var trianglePoints = DrawingHelper.CalculateTrianglePoints(startPoint, endPoint);
                var trianglePointData = trianglePoints.Select(p => new { x = p.X, y = p.Y });
                var triangleData = new { points = trianglePointData };
                geometryData = JsonSerializer.Serialize(triangleData);
                break;
        }

        if (string.IsNullOrEmpty(geometryData))
            return;

        var shape = new ShapeModel
        {
            Type = shapeType,
            StrokeColor = ColorToHex(ViewModel.CurrentStrokeColor),
            FillColor = ViewModel.IsFillEnabled && ViewModel.CurrentFillColor.HasValue 
                ? ColorToHex(ViewModel.CurrentFillColor.Value) 
                : null,
            StrokeThickness = ViewModel.CurrentStrokeThickness,
            GeometryData = geometryData,
            X = bounds.X,
            Y = bounds.Y,
            Width = bounds.Width,
            Height = bounds.Height,
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
        // Check if clicking on first point to close polygon
        if (ViewModel.IsDrawingPolygon && ViewModel.PolygonPoints.Count >= 3)
        {
            var firstPoint = ViewModel.PolygonPoints[0];
            var distance = DrawingHelper.CalculateDistance(point, firstPoint);
            
            // If clicking close to first point, complete polygon
            if (distance <= 10) // 10 pixel tolerance
            {
                CompletePolygon();
                return;
            }
        }

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

        // Triangle auto-complete is handled in ViewModel
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
            Fill = GetFillBrush(),
            StrokeThickness = ViewModel.CurrentStrokeThickness
        };
        
        ApplyDashStyle(polygon);

        foreach (var point in ViewModel.PolygonPoints)
        {
            polygon.Points.Add(point);
        }

        ClearTemporaryPolygonDrawing();
        DrawingCanvas.Children.Add(polygon);

        // Save to database with points array format
        var polygonPointData = ViewModel.PolygonPoints.Select(p => new { x = p.X, y = p.Y });
        var polygonData = new { points = polygonPointData };
        var geometryData = JsonSerializer.Serialize(polygonData);

        var bounds = DrawingHelper.CalculateBoundingRect(
            new Point(ViewModel.PolygonPoints.Min(p => p.X), ViewModel.PolygonPoints.Min(p => p.Y)),
            new Point(ViewModel.PolygonPoints.Max(p => p.X), ViewModel.PolygonPoints.Max(p => p.Y))
        );

        var shape = new ShapeModel
        {
            Type = ViewModel.SelectedTool,
            StrokeColor = ColorToHex(ViewModel.CurrentStrokeColor),
            FillColor = ViewModel.IsFillEnabled && ViewModel.CurrentFillColor.HasValue 
                ? ColorToHex(ViewModel.CurrentFillColor.Value) 
                : null,
            StrokeThickness = ViewModel.CurrentStrokeThickness,
            GeometryData = geometryData,
            X = bounds.X,
            Y = bounds.Y,
            Width = bounds.Width,
            Height = bounds.Height,
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

    private void HandleTriangleClick(Point point)
    {
        // Triangle uses direct drawing (not polygon mode)
        _startPoint = point;
        _isDrawing = true;
        
        // Create preview triangle
        var trianglePoints = DrawingHelper.CalculateTrianglePoints(_startPoint, _startPoint);
        _currentShape = new Polygon
        {
            Stroke = new SolidColorBrush(ViewModel.CurrentStrokeColor),
            Fill = GetFillBrush(),
            StrokeThickness = ViewModel.CurrentStrokeThickness
        };
        
        ApplyDashStyle(_currentShape);
        
        foreach (var p in trianglePoints)
        {
            ((Polygon)_currentShape).Points.Add(p);
        }
        
        DrawingCanvas.Children.Add(_currentShape);
        DrawingCanvas.CapturePointer(null);
    }
}
