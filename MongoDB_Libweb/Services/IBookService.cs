using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Services
{
    public interface IBookService
    {
        Task<ApiResponse<BookDto>> CreateBookAsync(BookCreateDto dto);
        Task<ApiResponse<BookDto>> GetBookByIdAsync(string id);
        Task<ApiResponse<List<BookDto>>> GetAllBooksAsync(int page = 1, int limit = 10);
        Task<ApiResponse<List<BookDto>>> SearchBooksAsync(BookSearchDto searchDto);
        Task<ApiResponse<BookDto>> UpdateBookAsync(string id, BookUpdateDto dto);
        Task<ApiResponse<bool>> DeleteBookAsync(string id);
        Task<ApiResponse<long>> GetBookCountAsync();
        Task<ApiResponse<List<BookDto>>> GetBooksByCategoryAsync(string categoryId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BookDto>>> GetBooksByAuthorAsync(string authorId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BookDto>>> GetFeaturedBooksAsync(int limit = 6);
        Task<ApiResponse<bool>> SetBookAvailabilityAsync(string bookId, bool isAvailable);
    }
}
