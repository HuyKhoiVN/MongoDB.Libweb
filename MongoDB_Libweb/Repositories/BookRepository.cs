using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly IMongoCollection<Book> _books;

        public BookRepository(MongoDbContext context)
        {
            _books = context.Books;
        }

        public async Task<List<Book>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _books.Find(_ => true)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(string id)
        {
            return await _books.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Book>> SearchAsync(string? searchQuery, List<string>? categories, List<string>? authors, int? minYear, int? maxYear, bool? isAvailable, int page = 1, int limit = 10)
        {
            var filter = Builders<Book>.Filter.Empty;
            var skip = (page - 1) * limit;

            // Text search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                filter &= Builders<Book>.Filter.Text(searchQuery);
            }

            // Category filter
            if (categories != null && categories.Any())
            {
                filter &= Builders<Book>.Filter.AnyIn(b => b.Categories, categories);
            }

            // Author filter
            if (authors != null && authors.Any())
            {
                filter &= Builders<Book>.Filter.AnyIn(b => b.Authors, authors);
            }

            // Year range filter
            if (minYear.HasValue)
            {
                filter &= Builders<Book>.Filter.Gte(b => b.PublishYear, minYear.Value);
            }

            if (maxYear.HasValue)
            {
                filter &= Builders<Book>.Filter.Lte(b => b.PublishYear, maxYear.Value);
            }

            // Availability filter
            if (isAvailable.HasValue)
            {
                filter &= Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value);
            }

            return await _books.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Book> CreateAsync(Book book)
        {
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            await _books.InsertOneAsync(book);
            return book;
        }

        public async Task<Book?> UpdateAsync(string id, Book book)
        {
            book.UpdatedAt = DateTime.UtcNow;
            var result = await _books.FindOneAndUpdateAsync(
                b => b.Id == id,
                Builders<Book>.Update
                    .Set(b => b.Title, book.Title)
                    .Set(b => b.Authors, book.Authors)
                    .Set(b => b.Categories, book.Categories)
                    .Set(b => b.Description, book.Description)
                    .Set(b => b.PublishYear, book.PublishYear)
                    .Set(b => b.FileUrl, book.FileUrl)
                    .Set(b => b.FileFormat, book.FileFormat)
                    .Set(b => b.IsAvailable, book.IsAvailable)
                    .Set(b => b.UpdatedAt, book.UpdatedAt),
                new FindOneAndUpdateOptions<Book> { ReturnDocument = ReturnDocument.After }
            );
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _books.DeleteOneAsync(b => b.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _books.CountDocumentsAsync(_ => true);
        }

        public async Task<List<Book>> GetByCategoryAsync(string categoryId)
        {
            return await _books.Find(b => b.Categories.Contains(categoryId)).ToListAsync();
        }

        public async Task<List<Book>> GetByAuthorAsync(string authorId)
        {
            return await _books.Find(b => b.Authors.Contains(authorId)).ToListAsync();
        }

        public async Task<bool> IsAvailableAsync(string bookId)
        {
            var book = await _books.Find(b => b.Id == bookId).FirstOrDefaultAsync();
            return book?.IsAvailable ?? false;
        }

        public async Task<bool> SetAvailabilityAsync(string bookId, bool isAvailable)
        {
            var result = await _books.UpdateOneAsync(
                b => b.Id == bookId,
                Builders<Book>.Update
                    .Set(b => b.IsAvailable, isAvailable)
                    .Set(b => b.UpdatedAt, DateTime.UtcNow)
            );
            return result.ModifiedCount > 0;
        }
    }
}
