using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintApp.Data;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Implementation of statistics service for analytics and reporting
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, int>> GetShapeTypeDistributionAsync(int profileId)
    {
        try
        {
            // Get all shapes from canvases belonging to this profile
            var shapeTypes = await _context.Shapes
                .Include(s => s.Canvas)
                .Where(s => s.Canvas != null && s.Canvas.ProfileId == profileId && !s.IsTemplate)
                .GroupBy(s => s.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return shapeTypes;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetShapeTypeDistributionAsync error: {ex.Message}");
            return new Dictionary<string, int>();
        }
    }

    /// <inheritdoc/>
    public async Task<List<Shape>> GetMostUsedTemplatesAsync(int profileId, int count = 10)
    {
        try
        {
            // Get templates ordered by usage count
            // Note: We'll need to track which profile created/owns templates
            // For now, get all templates and filter by usage
            var templates = await _context.Shapes
                .Where(s => s.IsTemplate)
                .OrderByDescending(s => s.UsageCount)
                .Take(count)
                .ToListAsync();

            return templates;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMostUsedTemplatesAsync error: {ex.Message}");
            return new List<Shape>();
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCanvasCountAsync(int profileId)
    {
        try
        {
            var count = await _context.Canvases
                .Where(c => c.ProfileId == profileId)
                .CountAsync();

            return count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetTotalCanvasCountAsync error: {ex.Message}");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalShapeCountAsync(int profileId)
    {
        try
        {
            var count = await _context.Shapes
                .Include(s => s.Canvas)
                .Where(s => s.Canvas != null && s.Canvas.ProfileId == profileId && !s.IsTemplate)
                .CountAsync();

            return count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetTotalShapeCountAsync error: {ex.Message}");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalTemplateCountAsync(int profileId)
    {
        try
        {
            // Get templates count
            // TODO: Add ProfileId to Shape model to track template ownership
            var count = await _context.Shapes
                .Where(s => s.IsTemplate)
                .CountAsync();

            return count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetTotalTemplateCountAsync error: {ex.Message}");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<Dictionary<DateTime, int>> GetShapeActivityOverTimeAsync(int profileId, int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days).Date;

            var activity = await _context.Shapes
                .Include(s => s.Canvas)
                .Where(s => s.Canvas != null && 
                           s.Canvas.ProfileId == profileId && 
                           !s.IsTemplate &&
                           s.CreatedAt >= startDate)
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            // Fill in missing days with 0 count
            var result = new Dictionary<DateTime, int>();
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Now.AddDays(-i).Date;
                result[date] = activity.ContainsKey(date) ? activity[date] : 0;
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetShapeActivityOverTimeAsync error: {ex.Message}");
            return new Dictionary<DateTime, int>();
        }
    }

    /// <inheritdoc/>
    public async Task<Dictionary<DateTime, int>> GetCanvasActivityOverTimeAsync(int profileId, int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days).Date;

            var activity = await _context.Canvases
                .Where(c => c.ProfileId == profileId && c.CreatedAt >= startDate)
                .GroupBy(c => c.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            // Fill in missing days with 0 count
            var result = new Dictionary<DateTime, int>();
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Now.AddDays(-i).Date;
                result[date] = activity.ContainsKey(date) ? activity[date] : 0;
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetCanvasActivityOverTimeAsync error: {ex.Message}");
            return new Dictionary<DateTime, int>();
        }
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, int>> GetMostUsedColorsAsync(int profileId, int count = 10)
    {
        try
        {
            // Get stroke colors
            var strokeColors = await _context.Shapes
                .Include(s => s.Canvas)
                .Where(s => s.Canvas != null && s.Canvas.ProfileId == profileId && !s.IsTemplate)
                .GroupBy(s => s.StrokeColor)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get fill colors (excluding null/empty)
            var fillColors = await _context.Shapes
                .Include(s => s.Canvas)
                .Where(s => s.Canvas != null && 
                           s.Canvas.ProfileId == profileId && 
                           !s.IsTemplate &&
                           !string.IsNullOrEmpty(s.FillColor))
                .GroupBy(s => s.FillColor)
                .Select(g => new { Color = g.Key, Count = g.Count() })
                .ToListAsync();

            // Combine and aggregate
            var allColors = strokeColors.Concat(fillColors)
                .GroupBy(x => x.Color)
                .Select(g => new { Color = g.Key, Count = g.Sum(x => x.Count) })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToDictionary(x => x.Color ?? "#000000", x => x.Count);

            return allColors;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMostUsedColorsAsync error: {ex.Message}");
            return new Dictionary<string, int>();
        }
    }

    /// <inheritdoc/>
    public async Task<double> GetAverageShapesPerCanvasAsync(int profileId)
    {
        try
        {
            var canvasCount = await GetTotalCanvasCountAsync(profileId);
            if (canvasCount == 0)
                return 0;

            var shapeCount = await GetTotalShapeCountAsync(profileId);
            
            return Math.Round((double)shapeCount / canvasCount, 2);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAverageShapesPerCanvasAsync error: {ex.Message}");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<ProfileStatisticsSummary> GetProfileStatisticsSummaryAsync(int profileId)
    {
        try
        {
            var summary = new ProfileStatisticsSummary();

            // Get basic counts
            summary.TotalCanvases = await GetTotalCanvasCountAsync(profileId);
            summary.TotalShapes = await GetTotalShapeCountAsync(profileId);
            summary.TotalTemplates = await GetTotalTemplateCountAsync(profileId);
            summary.AverageShapesPerCanvas = await GetAverageShapesPerCanvasAsync(profileId);

            // Get most used shape type
            var shapeDistribution = await GetShapeTypeDistributionAsync(profileId);
            if (shapeDistribution.Any())
            {
                var mostUsed = shapeDistribution.OrderByDescending(x => x.Value).First();
                summary.MostUsedShapeType = mostUsed.Key;
                summary.MostUsedShapeTypeCount = mostUsed.Value;
            }

            // Get most used color
            var colorUsage = await GetMostUsedColorsAsync(profileId, 1);
            if (colorUsage.Any())
            {
                var mostUsedColor = colorUsage.First();
                summary.MostUsedColor = mostUsedColor.Key;
                summary.MostUsedColorCount = mostUsedColor.Value;
            }

            // Get canvas creation dates
            var canvases = await _context.Canvases
                .Where(c => c.ProfileId == profileId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => c.CreatedAt)
                .ToListAsync();

            if (canvases.Any())
            {
                summary.FirstCanvasCreated = canvases.First();
                summary.LastCanvasCreated = canvases.Last();
            }

            // Get last activity (most recent shape or canvas)
            var lastShapeActivity = await _context.Shapes
                .Include(s => s.Canvas)
                .Where(s => s.Canvas != null && s.Canvas.ProfileId == profileId && !s.IsTemplate)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            var lastCanvasActivity = await _context.Canvases
                .Where(c => c.ProfileId == profileId)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => c.UpdatedAt)
                .FirstOrDefaultAsync();

            summary.LastActivity = new[] { lastShapeActivity, lastCanvasActivity }
                .Where(d => d != default)
                .DefaultIfEmpty()
                .Max();

            // Calculate active days (days with at least one shape or canvas created)
            if (summary.FirstCanvasCreated.HasValue)
            {
                var activityDates = await _context.Shapes
                    .Include(s => s.Canvas)
                    .Where(s => s.Canvas != null && s.Canvas.ProfileId == profileId && !s.IsTemplate)
                    .Select(s => s.CreatedAt.Date)
                    .Union(_context.Canvases
                        .Where(c => c.ProfileId == profileId)
                        .Select(c => c.CreatedAt.Date))
                    .Distinct()
                    .CountAsync();

                summary.ActiveDays = activityDates;
            }

            return summary;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetProfileStatisticsSummaryAsync error: {ex.Message}");
            return new ProfileStatisticsSummary();
        }
    }
}
