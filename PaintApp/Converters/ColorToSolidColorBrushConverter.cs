using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;

namespace PaintApp.Converters;

public class ColorToSolidColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }
        
        // Handle nullable Color? - check for null first
        if (value != null && value.GetType() == typeof(Color?))
        {
            var nullableColor = (Color?)value;
            if (nullableColor.HasValue)
            {
                return new SolidColorBrush(nullableColor.Value);
            }
        }
        
        // Default to transparent
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color;
        }
        
        return Colors.Transparent;
    }
}
