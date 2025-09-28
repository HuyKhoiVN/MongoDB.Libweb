using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync(int page = 1, int limit = 10);
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(string id, User user);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
