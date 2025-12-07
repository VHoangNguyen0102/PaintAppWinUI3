using System.Collections.Generic;
using System.Threading.Tasks;
using PaintApp.Models;

namespace PaintApp.Services;

public interface ICanvasService
{
    Task<List<Canvas>> GetCanvasesByProfileIdAsync(int profileId);
    Task<Canvas?> GetCanvasByIdAsync(int id);
    Task<Canvas> CreateCanvasAsync(Canvas canvas);
    Task<Canvas> UpdateCanvasAsync(Canvas canvas);
    Task<bool> DeleteCanvasAsync(int id);
}
