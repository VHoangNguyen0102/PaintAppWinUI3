using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Windows.Foundation;

namespace PaintApp.Helpers;

/// <summary>
/// Helper class cho các phép tính geometry và serialization trong drawing
/// </summary>
public static class DrawingHelper
{
    #region Geometry Calculations

    /// <summary>
    /// Tính kho?ng cách gi?a hai ?i?m
    /// </summary>
    public static double CalculateDistance(Point point1, Point point2)
    {
        var dx = point2.X - point1.X;
        var dy = point2.Y - point1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Tính ?i?m gi?a c?a hai ?i?m
    /// </summary>
    public static Point CalculateMidpoint(Point point1, Point point2)
    {
        return new Point(
            (point1.X + point2.X) / 2,
            (point1.Y + point2.Y) / 2
        );
    }

    /// <summary>
    /// Tính góc (angle) gi?a hai ?i?m tính theo ??
    /// </summary>
    public static double CalculateAngle(Point point1, Point point2)
    {
        var dx = point2.X - point1.X;
        var dy = point2.Y - point1.Y;
        return Math.Atan2(dy, dx) * 180 / Math.PI;
    }

    /// <summary>
    /// Tính bounding rectangle t? hai ?i?m
    /// </summary>
    public static Rect CalculateBoundingRect(Point point1, Point point2)
    {
        var x = Math.Min(point1.X, point2.X);
        var y = Math.Min(point1.Y, point2.Y);
        var width = Math.Abs(point2.X - point1.X);
        var height = Math.Abs(point2.Y - point1.Y);
        
        return new Rect(x, y, width, height);
    }

    /// <summary>
    /// Tính các ?i?m c?a tam giác t? start point và end point
    /// </summary>
    public static Point[] CalculateTrianglePoints(Point startPoint, Point endPoint)
    {
        var topPoint = new Point((startPoint.X + endPoint.X) / 2, startPoint.Y);
        var bottomLeft = new Point(startPoint.X, endPoint.Y);
        var bottomRight = new Point(endPoint.X, endPoint.Y);
        
        return new[] { topPoint, bottomLeft, bottomRight };
    }

    /// <summary>
    /// Normalize rectangle ?? có width và height d??ng
    /// </summary>
    public static (Point topLeft, double width, double height) NormalizeRectangle(Point point1, Point point2)
    {
        var topLeft = new Point(
            Math.Min(point1.X, point2.X),
            Math.Min(point1.Y, point2.Y)
        );
        
        var width = Math.Abs(point2.X - point1.X);
        var height = Math.Abs(point2.Y - point1.Y);
        
        return (topLeft, width, height);
    }

    /// <summary>
    /// T?o hình vuông (square) t? start point và end point
    /// </summary>
    public static (Point topLeft, double size) CalculateSquare(Point startPoint, Point endPoint)
    {
        var size = Math.Max(
            Math.Abs(endPoint.X - startPoint.X),
            Math.Abs(endPoint.Y - startPoint.Y)
        );
        
        var topLeft = new Point(
            Math.Min(startPoint.X, endPoint.X),
            Math.Min(startPoint.Y, endPoint.Y)
        );
        
        return (topLeft, size);
    }

    /// <summary>
    /// Ki?m tra xem m?t ?i?m có n?m trong rectangle không
    /// </summary>
    public static bool IsPointInRectangle(Point point, Rect rectangle)
    {
        return point.X >= rectangle.X &&
               point.X <= rectangle.X + rectangle.Width &&
               point.Y >= rectangle.Y &&
               point.Y <= rectangle.Y + rectangle.Height;
    }

    /// <summary>
    /// Ki?m tra xem m?t ?i?m có g?n ???ng th?ng không (v?i tolerance)
    /// </summary>
    public static bool IsPointNearLine(Point point, Point lineStart, Point lineEnd, double tolerance = 5)
    {
        var lineLength = CalculateDistance(lineStart, lineEnd);
        if (lineLength == 0) return CalculateDistance(point, lineStart) <= tolerance;

        var distance = Math.Abs(
            (lineEnd.Y - lineStart.Y) * point.X -
            (lineEnd.X - lineStart.X) * point.Y +
            lineEnd.X * lineStart.Y -
            lineEnd.Y * lineStart.X
        ) / lineLength;

        return distance <= tolerance;
    }

    #endregion

    #region JSON Serialization

    /// <summary>
    /// Convert Point to JSON string
    /// </summary>
    public static string PointToJson(Point point)
    {
        var pointData = new { X = point.X, Y = point.Y };
        return JsonSerializer.Serialize(pointData);
    }

    /// <summary>
    /// Convert list of Points to JSON string
    /// </summary>
    public static string PointsToJson(IEnumerable<Point> points)
    {
        var pointsData = points.Select(p => new { X = p.X, Y = p.Y });
        return JsonSerializer.Serialize(pointsData);
    }

    /// <summary>
    /// Parse JSON string to Point
    /// </summary>
    public static Point JsonToPoint(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            var x = root.GetProperty("X").GetDouble();
            var y = root.GetProperty("Y").GetDouble();
            
            return new Point(x, y);
        }
        catch
        {
            return new Point(0, 0);
        }
    }

    /// <summary>
    /// Parse JSON string to list of Points
    /// </summary>
    public static List<Point> JsonToPoints(string json)
    {
        var points = new List<Point>();
        
        try
        {
            var doc = JsonDocument.Parse(json);
            var array = doc.RootElement;
            
            foreach (var element in array.EnumerateArray())
            {
                var x = element.GetProperty("X").GetDouble();
                var y = element.GetProperty("Y").GetDouble();
                points.Add(new Point(x, y));
            }
        }
        catch
        {
            // Return empty list on error
        }
        
        return points;
    }

    /// <summary>
    /// Convert Rect to JSON string
    /// </summary>
    public static string RectToJson(Rect rect)
    {
        var rectData = new
        {
            X = rect.X,
            Y = rect.Y,
            Width = rect.Width,
            Height = rect.Height
        };
        return JsonSerializer.Serialize(rectData);
    }

    /// <summary>
    /// Parse JSON string to Rect
    /// </summary>
    public static Rect JsonToRect(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            var x = root.GetProperty("X").GetDouble();
            var y = root.GetProperty("Y").GetDouble();
            var width = root.GetProperty("Width").GetDouble();
            var height = root.GetProperty("Height").GetDouble();
            
            return new Rect(x, y, width, height);
        }
        catch
        {
            return Rect.Empty;
        }
    }

    #endregion

    #region Color Helpers

    /// <summary>
    /// Convert hex color string to RGB values
    /// </summary>
    public static (byte R, byte G, byte B, byte A) HexToRgb(string hexColor)
    {
        try
        {
            hexColor = hexColor.TrimStart('#');
            
            byte a = 255;
            byte r, g, b;
            
            if (hexColor.Length == 8)
            {
                a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                b = Convert.ToByte(hexColor.Substring(6, 2), 16);
            }
            else if (hexColor.Length == 6)
            {
                r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                b = Convert.ToByte(hexColor.Substring(4, 2), 16);
            }
            else
            {
                return (255, 255, 255, 255);
            }
            
            return (r, g, b, a);
        }
        catch
        {
            return (255, 255, 255, 255);
        }
    }

    /// <summary>
    /// Convert RGB values to hex color string
    /// </summary>
    public static string RgbToHex(byte r, byte g, byte b, byte a = 255)
    {
        if (a == 255)
        {
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
    }

    #endregion

    #region Shape Validation

    /// <summary>
    /// Validate n?u shape có kích th??c h?p l? (minimum size)
    /// </summary>
    public static bool IsValidShapeSize(double width, double height, double minSize = 1)
    {
        return width >= minSize && height >= minSize;
    }

    /// <summary>
    /// Validate n?u line có ?? dài h?p l?
    /// </summary>
    public static bool IsValidLineLength(Point point1, Point point2, double minLength = 1)
    {
        return CalculateDistance(point1, point2) >= minLength;
    }

    /// <summary>
    /// Clamp point trong boundaries
    /// </summary>
    public static Point ClampPoint(Point point, double minX, double minY, double maxX, double maxY)
    {
        return new Point(
            Math.Max(minX, Math.Min(maxX, point.X)),
            Math.Max(minY, Math.Min(maxY, point.Y))
        );
    }

    #endregion
}
