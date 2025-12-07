using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintApp.Data;
using PaintApp.Models;

namespace PaintApp.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _dbContext;

    public ProfileService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Profile>> GetAllProfilesAsync()
    {
        try
        {
            return await _dbContext.Profiles
                .Include(p => p.Drawings)
                .Include(p => p.Canvases)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to retrieve profiles from database.", ex);
        }
    }

    public async Task<Profile?> GetProfileByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Profile ID must be greater than zero.", nameof(id));
            }

            return await _dbContext.Profiles
                .Include(p => p.Drawings)
                .Include(p => p.Canvases)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve profile with ID {id}.", ex);
        }
    }

    public async Task<Profile> CreateProfileAsync(Profile profile)
    {
        try
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                throw new ArgumentException("Profile name is required.", nameof(profile));
            }

            profile.CreatedAt = DateTime.Now;
            
            _dbContext.Profiles.Add(profile);
            await _dbContext.SaveChangesAsync();
            
            return profile;
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
            throw new InvalidOperationException("Failed to create profile in database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while creating the profile.", ex);
        }
    }

    public async Task<Profile> UpdateProfileAsync(Profile profile)
    {
        try
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
            }

            if (profile.Id <= 0)
            {
                throw new ArgumentException("Profile ID must be greater than zero.", nameof(profile));
            }

            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                throw new ArgumentException("Profile name is required.", nameof(profile));
            }

            var existingProfile = await _dbContext.Profiles.FindAsync(profile.Id);
            
            if (existingProfile == null)
            {
                throw new InvalidOperationException($"Profile with ID {profile.Id} not found.");
            }

            existingProfile.Name = profile.Name;
            existingProfile.AvatarPath = profile.AvatarPath;
            existingProfile.Theme = profile.Theme;
            existingProfile.DefaultCanvasWidth = profile.DefaultCanvasWidth;
            existingProfile.DefaultCanvasHeight = profile.DefaultCanvasHeight;
            existingProfile.DefaultCanvasBackgroundColor = profile.DefaultCanvasBackgroundColor;
            existingProfile.DefaultStrokeThickness = profile.DefaultStrokeThickness;
            existingProfile.DefaultStrokeColor = profile.DefaultStrokeColor;
            existingProfile.DefaultFillColor = profile.DefaultFillColor;

            await _dbContext.SaveChangesAsync();
            
            return existingProfile;
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
            throw new InvalidOperationException("Failed to update profile in database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while updating the profile.", ex);
        }
    }

    public async Task<bool> DeleteProfileAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Profile ID must be greater than zero.", nameof(id));
            }

            var profile = await _dbContext.Profiles.FindAsync(id);
            
            if (profile == null)
            {
                return false;
            }

            _dbContext.Profiles.Remove(profile);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to delete profile with ID {id} from database.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An unexpected error occurred while deleting profile with ID {id}.", ex);
        }
    }
}
