using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PaintApp.Converters;

/// <summary>
/// Converter ?? so sánh string và tr? v? Visibility
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue && parameter is string parameterValue)
        {
            return string.Equals(stringValue, parameterValue, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
