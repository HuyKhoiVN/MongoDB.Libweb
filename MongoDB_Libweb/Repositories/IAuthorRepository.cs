using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface IAuthorRepository
    {
        Task<List<Author>> GetAllAsync(int page = 1, int limit = 10);
        Task<Author?> GetByIdAsync(string id);
        Task<Author?> GetByNameAsync(string name);
        Task<Author> CreateAsync(Author author);
        Task<Author?> UpdateAsync(string id, Author author);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<bool> ExistsByNameAsync(string name);
    }
}
