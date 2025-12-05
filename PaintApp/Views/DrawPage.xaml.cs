using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using PaintApp.ViewModels;
using Windows.Foundation;
using Windows.UI;

namespace PaintApp.Views;

public sealed partial class DrawPage : Page
{
    public DrawPageViewModel ViewModel { get; }
    private Point _startPoint;
    private Shape? _currentShape;

    public DrawPage()
    {
        InitializeComponent();
        ViewModel = App.ServiceProvider.GetRequiredService<DrawPageViewModel>();
        DataContext = ViewModel;
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
        _startPoint = e.GetCurrentPoint(DrawingCanvas).Position;
        
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
            Canvas.SetLeft(_currentShape, _startPoint.X);
            Canvas.SetTop(_currentShape, _startPoint.Y);
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
                Canvas.SetLeft(rect, Math.Min(_startPoint.X, currentPoint.X));
                Canvas.SetTop(rect, Math.Min(_startPoint.Y, currentPoint.Y));
                break;

            case Ellipse ellipse when ViewModel.SelectedTool == "Circle":
                var radius = Math.Max(
                    Math.Abs(currentPoint.X - _startPoint.X),
                    Math.Abs(currentPoint.Y - _startPoint.Y));
                ellipse.Width = radius;
                ellipse.Height = radius;
                Canvas.SetLeft(ellipse, Math.Min(_startPoint.X, currentPoint.X));
                Canvas.SetTop(ellipse, Math.Min(_startPoint.Y, currentPoint.Y));
                break;

            case Ellipse ellipse:
                var w = Math.Abs(currentPoint.X - _startPoint.X);
                var h = Math.Abs(currentPoint.Y - _startPoint.Y);
                ellipse.Width = w;
                ellipse.Height = h;
                Canvas.SetLeft(ellipse, Math.Min(_startPoint.X, currentPoint.X));
                Canvas.SetTop(ellipse, Math.Min(_startPoint.Y, currentPoint.Y));
                break;
        }
    }

    private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _currentShape = null;
        DrawingCanvas.ReleasePointerCapture(e.Pointer);
    }
}
