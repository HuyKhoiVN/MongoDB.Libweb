using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryCreateDto dto);
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(string id);
        Task<ApiResponse<CategoryDto>> GetCategoryByNameAsync(string name);
        Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync(int page = 1, int limit = 10);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(string id, CategoryUpdateDto dto);
        Task<ApiResponse<bool>> DeleteCategoryAsync(string id);
        Task<ApiResponse<long>> GetCategoryCountAsync();
    }
}
