using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly IMongoCollection<Book> _books;
        private readonly IMongoCollection<Borrow> _borrows;
        private readonly IMongoCollection<Author> _authors; // Adjusted for multi Id
        private readonly IMongoCollection<Category> _categories; // Adjusted for multi Id

        public BookRepository(MongoDbContext context)
        {
            _books = context.Books;
            _borrows = context.Borrows;
            _authors = context.Authors; // Adjusted for multi Id
            _categories = context.Categories; // Adjusted for multi Id
        }

        public async Task<List<Book>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _books.Find(_ => true)
                .Sort(Builders<Book>.Sort.Descending(b => b.CreatedAt))
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(string id)
        {
            return await _books.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Book>> SearchAsync(BookSearchDto searchDto)
        {
            var filter = Builders<Book>.Filter.Empty;
            var skip = (searchDto.Page - 1) * searchDto.Limit;

            // Text search
            if (!string.IsNullOrEmpty(searchDto.SearchQuery))
            {
                filter &= Builders<Book>.Filter.Text(searchDto.SearchQuery);
            }

            // Category filter - multi-select
            if (searchDto.Categories != null && searchDto.Categories.Any())
            {
                filter &= Builders<Book>.Filter.AnyIn(b => b.Categories, searchDto.Categories);
            }

            // Author filter - multi-select
            if (searchDto.Authors != null && searchDto.Authors.Any())
            {
                filter &= Builders<Book>.Filter.AnyIn(b => b.Authors, searchDto.Authors);
            }

            // Year range filter
            if (searchDto.MinYear.HasValue)
            {
                filter &= Builders<Book>.Filter.And(
                    Builders<Book>.Filter.Ne(b => b.PublishYear, null),
                    Builders<Book>.Filter.Gte(b => b.PublishYear, searchDto.MinYear.Value)
                );
            }

            if (searchDto.MaxYear.HasValue)
            {
                filter &= Builders<Book>.Filter.And(
                    Builders<Book>.Filter.Ne(b => b.PublishYear, null),
                    Builders<Book>.Filter.Lte(b => b.PublishYear, searchDto.MaxYear.Value)
                );
            }

            // Availability filter
            if (searchDto.IsAvailable.HasValue)
            {
                filter &= Builders<Book>.Filter.Eq(b => b.IsAvailable, searchDto.IsAvailable.Value);
            }

            return await _books.Find(filter)
                .Sort(Builders<Book>.Sort.Descending(b => b.CreatedAt))
                .Skip(skip)
                .Limit(searchDto.Limit)
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

        public async Task<long> CountSearchAsync(BookSearchDto searchDto)
        {
            var filter = Builders<Book>.Filter.Empty;

            // Text search
            if (!string.IsNullOrEmpty(searchDto.SearchQuery))
            {
                filter &= Builders<Book>.Filter.Text(searchDto.SearchQuery);
            }

            // Category filter - multi-select
            if (searchDto.Categories != null && searchDto.Categories.Any())
            {
                filter &= Builders<Book>.Filter.AnyIn(b => b.Categories, searchDto.Categories);
            }

            // Author filter - multi-select
            if (searchDto.Authors != null && searchDto.Authors.Any())
            {
                filter &= Builders<Book>.Filter.AnyIn(b => b.Authors, searchDto.Authors);
            }

            // Year range filter
            if (searchDto.MinYear.HasValue)
            {
                filter &= Builders<Book>.Filter.And(
                    Builders<Book>.Filter.Ne(b => b.PublishYear, null),
                    Builders<Book>.Filter.Gte(b => b.PublishYear, searchDto.MinYear.Value)
                );
            }

            if (searchDto.MaxYear.HasValue)
            {
                filter &= Builders<Book>.Filter.And(
                    Builders<Book>.Filter.Ne(b => b.PublishYear, null),
                    Builders<Book>.Filter.Lte(b => b.PublishYear, searchDto.MaxYear.Value)
                );
            }

            // Availability filter
            if (searchDto.IsAvailable.HasValue)
            {
                filter &= Builders<Book>.Filter.Eq(b => b.IsAvailable, searchDto.IsAvailable.Value);
            }

            return await _books.CountDocumentsAsync(filter);
        }

        public async Task<List<Book>> GetByCategoryAsync(string categoryId)
        {
            return await _books.Find(b => b.Categories.Contains(categoryId)).ToListAsync();
        }

        public async Task<List<Book>> GetByCategoryAsync(string categoryId, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _books.Find(b => b.Categories.Contains(categoryId))
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<Book>> GetByAuthorAsync(string authorId)
        {
            return await _books.Find(b => b.Authors.Contains(authorId)).ToListAsync();
        }

        public async Task<List<Book>> GetByAuthorAsync(string authorId, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _books.Find(b => b.Authors.Contains(authorId))
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<Book>> GetFeaturedBooksAsync(int limit = 6)
        {
            return await _books.Find(_ => true)
                .Sort(Builders<Book>.Sort.Descending(b => b.CreatedAt))
                .Limit(limit)
                .ToListAsync();
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

        public async Task<bool> HasActiveBorrowsAsync(string bookId)
        {
            var count = await _borrows.CountDocumentsAsync(
                b => b.BookId == bookId && b.Status == "Borrowed"
            );
            return count > 0;
        }

        public async Task<List<SelectOptionDto>> GetAllAuthorsAsync()
        {
            // Adjusted for multi Id - Get from Author collection instead of Book collection
            var authors = await _authors.Find(_ => true)
                .Sort(Builders<Author>.Sort.Ascending(a => a.Name))
                .ToListAsync();
            
            return authors.Select(author => new SelectOptionDto
            {
                Id = author.Id,
                Value = author.Name
            }).ToList();
        }

        public async Task<List<SelectOptionDto>> GetAllCategoriesAsync()
        {
            // Adjusted for multi Id - Get from Category collection instead of Book collection
            var categories = await _categories.Find(_ => true)
                .Sort(Builders<Category>.Sort.Ascending(c => c.Name))
                .ToListAsync();
            
            return categories.Select(category => new SelectOptionDto
            {
                Id = category.Id,
                Value = category.Name
            }).ToList();
        }

        // Adjusted for multi Id - Add methods to create new authors and categories
        public async Task<Author?> GetAuthorByIdAsync(string id)
        {
            return await _authors.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Author?> GetAuthorByNameAsync(string name)
        {
            return await _authors.Find(a => a.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Author> CreateAuthorAsync(Author author)
        {
            author.CreatedAt = DateTime.UtcNow;
            await _authors.InsertOneAsync(author);
            return author;
        }

        public async Task<Category?> GetCategoryByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _categories.Find(c => c.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            await _categories.InsertOneAsync(category);
            return category;
        }
    }
}
