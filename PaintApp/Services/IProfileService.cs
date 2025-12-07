using System.Collections.Generic;
using System.Threading.Tasks;
using PaintApp.Models;

namespace PaintApp.Services;

public interface IProfileService
{
    Task<List<Profile>> GetAllProfilesAsync();
    Task<Profile?> GetProfileByIdAsync(int id);
    Task<Profile> CreateProfileAsync(Profile profile);
    Task<Profile> UpdateProfileAsync(Profile profile);
    Task<bool> DeleteProfileAsync(int id);
}
