using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllAsync(int page = 1, int limit = 10);
        Task<Book?> GetByIdAsync(string id);
        Task<List<Book>> SearchAsync(BookSearchDto searchDto);
        Task<Book> CreateAsync(Book book);
        Task<Book?> UpdateAsync(string id, Book book);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<long> CountSearchAsync(BookSearchDto searchDto);
        Task<List<Book>> GetByCategoryAsync(string categoryId);
        Task<List<Book>> GetByCategoryAsync(string categoryId, int page = 1, int limit = 10);
        Task<List<Book>> GetByAuthorAsync(string authorId);
        Task<List<Book>> GetByAuthorAsync(string authorId, int page = 1, int limit = 10);
        Task<List<Book>> GetFeaturedBooksAsync(int limit = 6);
        Task<bool> IsAvailableAsync(string bookId);
        Task<bool> SetAvailabilityAsync(string bookId, bool isAvailable);
        Task<bool> HasActiveBorrowsAsync(string bookId);
        Task<List<SelectOptionDto>> GetAllAuthorsAsync(); // Adjusted for multi Id
        Task<List<SelectOptionDto>> GetAllCategoriesAsync(); // Adjusted for multi Id
        Task<Author?> GetAuthorByIdAsync(string id); // Adjusted for multi Id
        Task<Author?> GetAuthorByNameAsync(string name); // Adjusted for multi Id
        Task<Author> CreateAuthorAsync(Author author); // Adjusted for multi Id
        Task<Category?> GetCategoryByIdAsync(string id); // Adjusted for multi Id
        Task<Category?> GetCategoryByNameAsync(string name); // Adjusted for multi Id
        Task<Category> CreateCategoryAsync(Category category); // Adjusted for multi Id
    }
}
