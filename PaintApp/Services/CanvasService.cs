using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintApp.Data;
using PaintApp.Models;

namespace PaintApp.Services;

public class CanvasService : ICanvasService
{
    private readonly AppDbContext _dbContext;

    public CanvasService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Canvas>> GetCanvasesByProfileIdAsync(int profileId)
    {
        try
        {
            if (profileId <= 0)
            {
                throw new ArgumentException("Profile ID must be greater than zero.", nameof(profileId));
            }

            return await _dbContext.Canvases
                .Include(c => c.Shapes)
                .Where(c => c.ProfileId == profileId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve canvases for profile {profileId}.", ex);
        }
    }

    public async Task<Canvas?> GetCanvasByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Canvas ID must be greater than zero.", nameof(id));
            }

            return await _dbContext.Canvases
                .Include(c => c.Shapes)
                .Include(c => c.Profile)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve canvas with ID {id}.", ex);
        }
    }

    public async Task<Canvas> CreateCanvasAsync(Canvas canvas)
    {
        try
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas), "Canvas cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(canvas.Name))
            {
                throw new ArgumentException("Canvas name is required.", nameof(canvas));
            }

            if (canvas.Width <= 0)
            {
                throw new ArgumentException("Canvas width must be greater than zero.", nameof(canvas));
            }

            if (canvas.Height <= 0)
            {
                throw new ArgumentException("Canvas height must be greater than zero.", nameof(canvas));
            }

            if (canvas.ProfileId <= 0)
            {
                throw new ArgumentException("Profile ID must be greater than zero.", nameof(canvas));
            }

            canvas.CreatedAt = DateTime.Now;
            canvas.UpdatedAt = null;

            _dbContext.Canvases.Add(canvas);
            await _dbContext.SaveChangesAsync();

            return canvas;
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
            throw new InvalidOperationException("Failed to create canvas in database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while creating the canvas.", ex);
        }
    }

    public async Task<Canvas> UpdateCanvasAsync(Canvas canvas)
    {
        try
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas), "Canvas cannot be null.");
            }

            if (canvas.Id <= 0)
            {
                throw new ArgumentException("Canvas ID must be greater than zero.", nameof(canvas));
            }

            if (string.IsNullOrWhiteSpace(canvas.Name))
            {
                throw new ArgumentException("Canvas name is required.", nameof(canvas));
            }

            if (canvas.Width <= 0)
            {
                throw new ArgumentException("Canvas width must be greater than zero.", nameof(canvas));
            }

            if (canvas.Height <= 0)
            {
                throw new ArgumentException("Canvas height must be greater than zero.", nameof(canvas));
            }

            var existingCanvas = await _dbContext.Canvases.FindAsync(canvas.Id);

            if (existingCanvas == null)
            {
                throw new InvalidOperationException($"Canvas with ID {canvas.Id} not found.");
            }

            existingCanvas.Name = canvas.Name;
            existingCanvas.Width = canvas.Width;
            existingCanvas.Height = canvas.Height;
            existingCanvas.BackgroundColor = canvas.BackgroundColor;
            existingCanvas.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return existingCanvas;
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
            throw new InvalidOperationException("Failed to update canvas in database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while updating the canvas.", ex);
        }
    }

    public async Task<bool> DeleteCanvasAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Canvas ID must be greater than zero.", nameof(id));
            }

            var canvas = await _dbContext.Canvases
                .Include(c => c.Shapes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (canvas == null)
            {
                return false;
            }

            _dbContext.Canvases.Remove(canvas);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to delete canvas with ID {id} from database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An unexpected error occurred while deleting canvas with ID {id}.", ex);
        }
    }
}
