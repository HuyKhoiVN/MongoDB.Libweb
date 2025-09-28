using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly IMongoCollection<Author> _authors;

        public AuthorRepository(MongoDbContext context)
        {
            _authors = context.Authors;
        }

        public async Task<List<Author>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _authors.Find(_ => true)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Author?> GetByIdAsync(string id)
        {
            return await _authors.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Author?> GetByNameAsync(string name)
        {
            return await _authors.Find(a => a.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Author> CreateAsync(Author author)
        {
            author.CreatedAt = DateTime.UtcNow;
            await _authors.InsertOneAsync(author);
            return author;
        }

        public async Task<Author?> UpdateAsync(string id, Author author)
        {
            var result = await _authors.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Author>.Update
                    .Set(a => a.Name, author.Name)
                    .Set(a => a.Bio, author.Bio),
                new FindOneAndUpdateOptions<Author> { ReturnDocument = ReturnDocument.After }
            );
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _authors.DeleteOneAsync(a => a.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _authors.CountDocumentsAsync(_ => true);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            var count = await _authors.CountDocumentsAsync(a => a.Name == name);
            return count > 0;
        }
    }
}
