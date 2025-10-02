using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllAsync(int page = 1, int limit = 10);
        Task<Book?> GetByIdAsync(string id);
        Task<List<Book>> SearchAsync(string? searchQuery, List<string>? categories, List<string>? authors, int? minYear, int? maxYear, bool? isAvailable, int page = 1, int limit = 10);
        Task<Book> CreateAsync(Book book);
        Task<Book?> UpdateAsync(string id, Book book);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<List<Book>> GetByCategoryAsync(string categoryId);
        Task<List<Book>> GetByCategoryAsync(string categoryId, int page = 1, int limit = 10);
        Task<List<Book>> GetByAuthorAsync(string authorId);
        Task<List<Book>> GetByAuthorAsync(string authorId, int page = 1, int limit = 10);
        Task<List<Book>> GetFeaturedBooksAsync(int limit = 6);
        Task<bool> IsAvailableAsync(string bookId);
        Task<bool> SetAvailabilityAsync(string bookId, bool isAvailable);
    }
}
