using System.Collections.Generic;
using System.Threading.Tasks;
using PaintApp.Models;

namespace PaintApp.Services;

/// <summary>
/// Interface cho Shape Service - qu?n lý shapes trong canvas
/// </summary>
public interface IShapeService
{
    /// <summary>
    /// L?y t?t c? shapes c?a m?t canvas
    /// </summary>
    /// <param name="canvasId">ID c?a canvas</param>
    /// <returns>List các shapes thu?c canvas</returns>
    Task<List<Shape>> GetShapesByCanvasIdAsync(int canvasId);
    
    /// <summary>
    /// L?y các template shapes (shapes m?u có th? reuse)
    /// </summary>
    /// <returns>List các template shapes</returns>
    Task<List<Shape>> GetTemplateShapesAsync();
    
    /// <summary>
    /// T?o shape m?i
    /// </summary>
    /// <param name="shape">Shape c?n t?o</param>
    /// <returns>Shape ?ã ???c t?o v?i ID</returns>
    Task<Shape> CreateShapeAsync(Shape shape);
    
    /// <summary>
    /// C?p nh?t thông tin shape
    /// </summary>
    /// <param name="shape">Shape v?i thông tin m?i</param>
    /// <returns>Shape ?ã ???c c?p nh?t</returns>
    Task<Shape> UpdateShapeAsync(Shape shape);
    
    /// <summary>
    /// Xóa shape theo ID
    /// </summary>
    /// <param name="id">ID c?a shape c?n xóa</param>
    /// <returns>True n?u xóa thành công, False n?u không tìm th?y</returns>
    Task<bool> DeleteShapeAsync(int id);
}
