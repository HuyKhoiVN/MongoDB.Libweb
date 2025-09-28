using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface IBorrowRepository
    {
        Task<List<Borrow>> GetAllAsync(int page = 1, int limit = 10);
        Task<Borrow?> GetByIdAsync(string id);
        Task<List<Borrow>> GetByUserIdAsync(string userId, int page = 1, int limit = 10);
        Task<List<Borrow>> GetByBookIdAsync(string bookId, int page = 1, int limit = 10);
        Task<List<Borrow>> GetByStatusAsync(string status, int page = 1, int limit = 10);
        Task<List<Borrow>> GetOverdueBorrowsAsync();
        Task<Borrow> CreateAsync(Borrow borrow);
        Task<Borrow?> UpdateAsync(string id, Borrow borrow);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<bool> HasActiveBorrowAsync(string userId, string bookId);
        Task<List<Borrow>> GetActiveBorrowsByUserAsync(string userId);
    }
}
