using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Service for generating statistics and analytics data
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Gets the distribution of shape types for a specific profile
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <returns>Dictionary with shape type as key and count as value</returns>
    Task<Dictionary<string, int>> GetShapeTypeDistributionAsync(int profileId);

    /// <summary>
    /// Gets the most frequently used templates for a profile
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <param name="count">Number of top templates to return</param>
    /// <returns>List of most used template shapes ordered by usage count</returns>
    Task<List<Shape>> GetMostUsedTemplatesAsync(int profileId, int count = 10);

    /// <summary>
    /// Gets the total number of canvases for a profile
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <returns>Total canvas count</returns>
    Task<int> GetTotalCanvasCountAsync(int profileId);

    /// <summary>
    /// Gets the total number of shapes created by a profile
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <returns>Total shape count across all canvases</returns>
    Task<int> GetTotalShapeCountAsync(int profileId);

    /// <summary>
    /// Gets the total number of templates saved by a profile
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <returns>Total template count</returns>
    Task<int> GetTotalTemplateCountAsync(int profileId);

    /// <summary>
    /// Gets shape creation activity over time
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <param name="days">Number of days to look back</param>
    /// <returns>Dictionary with date as key and shape count as value</returns>
    Task<Dictionary<DateTime, int>> GetShapeActivityOverTimeAsync(int profileId, int days = 30);

    /// <summary>
    /// Gets canvas creation activity over time
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <param name="days">Number of days to look back</param>
    /// <returns>Dictionary with date as key and canvas count as value</returns>
    Task<Dictionary<DateTime, int>> GetCanvasActivityOverTimeAsync(int profileId, int days = 30);

    /// <summary>
    /// Gets the most used colors for shapes
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <param name="count">Number of top colors to return</param>
    /// <returns>Dictionary with color (hex) as key and usage count as value</returns>
    Task<Dictionary<string, int>> GetMostUsedColorsAsync(int profileId, int count = 10);

    /// <summary>
    /// Gets average shapes per canvas
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <returns>Average number of shapes per canvas</returns>
    Task<double> GetAverageShapesPerCanvasAsync(int profileId);

    /// <summary>
    /// Gets statistics summary for a profile
    /// </summary>
    /// <param name="profileId">Profile ID to analyze</param>
    /// <returns>Summary object with key statistics</returns>
    Task<ProfileStatisticsSummary> GetProfileStatisticsSummaryAsync(int profileId);
}

/// <summary>
/// Summary of profile statistics
/// </summary>
public class ProfileStatisticsSummary
{
    public int TotalCanvases { get; set; }
    public int TotalShapes { get; set; }
    public int TotalTemplates { get; set; }
    public double AverageShapesPerCanvas { get; set; }
    public string? MostUsedShapeType { get; set; }
    public int MostUsedShapeTypeCount { get; set; }
    public string? MostUsedColor { get; set; }
    public int MostUsedColorCount { get; set; }
    public DateTime? FirstCanvasCreated { get; set; }
    public DateTime? LastCanvasCreated { get; set; }
    public DateTime? LastActivity { get; set; }
    public int ActiveDays { get; set; }
}
