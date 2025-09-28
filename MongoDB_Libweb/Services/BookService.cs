using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<ApiResponse<BookDto>> CreateBookAsync(BookCreateDto dto)
        {
            try
            {
                var book = new Book
                {
                    Title = dto.Title,
                    Authors = dto.Authors,
                    Categories = dto.Categories,
                    Description = dto.Description,
                    PublishYear = dto.PublishYear,
                    FileUrl = dto.FileUrl,
                    FileFormat = dto.FileFormat,
                    IsAvailable = true
                };

                var createdBook = await _bookRepository.CreateAsync(book);
                var bookDto = MapToDto(createdBook);

                return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BookDto>.ErrorResponse($"Failed to create book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BookDto>> GetBookByIdAsync(string id)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book not found");
                }

                var bookDto = MapToDto(book);
                return ApiResponse<BookDto>.SuccessResponse(bookDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BookDto>.ErrorResponse($"Failed to get book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDto>>> GetAllBooksAsync(int page = 1, int limit = 10)
        {
            try
            {
                var books = await _bookRepository.GetAllAsync(page, limit);
                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to get books: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDto>>> SearchBooksAsync(BookSearchDto searchDto)
        {
            try
            {
                var books = await _bookRepository.SearchAsync(
                    searchDto.SearchQuery,
                    searchDto.Categories,
                    searchDto.Authors,
                    searchDto.MinYear,
                    searchDto.MaxYear,
                    searchDto.IsAvailable,
                    searchDto.Page,
                    searchDto.Limit
                );

                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to search books: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BookDto>> UpdateBookAsync(string id, BookUpdateDto dto)
        {
            try
            {
                var existingBook = await _bookRepository.GetByIdAsync(id);
                if (existingBook == null)
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.Title))
                    existingBook.Title = dto.Title;

                if (dto.Authors != null)
                    existingBook.Authors = dto.Authors;

                if (dto.Categories != null)
                    existingBook.Categories = dto.Categories;

                if (!string.IsNullOrEmpty(dto.Description))
                    existingBook.Description = dto.Description;

                if (dto.PublishYear.HasValue)
                    existingBook.PublishYear = dto.PublishYear.Value;

                if (!string.IsNullOrEmpty(dto.FileUrl))
                    existingBook.FileUrl = dto.FileUrl;

                if (!string.IsNullOrEmpty(dto.FileFormat))
                    existingBook.FileFormat = dto.FileFormat;

                if (dto.IsAvailable.HasValue)
                    existingBook.IsAvailable = dto.IsAvailable.Value;

                var updatedBook = await _bookRepository.UpdateAsync(id, existingBook);
                if (updatedBook == null)
                {
                    return ApiResponse<BookDto>.ErrorResponse("Failed to update book");
                }

                var bookDto = MapToDto(updatedBook);
                return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BookDto>.ErrorResponse($"Failed to update book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBookAsync(string id)
        {
            try
            {
                var result = await _bookRepository.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Book not found");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Book deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to delete book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetBookCountAsync()
        {
            try
            {
                var count = await _bookRepository.CountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get book count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDto>>> GetBooksByCategoryAsync(string categoryId)
        {
            try
            {
                var books = await _bookRepository.GetByCategoryAsync(categoryId);
                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to get books by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDto>>> GetBooksByAuthorAsync(string authorId)
        {
            try
            {
                var books = await _bookRepository.GetByAuthorAsync(authorId);
                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to get books by author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SetBookAvailabilityAsync(string bookId, bool isAvailable)
        {
            try
            {
                var result = await _bookRepository.SetAvailabilityAsync(bookId, isAvailable);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Book not found or failed to update availability");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Book availability updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to update book availability: {ex.Message}");
            }
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Categories = book.Categories,
                Description = book.Description,
                PublishYear = book.PublishYear,
                FileUrl = book.FileUrl,
                FileFormat = book.FileFormat,
                IsAvailable = book.IsAvailable,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }
    }
}
