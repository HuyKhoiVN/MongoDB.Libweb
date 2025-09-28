using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        public async Task<List<User>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _users.Find(u => u.IsActive)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<User?> UpdateAsync(string id, User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _users.FindOneAndUpdateAsync(
                u => u.Id == id,
                Builders<User>.Update
                    .Set(u => u.Username, user.Username)
                    .Set(u => u.Email, user.Email)
                    .Set(u => u.FullName, user.FullName)
                    .Set(u => u.Role, user.Role)
                    .Set(u => u.IsActive, user.IsActive)
                    .Set(u => u.UpdatedAt, user.UpdatedAt),
                new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After }
            );
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _users.CountDocumentsAsync(u => u.IsActive);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            var count = await _users.CountDocumentsAsync(u => u.Username == username);
            return count > 0;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var count = await _users.CountDocumentsAsync(u => u.Email == email);
            return count > 0;
        }
    }
}
