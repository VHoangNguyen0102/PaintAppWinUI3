using System;

namespace PaintApp.ViewModels;

/// <summary>
/// Data item for chart visualization
/// </summary>
public class ChartDataItem
{
    /// <summary>
    /// Label for the data point (e.g., "Rectangle", "Red", etc.)
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Numeric value for the data point
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Optional color for visual representation (hex format)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Percentage of total (calculated dynamically)
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Data item for time-based activity charts
/// </summary>
public class ActivityDataItem
{
    /// <summary>
    /// Date of the activity
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Count of items on that date
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Formatted date string for display
    /// </summary>
    public string DateLabel => Date.ToString("MMM dd");
}
