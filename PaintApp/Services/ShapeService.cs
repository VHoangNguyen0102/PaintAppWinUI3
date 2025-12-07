using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintApp.Data;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Service implementation cho qu?n lý shapes
/// </summary>
public class ShapeService : IShapeService
{
    private readonly AppDbContext _dbContext;

    public ShapeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Shape>> GetShapesByCanvasIdAsync(int canvasId)
    {
        try
        {
            if (canvasId <= 0)
            {
                throw new ArgumentException("Canvas ID must be greater than zero.", nameof(canvasId));
            }

            return await _dbContext.Shapes
                .Where(s => s.CanvasId == canvasId)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve shapes for canvas {canvasId}.", ex);
        }
    }

    public async Task<List<Shape>> GetTemplateShapesAsync()
    {
        try
        {
            // Only get shapes that are explicitly marked as templates
            return await _dbContext.Shapes
                .Where(s => s.IsTemplate == true)
                .OrderByDescending(s => s.UsageCount)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to retrieve template shapes.", ex);
        }
    }

    public async Task<Shape> CreateShapeAsync(Shape shape)
    {
        try
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape), "Shape cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(shape.Type))
            {
                throw new ArgumentException("Shape type is required.", nameof(shape));
            }

            // Only validate CanvasId if shape is not a template
            if (!shape.IsTemplate && shape.CanvasId <= 0)
            {
                throw new ArgumentException("Canvas ID must be greater than zero for non-template shapes.", nameof(shape));
            }

            if (string.IsNullOrWhiteSpace(shape.GeometryData))
            {
                throw new ArgumentException("Geometry data is required.", nameof(shape));
            }

            // Set timestamp
            shape.CreatedAt = DateTime.Now;

            _dbContext.Shapes.Add(shape);
            await _dbContext.SaveChangesAsync();

            return shape;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to create shape in database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while creating the shape.", ex);
        }
    }

    public async Task<Shape> UpdateShapeAsync(Shape shape)
    {
        try
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape), "Shape cannot be null.");
            }

            if (shape.Id <= 0)
            {
                throw new ArgumentException("Shape ID must be greater than zero.", nameof(shape));
            }

            if (string.IsNullOrWhiteSpace(shape.Type))
            {
                throw new ArgumentException("Shape type is required.", nameof(shape));
            }

            if (string.IsNullOrWhiteSpace(shape.GeometryData))
            {
                throw new ArgumentException("Geometry data is required.", nameof(shape));
            }

            var existingShape = await _dbContext.Shapes.FindAsync(shape.Id);

            if (existingShape == null)
            {
                throw new InvalidOperationException($"Shape with ID {shape.Id} not found.");
            }

            // Update properties
            existingShape.Type = shape.Type;
            existingShape.StrokeColor = shape.StrokeColor;
            existingShape.FillColor = shape.FillColor;
            existingShape.StrokeThickness = shape.StrokeThickness;
            existingShape.GeometryData = shape.GeometryData;
            existingShape.IsTemplate = shape.IsTemplate;
            existingShape.UsageCount = shape.UsageCount;
            existingShape.X = shape.X;
            existingShape.Y = shape.Y;
            existingShape.Width = shape.Width;
            existingShape.Height = shape.Height;
            existingShape.ZIndex = shape.ZIndex;

            await _dbContext.SaveChangesAsync();

            return existingShape;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to update shape in database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while updating the shape.", ex);
        }
    }

    public async Task<bool> DeleteShapeAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Shape ID must be greater than zero.", nameof(id));
            }

            var shape = await _dbContext.Shapes.FindAsync(id);

            if (shape == null)
            {
                return false;
            }

            _dbContext.Shapes.Remove(shape);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to delete shape with ID {id} from database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An unexpected error occurred while deleting shape with ID {id}.", ex);
        }
    }
}
