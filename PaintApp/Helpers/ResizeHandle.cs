using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.Foundation;
using Windows.UI;

namespace PaintApp.Helpers;

public class ResizeHandle
{
    public Ellipse HandleEllipse { get; }
    public int PointIndex { get; }
    public ResizeHandleType Type { get; }

    public ResizeHandle(Point position, int pointIndex, ResizeHandleType type)
    {
        PointIndex = pointIndex;
        Type = type;
        
        HandleEllipse = new Ellipse
        {
            Width = 12,
            Height = 12,
            Fill = new SolidColorBrush(Colors.White),
            Stroke = new SolidColorBrush(Colors.Blue),
            StrokeThickness = 2
        };

        SetPosition(position);
    }

    public void SetPosition(Point position)
    {
        Microsoft.UI.Xaml.Controls.Canvas.SetLeft(HandleEllipse, position.X - 6);
        Microsoft.UI.Xaml.Controls.Canvas.SetTop(HandleEllipse, position.Y - 6);
    }

    public Point GetPosition()
    {
        var left = Microsoft.UI.Xaml.Controls.Canvas.GetLeft(HandleEllipse);
        var top = Microsoft.UI.Xaml.Controls.Canvas.GetTop(HandleEllipse);
        return new Point(left + 6, top + 6);
    }
}

public enum ResizeHandleType
{
    Point,           // For Line, Triangle, Polygon individual points
    Corner,          // For Rectangle corners
    BoundingBox      // For Circle/Oval bounding box corners
}
