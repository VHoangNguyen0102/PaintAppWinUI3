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
    
    // Selection infrastructure  
    private Dictionary<ShapeModel, XamlShape> _shapeMap = new Dictionary<ShapeModel, XamlShape>();
    private Border? _selectionBorder;
    
    // Resize handles
    private List<ResizeHandle> _resizeHandles = new List<ResizeHandle>();
    private ResizeHandle? _activeHandle;
    private bool _isResizing;
    private ShapeModel? _resizingShape;
    
    // Move shape functionality
    private bool _isMovingShape;
    private ShapeModel? _movingShape;
    private Point _moveStartPoint;
    private Point _shapeOriginalPosition;

    public DrawPage()
    {
        InitializeComponent();
        ViewModel = App.ServiceProvider.GetRequiredService<DrawPageViewModel>();
        DataContext = ViewModel;
        
        Loaded += DrawPage_Loaded;
        Unloaded += DrawPage_Unloaded;
        ViewModel.CanvasLoaded += ViewModel_CanvasLoaded;
        ViewModel.ShapeCreated += ViewModel_ShapeCreated;
        ViewModel.ShapeUpdated += ViewModel_ShapeUpdated;
        ViewModel.ShapeDeleted += ViewModel_ShapeDeleted;
    }

    private void DrawPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetXamlRoot(this.XamlRoot);
        
        // Load templates when page loads
        _ = ViewModel.LoadTemplatesAsync();
    }

    private void DrawPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // Stop auto-save timer when page is unloaded
        ViewModel.StopAutoSave();
    }

    private void ViewModel_CanvasLoaded(object? sender, CanvasModel canvas)
    {
        DrawingCanvas.Width = canvas.Width;
        DrawingCanvas.Height = canvas.Height;
        DrawingCanvas.Background = new SolidColorBrush(ParseColor(canvas.BackgroundColor));
        DrawingCanvas.Children.Clear();
        
        // Clear shape mapping
        _shapeMap.Clear();
        ClearSelectionBorder();
        ClearResizeHandles();
        
        // Render existing shapes
        RenderShapes();
    }

    private void ViewModel_ShapeCreated(object? sender, ShapeModel shape)
    {
        // Clear any temporary drawing artifacts
        ClearTemporaryPolygonDrawing();
        
        // Shape already added to collection, just render it
        RenderShape(shape);
    }

    private void ViewModel_ShapeUpdated(object? sender, ShapeModel shape)
    {
        // Remove old visual and re-render
        if (_shapeMap.TryGetValue(shape, out var oldXamlShape))
        {
            DrawingCanvas.Children.Remove(oldXamlShape);
            _shapeMap.Remove(shape);
        }
        
        // Re-render with updated properties
        RenderShape(shape);
        
        // Recreate resize handles if this is the selected shape
        if (ViewModel.SelectedShape?.Id == shape.Id)
        {
            ClearResizeHandles();
            CreateResizeHandles(shape);
        }
    }

    private void ViewModel_ShapeDeleted(object? sender, ShapeModel shape)
    {
        // Remove visual from canvas
        if (_shapeMap.TryGetValue(shape, out var xamlShape))
        {
            DrawingCanvas.Children.Remove(xamlShape);
            _shapeMap.Remove(shape);
        }
        
        // Clear selection UI
        ClearSelectionBorder();
        ClearResizeHandles();
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
            // Track shape mapping for selection
            _shapeMap[shape] = xamlShape;
            
            // Add tap handler for selection
            xamlShape.Tapped += (s, e) => Shape_Tapped(shape, e);
            
            // Add pointer events for moving
            xamlShape.PointerPressed += (s, e) => Shape_PointerPressed(shape, e);
            xamlShape.PointerMoved += (s, e) => Shape_PointerMoved(shape, e);
            xamlShape.PointerReleased += (s, e) => Shape_PointerReleased(shape, e);
            
            DrawingCanvas.Children.Add(xamlShape);
        }
    }

    private void Shape_Tapped(ShapeModel shape, TappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedTool == "Select")
        {
            e.Handled = true;
            SelectShape(shape);
        }
    }

    private void SelectShape(ShapeModel shape)
    {
        ViewModel.SelectShapeCommand(shape);
        ClearSelectionBorder();
        ClearResizeHandles();
        CreateResizeHandles(shape);
    }

    private void CreateSelectionBorder(ShapeModel shape)
    {
        if (!_shapeMap.TryGetValue(shape, out var xamlShape))
            return;

        var bounds = CalculateShapeBounds(xamlShape);
        
        _selectionBorder = new Border
        {
            BorderBrush = new SolidColorBrush(Colors.Blue),
            BorderThickness = new Thickness(2),
            Width = bounds.Width + 10,
            Height = bounds.Height + 10,
            IsHitTestVisible = false
        };
        
        XamlCanvas.SetLeft(_selectionBorder, bounds.X - 5);
        XamlCanvas.SetTop(_selectionBorder, bounds.Y - 5);
        
        DrawingCanvas.Children.Add(_selectionBorder);
    }

    private void ClearSelectionBorder()
    {
        if (_selectionBorder != null)
        {
            DrawingCanvas.Children.Remove(_selectionBorder);
            _selectionBorder = null;
        }
    }

    private void ClearResizeHandles()
    {
        foreach (var handle in _resizeHandles)
        {
            DrawingCanvas.Children.Remove(handle.HandleEllipse);
        }
        _resizeHandles.Clear();
    }

    private void CreateResizeHandles(ShapeModel shape)
    {
        if (!_shapeMap.TryGetValue(shape, out var xamlShape))
            return;

        switch (shape.Type)
        {
            case "Line":
                CreateLineHandles(shape);
                break;
            case "Rectangle":
                CreateRectangleHandles(shape);
                break;
            case "Circle":
            case "Oval":
                CreateEllipseHandles(shape);
                break;
            case "Triangle":
            case "Polygon":
                CreatePolygonHandles(shape);
                break;
        }
    }

    private void CreateLineHandles(ShapeModel shape)
    {
        var points = DrawingHelper.JsonToPoints(shape.GeometryData);
        if (points.Count < 2) return;

        for (int i = 0; i < 2; i++)
        {
            var handle = new ResizeHandle(points[i], i, ResizeHandleType.Point);
            handle.HandleEllipse.PointerPressed += Handle_PointerPressed;
            handle.HandleEllipse.PointerMoved += Handle_PointerMoved;
            handle.HandleEllipse.PointerReleased += Handle_PointerReleased;
            _resizeHandles.Add(handle);
            DrawingCanvas.Children.Add(handle.HandleEllipse);
        }
    }

    private void CreateRectangleHandles(ShapeModel shape)
    {
        var rect = DrawingHelper.JsonToRect(shape.GeometryData);
        var corners = new[]
        {
            new Point(rect.X, rect.Y),
            new Point(rect.X + rect.Width, rect.Y),
            new Point(rect.X + rect.Width, rect.Y + rect.Height),
            new Point(rect.X, rect.Y + rect.Height)
        };

        for (int i = 0; i < 4; i++)
        {
            var handle = new ResizeHandle(corners[i], i, ResizeHandleType.Corner);
            handle.HandleEllipse.PointerPressed += Handle_PointerPressed;
            handle.HandleEllipse.PointerMoved += Handle_PointerMoved;
            handle.HandleEllipse.PointerReleased += Handle_PointerReleased;
            _resizeHandles.Add(handle);
            DrawingCanvas.Children.Add(handle.HandleEllipse);
        }
    }

    private void CreateEllipseHandles(ShapeModel shape)
    {
        try
        {
            var doc = JsonDocument.Parse(shape.GeometryData);
            var root = doc.RootElement;
            
            double centerX, centerY, radiusX, radiusY;
            if (shape.Type == "Circle")
            {
                centerX = root.GetProperty("centerX").GetDouble();
                centerY = root.GetProperty("centerY").GetDouble();
                var radius = root.GetProperty("radius").GetDouble();
                radiusX = radius;
                radiusY = radius;
            }
            else
            {
                centerX = root.GetProperty("centerX").GetDouble();
                centerY = root.GetProperty("centerY").GetDouble();
                radiusX = root.GetProperty("radiusX").GetDouble();
                radiusY = root.GetProperty("radiusY").GetDouble();
            }

            var corners = new[]
            {
                new Point(centerX - radiusX, centerY - radiusY),
                new Point(centerX + radiusX, centerY - radiusY),
                new Point(centerX + radiusX, centerY + radiusY),
                new Point(centerX - radiusX, centerY + radiusY)
            };

            for (int i = 0; i < 4; i++)
            {
                var handle = new ResizeHandle(corners[i], i, ResizeHandleType.BoundingBox);
                handle.HandleEllipse.PointerPressed += Handle_PointerPressed;
                handle.HandleEllipse.PointerMoved += Handle_PointerMoved;
                handle.HandleEllipse.PointerReleased += Handle_PointerReleased;
                _resizeHandles.Add(handle);
                DrawingCanvas.Children.Add(handle.HandleEllipse);
            }
        }
        catch { }
    }

    private void CreatePolygonHandles(ShapeModel shape)
    {
        try
        {
            var doc = JsonDocument.Parse(shape.GeometryData);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("points", out var pointsArray))
                return;

            int index = 0;
            foreach (var pointElement in pointsArray.EnumerateArray())
            {
                var x = pointElement.GetProperty("x").GetDouble();
                var y = pointElement.GetProperty("y").GetDouble();
                var point = new Point(x, y);

                var handle = new ResizeHandle(point, index, ResizeHandleType.Point);
                handle.HandleEllipse.PointerPressed += Handle_PointerPressed;
                handle.HandleEllipse.PointerMoved += Handle_PointerMoved;
                handle.HandleEllipse.PointerReleased += Handle_PointerReleased;
                _resizeHandles.Add(handle);
                DrawingCanvas.Children.Add(handle.HandleEllipse);
                index++;
            }
        }
        catch { }
    }

    private Windows.Foundation.Rect CalculateShapeBounds(XamlShape shape)
    {
        return shape switch
        {
            Line line => new Windows.Foundation.Rect(
                Math.Min(line.X1, line.X2),
                Math.Min(line.Y1, line.Y2),
                Math.Abs(line.X2 - line.X1),
                Math.Abs(line.Y2 - line.Y1)
            ),
            Rectangle rect => new Windows.Foundation.Rect(
                XamlCanvas.GetLeft(rect),
                XamlCanvas.GetTop(rect),
                rect.Width,
                rect.Height
            ),
            Ellipse ellipse => new Windows.Foundation.Rect(
                XamlCanvas.GetLeft(ellipse),
                XamlCanvas.GetTop(ellipse),
                ellipse.Width,
                ellipse.Height
            ),
            Polygon polygon => DrawingHelper.CalculateBoundingRect(
                new Point(polygon.Points.Min(p => p.X), polygon.Points.Min(p => p.Y)),
                new Point(polygon.Points.Max(p => p.X), polygon.Points.Max(p => p.Y))
            ),
            _ => new Windows.Foundation.Rect(0, 0, 0, 0)
        };
    }

    private void Handle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Ellipse ellipse) return;
        _activeHandle = _resizeHandles.FirstOrDefault(h => h.HandleEllipse == ellipse);
        if (_activeHandle == null) return;
        _isResizing = true;
        _resizingShape = ViewModel.SelectedShape;
        ellipse.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void Handle_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isResizing || _activeHandle == null || _resizingShape == null) return;
        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;
        _activeHandle.SetPosition(currentPoint);
        UpdateShapeGeometry(_resizingShape, _activeHandle, currentPoint);
        e.Handled = true;
    }

    private async void Handle_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isResizing) return;
        _isResizing = false;
        if (sender is Ellipse ellipse)
        {
            ellipse.ReleasePointerCapture(e.Pointer);
        }
        
        // Simply save the updated geometry - shape is already updated visually
        if (_resizingShape != null)
        {
            await SaveShapeGeometry(_resizingShape);
        }
        
        _activeHandle = null;
        _resizingShape = null;
        e.Handled = true;
    }

    // Shape Move Functionality
    private void Shape_PointerPressed(ShapeModel shape, PointerRoutedEventArgs e)
    {
        // Only allow moving if Select tool is active and shape is selected
        if (ViewModel.SelectedTool != "Select" || ViewModel.SelectedShape != shape)
            return;

        // Check if clicking on a handle (don't move if clicking handle)
        if (e.OriginalSource is Ellipse ellipse && _resizeHandles.Any(h => h.HandleEllipse == ellipse))
            return;

        if (!_shapeMap.TryGetValue(shape, out var xamlShape))
            return;

        _isMovingShape = true;
        _movingShape = shape;
        _moveStartPoint = e.GetCurrentPoint(DrawingCanvas).Position;
        _shapeOriginalPosition = GetShapePosition(xamlShape);

        xamlShape.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void Shape_PointerMoved(ShapeModel shape, PointerRoutedEventArgs e)
    {
        if (!_isMovingShape || _movingShape != shape)
            return;

        if (!_shapeMap.TryGetValue(shape, out var xamlShape))
            return;

        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;
        var deltaX = currentPoint.X - _moveStartPoint.X;
        var deltaY = currentPoint.Y - _moveStartPoint.Y;

        // Move shape
        MoveShape(xamlShape, deltaX, deltaY);

        // Move handles
        foreach (var handle in _resizeHandles)
        {
            var handlePos = handle.GetPosition();
            handle.SetPosition(new Point(handlePos.X + deltaX, handlePos.Y + deltaY));
        }

        _moveStartPoint = currentPoint;
        e.Handled = true;
    }

    private async void Shape_PointerReleased(ShapeModel shape, PointerRoutedEventArgs e)
    {
        if (!_isMovingShape || _movingShape != shape)
            return;

        _isMovingShape = false;

        if (_shapeMap.TryGetValue(shape, out var xamlShape))
        {
            xamlShape.ReleasePointerCapture(e.Pointer);
            
            // Simply save the updated position - shape is already moved visually
            await SaveShapeGeometry(shape);
        }

        _movingShape = null;
        e.Handled = true;
    }

    private Point GetShapePosition(XamlShape shape)
    {
        return shape switch
        {
            Line line => new Point(line.X1, line.Y1),
            Rectangle rect => new Point(XamlCanvas.GetLeft(rect), XamlCanvas.GetTop(rect)),
            Ellipse ellipse => new Point(XamlCanvas.GetLeft(ellipse), XamlCanvas.GetTop(ellipse)),
            Polygon polygon => new Point(polygon.Points[0].X, polygon.Points[0].Y),
            _ => new Point(0, 0)
        };
    }

    private void MoveShape(XamlShape shape, double deltaX, double deltaY)
    {
        switch (shape)
        {
            case Line line:
                line.X1 += deltaX;
                line.Y1 += deltaY;
                line.X2 += deltaX;
                line.Y2 += deltaY;
                break;

            case Rectangle rect:
                var rectLeft = XamlCanvas.GetLeft(rect);
                var rectTop = XamlCanvas.GetTop(rect);
                XamlCanvas.SetLeft(rect, rectLeft + deltaX);
                XamlCanvas.SetTop(rect, rectTop + deltaY);
                break;

            case Ellipse ellipse:
                var ellipseLeft = XamlCanvas.GetLeft(ellipse);
                var ellipseTop = XamlCanvas.GetTop(ellipse);
                XamlCanvas.SetLeft(ellipse, ellipseLeft + deltaX);
                XamlCanvas.SetTop(ellipse, ellipseTop + deltaY);
                break;

            case Polygon polygon:
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    var point = polygon.Points[i];
                    polygon.Points[i] = new Point(point.X + deltaX, point.Y + deltaY);
                }
                break;
        }
    }

    private void UpdateShapeGeometry(ShapeModel shape, ResizeHandle handle, Point newPosition)
    {
        if (!_shapeMap.TryGetValue(shape, out var xamlShape))
            return;

        switch (shape.Type)
        {
            case "Line":
                UpdateLineGeometry(xamlShape as Line, handle, newPosition);
                break;
            case "Rectangle":
                UpdateRectangleGeometry(xamlShape as Rectangle, handle, newPosition);
                break;
            case "Circle":
            case "Oval":
                UpdateEllipseGeometry(shape, xamlShape as Ellipse, handle, newPosition);
                break;
            case "Triangle":
            case "Polygon":
                UpdatePolygonGeometry(xamlShape as Polygon, handle, newPosition);
                break;
        }
    }

    private void UpdateLineGeometry(Line? line, ResizeHandle handle, Point newPosition)
    {
        if (line == null) return;
        if (handle.PointIndex == 0)
        {
            line.X1 = newPosition.X;
            line.Y1 = newPosition.Y;
        }
        else
        {
            line.X2 = newPosition.X;
            line.Y2 = newPosition.Y;
        }
    }

    private void UpdateRectangleGeometry(Rectangle? rectangle, ResizeHandle handle, Point newPosition)
    {
        if (rectangle == null) return;
        var oppositeIndex = (handle.PointIndex + 2) % 4;
        var oppositeHandle = _resizeHandles.FirstOrDefault(h => h.PointIndex == oppositeIndex);
        if (oppositeHandle == null) return;
        var anchorPoint = oppositeHandle.GetPosition();
        var newBounds = DrawingHelper.CalculateBoundingRect(anchorPoint, newPosition);
        rectangle.Width = newBounds.Width;
        rectangle.Height = newBounds.Height;
        XamlCanvas.SetLeft(rectangle, newBounds.X);
        XamlCanvas.SetTop(rectangle, newBounds.Y);
        _resizeHandles[0].SetPosition(new Point(newBounds.X, newBounds.Y));
        _resizeHandles[1].SetPosition(new Point(newBounds.X + newBounds.Width, newBounds.Y));
        _resizeHandles[2].SetPosition(new Point(newBounds.X + newBounds.Width, newBounds.Y + newBounds.Height));
        _resizeHandles[3].SetPosition(new Point(newBounds.X, newBounds.Y + newBounds.Height));
    }

    private void UpdateEllipseGeometry(ShapeModel shape, Ellipse? ellipse, ResizeHandle handle, Point newPosition)
    {
        if (ellipse == null) return;
        var oppositeIndex = (handle.PointIndex + 2) % 4;
        var oppositeHandle = _resizeHandles.FirstOrDefault(h => h.PointIndex == oppositeIndex);
        if (oppositeHandle == null) return;
        var anchorPoint = oppositeHandle.GetPosition();
        
        if (shape.Type == "Circle")
        {
            // For Circle: keep anchor point FIXED and expand from anchor
            // Calculate distance from anchor to new position
            var distanceX = Math.Abs(newPosition.X - anchorPoint.X);
            var distanceY = Math.Abs(newPosition.Y - anchorPoint.Y);
            var radius = Math.Max(distanceX, distanceY) / 2;
            
            // Calculate center position that keeps anchor fixed
            // The center is radius away from anchor point
            double centerX, centerY;
            
            // Determine direction based on which handle is being dragged
            switch (handle.PointIndex)
            {
                case 0: // Top-left, anchor is bottom-right
                    centerX = anchorPoint.X - radius;
                    centerY = anchorPoint.Y - radius;
                    break;
                case 1: // Top-right, anchor is bottom-left
                    centerX = anchorPoint.X + radius;
                    centerY = anchorPoint.Y - radius;
                    break;
                case 2: // Bottom-right, anchor is top-left
                    centerX = anchorPoint.X + radius;
                    centerY = anchorPoint.Y + radius;
                    break;
                case 3: // Bottom-left, anchor is top-right
                    centerX = anchorPoint.X - radius;
                    centerY = anchorPoint.Y + radius;
                    break;
                default:
                    centerX = anchorPoint.X;
                    centerY = anchorPoint.Y;
                    break;
            }
            
            ellipse.Width = radius * 2;
            ellipse.Height = radius * 2;
            XamlCanvas.SetLeft(ellipse, centerX - radius);
            XamlCanvas.SetTop(ellipse, centerY - radius);

            // Update all handles
            _resizeHandles[0].SetPosition(new Point(centerX - radius, centerY - radius));
            _resizeHandles[1].SetPosition(new Point(centerX + radius, centerY - radius));
            _resizeHandles[2].SetPosition(new Point(centerX + radius, centerY + radius));
            _resizeHandles[3].SetPosition(new Point(centerX - radius, centerY + radius));
        }
        else // Oval
        {
            // For Oval: standard anchor-based resize
            var centerX = (anchorPoint.X + newPosition.X) / 2;
            var centerY = (anchorPoint.Y + newPosition.Y) / 2;
            var radiusX = Math.Abs(newPosition.X - anchorPoint.X) / 2;
            var radiusY = Math.Abs(newPosition.Y - anchorPoint.Y) / 2;

            ellipse.Width = radiusX * 2;
            ellipse.Height = radiusY * 2;
            XamlCanvas.SetLeft(ellipse, centerX - radiusX);
            XamlCanvas.SetTop(ellipse, centerY - radiusY);

            // Update all handles
            _resizeHandles[0].SetPosition(new Point(centerX - radiusX, centerY - radiusY));
            _resizeHandles[1].SetPosition(new Point(centerX + radiusX, centerY - radiusY));
            _resizeHandles[2].SetPosition(new Point(centerX + radiusX, centerY + radiusY));
            _resizeHandles[3].SetPosition(new Point(centerX - radiusX, centerY + radiusY));
        }
    }

    private void UpdatePolygonGeometry(Polygon? polygon, ResizeHandle handle, Point newPosition)
    {
        if (polygon == null || handle.PointIndex >= polygon.Points.Count) return;
        polygon.Points[handle.PointIndex] = newPosition;
    }

    private async Task SaveShapeGeometry(ShapeModel shape)
    {
        if (!_shapeMap.TryGetValue(shape, out var xamlShape))
            return;

        try
        {
            string geometryData = string.Empty;
            Windows.Foundation.Rect bounds;

            switch (shape.Type)
            {
                case "Line":
                    if (xamlShape is Line line)
                    {
                        var points = new List<Point>
                        {
                            new Point(line.X1, line.Y1),
                            new Point(line.X2, line.Y2)
                        };
                        geometryData = DrawingHelper.PointsToJson(points);
                        bounds = new Windows.Foundation.Rect(
                            Math.Min(line.X1, line.X2),
                            Math.Min(line.Y1, line.Y2),
                            Math.Abs(line.X2 - line.X1),
                            Math.Abs(line.Y2 - line.Y1)
                        );
                    }
                    else return;
                    break;

                case "Rectangle":
                    if (xamlShape is Rectangle rect)
                    {
                        var left = XamlCanvas.GetLeft(rect);
                        var top = XamlCanvas.GetTop(rect);
                        bounds = new Windows.Foundation.Rect(left, top, rect.Width, rect.Height);
                        geometryData = DrawingHelper.RectToJson(bounds);
                    }
                    else return;
                    break;

                case "Circle":
                    if (xamlShape is Ellipse circleEllipse)
                    {
                        var left = XamlCanvas.GetLeft(circleEllipse);
                        var top = XamlCanvas.GetTop(circleEllipse);
                        var centerX = left + circleEllipse.Width / 2;
                        var centerY = top + circleEllipse.Height / 2;
                        var radius = circleEllipse.Width / 2;

                        geometryData = JsonSerializer.Serialize(new
                        {
                            centerX = centerX,
                            centerY = centerY,
                            radius = radius
                        });
                        
                        bounds = new Windows.Foundation.Rect(left, top, circleEllipse.Width, circleEllipse.Height);
                    }
                    else return;
                    break;

                case "Oval":
                    if (xamlShape is Ellipse ovalEllipse)
                    {
                        var left = XamlCanvas.GetLeft(ovalEllipse);
                        var top = XamlCanvas.GetTop(ovalEllipse);
                        var centerX = left + ovalEllipse.Width / 2;
                        var centerY = top + ovalEllipse.Height / 2;
                        var radiusX = ovalEllipse.Width / 2;
                        var radiusY = ovalEllipse.Height / 2;

                        geometryData = JsonSerializer.Serialize(new
                        {
                            centerX = centerX,
                            centerY = centerY,
                            radiusX = radiusX,
                            radiusY = radiusY
                        });
                        
                        bounds = new Windows.Foundation.Rect(left, top, ovalEllipse.Width, ovalEllipse.Height);
                    }
                    else return;
                    break;

                case "Triangle":
                case "Polygon":
                    if (xamlShape is Polygon polygon)
                    {
                        var pointsData = polygon.Points.Select(p => new { x = p.X, y = p.Y });
                        geometryData = JsonSerializer.Serialize(new { points = pointsData });
                        
                        bounds = new Windows.Foundation.Rect(
                            polygon.Points.Min(p => p.X),
                            polygon.Points.Min(p => p.Y),
                            polygon.Points.Max(p => p.X) - polygon.Points.Min(p => p.X),
                            polygon.Points.Max(p => p.Y) - polygon.Points.Min(p => p.Y)
                        );
                    }
                    else return;
                    break;

                default:
                    return;
            }

            if (!string.IsNullOrEmpty(geometryData))
            {
                // Update shape model
                shape.GeometryData = geometryData;
                shape.X = bounds.X;
                shape.Y = bounds.Y;
                shape.Width = bounds.Width;
                shape.Height = bounds.Height;

                // Save to database
                await ViewModel.UpdateShapeAsync(shape);
            }
        }
        catch (Exception ex)
        {
            // Handle error silently or show notification
            System.Diagnostics.Debug.WriteLine($"Error saving shape geometry: {ex.Message}");
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
        else if (e.Parameter is DrawPageNavigationParameter navParam)
        {
            // Load canvas from ManagePage
            ViewModel.SetProfile(navParam.Profile);
            ViewModel.LoadCanvas(navParam.Canvas);
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

    private void SelectedShapeStrokeColorPicker_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Color color)
        {
            ViewModel.SelectedShapeStrokeColor = color;
        }
    }

    private void SelectedShapeFillColorPicker_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Color color)
        {
            ViewModel.SelectedShapeFillColor = color;
        }
    }

    private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;

        // Check if point is within canvas bounds
        if (!IsPointInCanvasBounds(currentPoint))
            return;

        // Handle Select tool
        if (ViewModel.SelectedTool == "Select")
        {
            ViewModel.ClearSelection();
            ClearSelectionBorder();
            ClearResizeHandles();
            return;
        }

        // Handle Triangle with multi-click (3 points)
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

    private bool IsPointInCanvasBounds(Point point)
    {
        return point.X >= 0 && point.X <= DrawingCanvas.ActualWidth &&
               point.Y >= 0 && point.Y <= DrawingCanvas.ActualHeight;
    }

    private Point ClampPointToCanvas(Point point)
    {
        return new Point(
            Math.Max(0, Math.Min(point.X, DrawingCanvas.ActualWidth)),
            Math.Max(0, Math.Min(point.Y, DrawingCanvas.ActualHeight))
        );
    }

    private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || _currentShape == null) return;

        var currentPoint = e.GetCurrentPoint(DrawingCanvas).Position;
        
        // Clamp point to canvas bounds
        currentPoint = ClampPointToCanvas(currentPoint);
        
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
                // Calculate circle with same logic as save
                var circleCenterX = (_startPoint.X + currentPoint.X) / 2;
                var circleCenterY = (_startPoint.Y + currentPoint.Y) / 2;
                var radius = Math.Max(
                    Math.Abs(currentPoint.X - _startPoint.X) / 2,
                    Math.Abs(currentPoint.Y - _startPoint.Y) / 2);
                
                ellipse.Width = radius * 2;
                ellipse.Height = radius * 2;
                XamlCanvas.SetLeft(ellipse, circleCenterX - radius);
                XamlCanvas.SetTop(ellipse, circleCenterY - radius);
                break;

            case Ellipse ellipse:
                // Calculate oval with same logic as save
                var ovalCenterX = (_startPoint.X + currentPoint.X) / 2;
                var ovalCenterY = (_startPoint.Y + currentPoint.Y) / 2;
                var radiusX = Math.Abs(currentPoint.X - _startPoint.X) / 2;
                var radiusY = Math.Abs(currentPoint.Y - _startPoint.Y) / 2;
                
                ellipse.Width = radiusX * 2;
                ellipse.Height = radiusY * 2;
                XamlCanvas.SetLeft(ellipse, ovalCenterX - radiusX);
                XamlCanvas.SetTop(ellipse, ovalCenterY - radiusY);
                break;
        }
    }

    private async void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing) return;

        _isDrawing = false;
        DrawingCanvas.ReleasePointerCapture(e.Pointer);

        // Save shape to database (but remove preview first)
        if (_endPoint.HasValue && _currentShape != null)
        {
            // Remove the preview shape before saving
            DrawingCanvas.Children.Remove(_currentShape);
            
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

    private void HandlePolygonClick(Point point)
    {
        // Clamp point to canvas bounds
        point = ClampPointToCanvas(point);
        
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
    }

    private void HandleTriangleClick(Point point)
    {
        // Clamp point to canvas bounds
        point = ClampPointToCanvas(point);
        
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

        // Auto-complete triangle when 3 points are added
        if (ViewModel.PolygonPoints.Count == 3)
        {
            CompleteTriangle();
        }
    }

    private async void CompletePolygon()
    {
        if (ViewModel.PolygonPoints.Count < 3)
            return;

        // Clear temporary drawing artifacts FIRST
        ClearTemporaryPolygonDrawing();

        // Don't add to canvas here - let ViewModel_ShapeCreated handle rendering
        // Just save to database
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

        // SaveShapeAsync will trigger ShapeCreated event, which will call RenderShape
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

    private async void CompleteTriangle()
    {
        if (ViewModel.PolygonPoints.Count != 3)
            return;

        // Clear temporary drawing artifacts FIRST
        ClearTemporaryPolygonDrawing();

        // Don't add to canvas here - let ViewModel_ShapeCreated handle rendering
        // Just save to database
        var trianglePointData = ViewModel.PolygonPoints.Select(p => new { x = p.X, y = p.Y });
        var triangleData = new { points = trianglePointData };
        var geometryData = JsonSerializer.Serialize(triangleData);

        var bounds = DrawingHelper.CalculateBoundingRect(
            new Point(ViewModel.PolygonPoints.Min(p => p.X), ViewModel.PolygonPoints.Min(p => p.Y)),
            new Point(ViewModel.PolygonPoints.Max(p => p.X), ViewModel.PolygonPoints.Max(p => p.Y))
        );

        var shape = new ShapeModel
        {
            Type = "Triangle",
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

        // SaveShapeAsync will trigger ShapeCreated event, which will call RenderShape
        await ViewModel.SaveShapeAsync(shape);
        ViewModel.ClearPolygonDrawing();
    }
}
