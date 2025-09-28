using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(int page = 1, int limit = 10);
        Task<Category?> GetByIdAsync(string id);
        Task<Category?> GetByNameAsync(string name);
        Task<Category> CreateAsync(Category category);
        Task<Category?> UpdateAsync(string id, Category category);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<bool> ExistsByNameAsync(string name);
    }
}
