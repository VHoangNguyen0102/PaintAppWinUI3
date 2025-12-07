using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;
using ShapeModel = PaintApp.Models.Shape;
using XamlCanvas = Microsoft.UI.Xaml.Controls.Canvas;
using XamlShape = Microsoft.UI.Xaml.Shapes.Shape;

namespace PaintApp.Controls;

public class ShapeThumbnailControl : XamlCanvas
{
    public static readonly DependencyProperty ShapeProperty =
        DependencyProperty.Register(
            nameof(Shape),
            typeof(ShapeModel),
            typeof(ShapeThumbnailControl),
            new PropertyMetadata(null, OnShapeChanged));

    public ShapeModel? Shape
    {
        get => (ShapeModel?)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShapeThumbnailControl control)
        {
            control.RenderShape();
        }
    }

    private void RenderShape()
    {
        Children.Clear();

        if (Shape == null) return;

        try
        {
            // Create background
            var background = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = ActualWidth > 0 ? ActualWidth : 100,
                Height = ActualHeight > 0 ? ActualHeight : 100,
                Fill = new SolidColorBrush(Colors.White)
            };
            Children.Add(background);

            // Calculate scale to fit shape in thumbnail
            var maxDimension = Math.Max(Shape.Width, Shape.Height);
            var targetSize = Math.Min(ActualWidth > 0 ? ActualWidth : 100, 
                                     ActualHeight > 0 ? ActualHeight : 100) * 0.8;
            var scale = maxDimension > 0 ? targetSize / maxDimension : 1;

            // Calculate offset to center shape
            var offsetX = (ActualWidth - Shape.Width * scale) / 2;
            var offsetY = (ActualHeight - Shape.Height * scale) / 2;

            UIElement? shapeElement = null;

            switch (Shape.Type)
            {
                case "Line":
                    shapeElement = RenderLine(scale, offsetX, offsetY);
                    break;
                case "Rectangle":
                    shapeElement = RenderRectangle(scale, offsetX, offsetY);
                    break;
                case "Circle":
                case "Oval":
                    shapeElement = RenderEllipse(scale, offsetX, offsetY);
                    break;
                case "Triangle":
                case "Polygon":
                    shapeElement = RenderPolygon(scale, offsetX, offsetY);
                    break;
            }

            if (shapeElement != null)
            {
                Children.Add(shapeElement);
            }
        }
        catch
        {
            // Fallback: show shape type as text
            var fallbackText = new TextBlock
            {
                Text = Shape.Type,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Children.Add(fallbackText);
        }
    }

    private Microsoft.UI.Xaml.Shapes.Line? RenderLine(double scale, double offsetX, double offsetY)
    {
        try
        {
            var doc = JsonDocument.Parse(Shape!.GeometryData);
            var points = new List<Point>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var point in doc.RootElement.EnumerateArray())
                {
                    var x = point.GetProperty("X").GetDouble();
                    var y = point.GetProperty("Y").GetDouble();
                    points.Add(new Point(x, y));
                }
            }

            if (points.Count < 2) return null;

            return new Microsoft.UI.Xaml.Shapes.Line
            {
                X1 = points[0].X * scale + offsetX,
                Y1 = points[0].Y * scale + offsetY,
                X2 = points[1].X * scale + offsetX,
                Y2 = points[1].Y * scale + offsetY,
                Stroke = new SolidColorBrush(ParseColor(Shape.StrokeColor)),
                StrokeThickness = Math.Max(1, Shape.StrokeThickness * scale)
            };
        }
        catch
        {
            return null;
        }
    }

    private Microsoft.UI.Xaml.Shapes.Rectangle? RenderRectangle(double scale, double offsetX, double offsetY)
    {
        try
        {
            var rect = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = Shape!.Width * scale,
                Height = Shape.Height * scale,
                Stroke = new SolidColorBrush(ParseColor(Shape.StrokeColor)),
                StrokeThickness = Math.Max(1, Shape.StrokeThickness * scale)
            };

            if (!string.IsNullOrEmpty(Shape.FillColor))
            {
                rect.Fill = new SolidColorBrush(ParseColor(Shape.FillColor));
            }

            XamlCanvas.SetLeft(rect, offsetX);
            XamlCanvas.SetTop(rect, offsetY);

            return rect;
        }
        catch
        {
            return null;
        }
    }

    private Microsoft.UI.Xaml.Shapes.Ellipse? RenderEllipse(double scale, double offsetX, double offsetY)
    {
        try
        {
            var doc = JsonDocument.Parse(Shape!.GeometryData);
            var root = doc.RootElement;

            double radiusX, radiusY;

            if (Shape.Type == "Circle")
            {
                var radius = root.GetProperty("radius").GetDouble();
                radiusX = radius;
                radiusY = radius;
            }
            else
            {
                radiusX = root.GetProperty("radiusX").GetDouble();
                radiusY = root.GetProperty("radiusY").GetDouble();
            }

            var ellipse = new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                Width = radiusX * 2 * scale,
                Height = radiusY * 2 * scale,
                Stroke = new SolidColorBrush(ParseColor(Shape.StrokeColor)),
                StrokeThickness = Math.Max(1, Shape.StrokeThickness * scale)
            };

            if (!string.IsNullOrEmpty(Shape.FillColor))
            {
                ellipse.Fill = new SolidColorBrush(ParseColor(Shape.FillColor));
            }

            XamlCanvas.SetLeft(ellipse, offsetX);
            XamlCanvas.SetTop(ellipse, offsetY);

            return ellipse;
        }
        catch
        {
            return null;
        }
    }

    private Microsoft.UI.Xaml.Shapes.Polygon? RenderPolygon(double scale, double offsetX, double offsetY)
    {
        try
        {
            var doc = JsonDocument.Parse(Shape!.GeometryData);
            var root = doc.RootElement;

            if (!root.TryGetProperty("points", out var pointsArray))
                return null;

            var polygon = new Microsoft.UI.Xaml.Shapes.Polygon
            {
                Stroke = new SolidColorBrush(ParseColor(Shape.StrokeColor)),
                StrokeThickness = Math.Max(1, Shape.StrokeThickness * scale)
            };

            if (!string.IsNullOrEmpty(Shape.FillColor))
            {
                polygon.Fill = new SolidColorBrush(ParseColor(Shape.FillColor));
            }

            // Find min X and Y for normalization
            var minX = Shape.X;
            var minY = Shape.Y;

            foreach (var pointElement in pointsArray.EnumerateArray())
            {
                var x = pointElement.GetProperty("x").GetDouble();
                var y = pointElement.GetProperty("y").GetDouble();
                
                var scaledX = (x - minX) * scale + offsetX;
                var scaledY = (y - minY) * scale + offsetY;
                
                polygon.Points.Add(new Point(scaledX, scaledY));
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
            return Colors.Black;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        RenderShape();
        return base.MeasureOverride(availableSize);
    }
}
