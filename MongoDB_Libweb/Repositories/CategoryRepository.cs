using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(MongoDbContext context)
        {
            _categories = context.Categories;
        }

        public async Task<List<Category>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _categories.Find(_ => true)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _categories.Find(c => c.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            await _categories.InsertOneAsync(category);
            return category;
        }

        public async Task<Category?> UpdateAsync(string id, Category category)
        {
            var result = await _categories.FindOneAndUpdateAsync(
                c => c.Id == id,
                Builders<Category>.Update
                    .Set(c => c.Name, category.Name)
                    .Set(c => c.Description, category.Description),
                new FindOneAndUpdateOptions<Category> { ReturnDocument = ReturnDocument.After }
            );
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _categories.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _categories.CountDocumentsAsync(_ => true);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            var count = await _categories.CountDocumentsAsync(c => c.Name == name);
            return count > 0;
        }
    }
}
