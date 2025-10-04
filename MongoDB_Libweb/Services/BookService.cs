using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileUploadRepository _fileUploadRepository;
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
        private static readonly string[] AllowedFileTypes = { "application/pdf", "application/epub+zip" };

        public BookService(IBookRepository bookRepository, IWebHostEnvironment webHostEnvironment, IFileUploadRepository fileUploadRepository)
        {
            _bookRepository = bookRepository;
            _webHostEnvironment = webHostEnvironment;
            _fileUploadRepository = fileUploadRepository;
        }

        public async Task<ApiResponse<BookDto>> CreateBookAsync(BookCreateDto dto)
        {
            try
            {
                // Validate file
                var fileValidation = await ValidateFileAsync(dto.File);
                if (!fileValidation.Success)
                {
                    return ApiResponse<BookDto>.ErrorResponse(fileValidation.Error);
                }

                // Adjusted for multi Id - Parse JSON strings to List<string>
                var authorsIds = ParseJsonStringToList(dto.AuthorsIds);
                var categoriesIds = ParseJsonStringToList(dto.CategoriesIds);

                // Validate authors exist
                var authorValidation = await ValidateAuthorsExistAsync(authorsIds);
                if (!authorValidation.Success)
                {
                    return ApiResponse<BookDto>.ErrorResponse(authorValidation.Error);
                }

                // Validate categories exist
                var categoryValidation = await ValidateCategoriesExistAsync(categoriesIds);
                if (!categoryValidation.Success)
                {
                    return ApiResponse<BookDto>.ErrorResponse(categoryValidation.Error);
                }

                // Save file
                var fileSaveResult = await SaveFileAsync(dto.File);
                if (!fileSaveResult.Success)
                {
                    return ApiResponse<BookDto>.ErrorResponse(fileSaveResult.Error);
                }

                var fileFormat = GetFileFormat(dto.File.ContentType);

                var book = new Book
                {
                    Title = dto.Title,
                    Authors = authorsIds, // Adjusted for multi Id - Use parsed ObjectIds
                    Categories = categoriesIds, // Adjusted for multi Id - Use parsed ObjectIds
                    Description = dto.Description,
                    PublishYear = dto.PublishYear,
                    FileUrl = fileSaveResult.Data,
                    FileFormat = fileFormat,
                    IsAvailable = true
                };

                var createdBook = await _bookRepository.CreateAsync(book);

                // Save file metadata
                await _fileUploadRepository.CreateAsync(new FileUpload
                {
                    Filename = dto.File.FileName,
                    Length = dto.File.Length,
                    UploadDate = DateTime.UtcNow,
                    BookId = createdBook.Id
                });

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
                var books = await _bookRepository.SearchAsync(searchDto);
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

                // Adjusted for multi Id - Validate and update authors
                if (dto.AuthorsIds != null)
                {
                    var authorsIds = ParseJsonStringToList(dto.AuthorsIds);
                    var authorValidation = await ValidateAuthorsExistAsync(authorsIds);
                    if (!authorValidation.Success)
                    {
                        return ApiResponse<BookDto>.ErrorResponse(authorValidation.Error);
                    }
                    existingBook.Authors = authorsIds;
                }

                // Adjusted for multi Id - Validate and update categories
                if (dto.CategoriesIds != null)
                {
                    var categoriesIds = ParseJsonStringToList(dto.CategoriesIds);
                    var categoryValidation = await ValidateCategoriesExistAsync(categoriesIds);
                    if (!categoryValidation.Success)
                    {
                        return ApiResponse<BookDto>.ErrorResponse(categoryValidation.Error);
                    }
                    existingBook.Categories = categoriesIds;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                    existingBook.Description = dto.Description;

                if (dto.PublishYear.HasValue)
                    existingBook.PublishYear = dto.PublishYear.Value;

                if (dto.IsAvailable.HasValue)
                    existingBook.IsAvailable = dto.IsAvailable.Value;

                // Handle file update
                if (dto.File != null && dto.ReplaceFile)
                {
                    // Validate new file
                    var fileValidation = await ValidateFileAsync(dto.File);
                    if (!fileValidation.Success)
                    {
                        return ApiResponse<BookDto>.ErrorResponse(fileValidation.Error);
                    }

                    // Delete old file
                    if (!string.IsNullOrEmpty(existingBook.FileUrl))
                    {
                        await DeleteFileAsync(existingBook.FileUrl);
                    }

                    // Save new file
                    var fileSaveResult = await SaveFileAsync(dto.File);
                    if (!fileSaveResult.Success)
                    {
                        return ApiResponse<BookDto>.ErrorResponse(fileSaveResult.Error);
                    }

                    existingBook.FileUrl = fileSaveResult.Data;
                    existingBook.FileFormat = GetFileFormat(dto.File.ContentType);

                    // Update file metadata
                    await _fileUploadRepository.UpdateByBookIdAsync(id, new FileUpload
                    {
                        Filename = dto.File.FileName,
                        Length = dto.File.Length,
                        UploadDate = DateTime.UtcNow,
                        BookId = id
                    });
                }

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
                // Check if book has active borrows
                var hasActiveBorrows = await _bookRepository.HasActiveBorrowsAsync(id);
                if (hasActiveBorrows)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete book with active borrows");
                }

                // Get book to delete file
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Book not found");
                }

                // Delete file
                if (!string.IsNullOrEmpty(book.FileUrl))
                {
                    await DeleteFileAsync(book.FileUrl);
                }

                // Delete file metadata
                await _fileUploadRepository.DeleteByBookIdAsync(id);

                // Delete book
                var result = await _bookRepository.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to delete book");
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

        public async Task<ApiResponse<List<BookDto>>> GetBooksByCategoryAsync(string categoryId, int page = 1, int limit = 10)
        {
            try
            {
                var books = await _bookRepository.GetByCategoryAsync(categoryId, page, limit);
                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to get books by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDto>>> GetBooksByAuthorAsync(string authorId, int page = 1, int limit = 10)
        {
            try
            {
                var books = await _bookRepository.GetByAuthorAsync(authorId, page, limit);
                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to get books by author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDto>>> GetFeaturedBooksAsync(int limit = 6)
        {
            try
            {
                // Get the most recently added books as featured books
                var books = await _bookRepository.GetFeaturedBooksAsync(limit);
                var bookDtos = books.Select(MapToDto).ToList();
                return ApiResponse<List<BookDto>>.SuccessResponse(bookDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDto>>.ErrorResponse($"Failed to get featured books: {ex.Message}");
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

        public async Task<ApiResponse<long>> GetSearchCountAsync(BookSearchDto searchDto)
        {
            try
            {
                var count = await _bookRepository.CountSearchAsync(searchDto);
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get search count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SelectOptionDto>>> GetAllAuthorsAsync()
        {
            try
            {
                var authors = await _bookRepository.GetAllAuthorsAsync();
                return ApiResponse<List<SelectOptionDto>>.SuccessResponse(authors);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SelectOptionDto>>.ErrorResponse($"Failed to get authors: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SelectOptionDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _bookRepository.GetAllCategoriesAsync();
                return ApiResponse<List<SelectOptionDto>>.SuccessResponse(categories);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SelectOptionDto>>.ErrorResponse($"Failed to get categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BookDisplayDto>>> GetAllBooksWithDisplayNamesAsync(int page = 1, int limit = 10)
        {
            try
            {
                var books = await _bookRepository.GetAllAsync(page, limit);
                var bookDisplayDtos = new List<BookDisplayDto>();
                
                foreach (var book in books)
                {
                    var bookDto = MapToDto(book);
                    var displayDto = await MapToDisplayDtoAsync(bookDto);
                    bookDisplayDtos.Add(displayDto);
                }
                
                return ApiResponse<List<BookDisplayDto>>.SuccessResponse(bookDisplayDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BookDisplayDto>>.ErrorResponse($"Failed to get books with display names: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ValidateFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return ApiResponse<bool>.ErrorResponse("File is required");
                }

                if (file.Length > MaxFileSize)
                {
                    return ApiResponse<bool>.ErrorResponse($"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB");
                }

                if (!AllowedFileTypes.Contains(file.ContentType))
                {
                    return ApiResponse<bool>.ErrorResponse("Only PDF and EPUB files are allowed");
                }

                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"File validation failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> SaveFileAsync(IFormFile file)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "files", "books");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var relativePath = $"/files/books/{uniqueFileName}";
                return ApiResponse<string>.SuccessResponse(relativePath);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Failed to save file: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return true;

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetFileFormat(string contentType)
        {
            return contentType switch
            {
                "application/pdf" => "PDF",
                "application/epub+zip" => "EPUB",
                _ => "Unknown"
            };
        }

        // Adjusted for multi Id - Add method to process authors and categories
        private async Task<List<string>> ProcessAuthorsAsync(List<string> authorIds, List<string>? authorNames = null)
        {
            var processedAuthors = new List<string>();
            
            // Process existing author IDs
            foreach (var authorId in authorIds)
            {
                if (!string.IsNullOrEmpty(authorId))
                {
                    processedAuthors.Add(authorId);
                }
            }
            
            // Process new author names if provided
            if (authorNames != null)
            {
                foreach (var authorName in authorNames)
                {
                    if (!string.IsNullOrEmpty(authorName))
                    {
                        // Check if author already exists
                        var existingAuthor = await _bookRepository.GetAuthorByNameAsync(authorName);
                        if (existingAuthor != null)
                        {
                            if (!processedAuthors.Contains(existingAuthor.Id))
                            {
                                processedAuthors.Add(existingAuthor.Id);
                            }
                        }
                        else
                        {
                            // Create new author
                            var newAuthor = new Author
                            {
                                Name = authorName,
                                Bio = $"Author: {authorName}"
                            };
                            var createdAuthor = await _bookRepository.CreateAuthorAsync(newAuthor);
                            processedAuthors.Add(createdAuthor.Id);
                        }
                    }
                }
            }
            
            return processedAuthors;
        }

        // Adjusted for multi Id - Add method to process categories
        private async Task<List<string>> ProcessCategoriesAsync(List<string> categoryIds, List<string>? categoryNames = null)
        {
            var processedCategories = new List<string>();
            
            // Process existing category IDs
            foreach (var categoryId in categoryIds)
            {
                if (!string.IsNullOrEmpty(categoryId))
                {
                    processedCategories.Add(categoryId);
                }
            }
            
            // Process new category names if provided
            if (categoryNames != null)
            {
                foreach (var categoryName in categoryNames)
                {
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        // Check if category already exists
                        var existingCategory = await _bookRepository.GetCategoryByNameAsync(categoryName);
                        if (existingCategory != null)
                        {
                            if (!processedCategories.Contains(existingCategory.Id))
                            {
                                processedCategories.Add(existingCategory.Id);
                            }
                        }
                        else
                        {
                            // Create new category
                            var newCategory = new Category
                            {
                                Name = categoryName,
                                Description = $"Category: {categoryName}"
                            };
                            var createdCategory = await _bookRepository.CreateCategoryAsync(newCategory);
                            processedCategories.Add(createdCategory.Id);
                        }
                    }
                }
            }
            
            return processedCategories;
        }

        private BookDto MapToDto(Book book)
        {
            // Adjusted for multi Id - Return IDs for processing
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors ?? new List<string>(), // Adjusted for multi Id - Return IDs for processing
                Categories = book.Categories ?? new List<string>(), // Adjusted for multi Id - Return IDs for processing
                Description = book.Description,
                PublishYear = book.PublishYear,
                FileUrl = book.FileUrl,
                FileFormat = book.FileFormat,
                IsAvailable = book.IsAvailable,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }

        // Adjusted for multi Id - Convert BookDto to BookDisplayDto with author/category names
        public async Task<BookDisplayDto> MapToDisplayDtoAsync(BookDto bookDto)
        {
            var authorNames = new List<string>();
            var categoryNames = new List<string>();

            // Get author names from IDs
            if (bookDto.Authors != null && bookDto.Authors.Any())
            {
                foreach (var authorId in bookDto.Authors)
                {
                    var author = await _bookRepository.GetAuthorByIdAsync(authorId);
                    if (author != null)
                    {
                        authorNames.Add(author.Name);
                    }
                    else
                    {
                        // If author not found, use ID as fallback
                        authorNames.Add(authorId);
                    }
                }
            }

            // Get category names from IDs
            if (bookDto.Categories != null && bookDto.Categories.Any())
            {
                foreach (var categoryId in bookDto.Categories)
                {
                    var category = await _bookRepository.GetCategoryByIdAsync(categoryId);
                    if (category != null)
                    {
                        categoryNames.Add(category.Name);
                    }
                    else
                    {
                        // If category not found, use ID as fallback
                        categoryNames.Add(categoryId);
                    }
                }
            }

            return new BookDisplayDto
            {
                Id = bookDto.Id,
                Title = bookDto.Title,
                Authors = authorNames, // Adjusted for multi Id - Return names for display
                Categories = categoryNames, // Adjusted for multi Id - Return names for display
                Description = bookDto.Description,
                PublishYear = bookDto.PublishYear,
                FileUrl = bookDto.FileUrl,
                FileFormat = bookDto.FileFormat,
                IsAvailable = bookDto.IsAvailable,
                CreatedAt = bookDto.CreatedAt,
                UpdatedAt = bookDto.UpdatedAt
            };
        }

        // Adjusted for multi Id - Validate that all author IDs exist
        private async Task<ApiResponse<bool>> ValidateAuthorsExistAsync(List<string> authorIds)
        {
            try
            {
                foreach (var authorId in authorIds)
                {
                    if (!IsValidObjectId(authorId))
                    {
                        return ApiResponse<bool>.ErrorResponse($"Invalid author ID format: {authorId}");
                    }

                    var author = await _bookRepository.GetAuthorByIdAsync(authorId);
                    if (author == null)
                    {
                        return ApiResponse<bool>.ErrorResponse($"Author with ID {authorId} not found");
                    }
                }
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error validating authors: {ex.Message}");
            }
        }

        // Adjusted for multi Id - Validate that all category IDs exist
        private async Task<ApiResponse<bool>> ValidateCategoriesExistAsync(List<string> categoryIds)
        {
            try
            {
                foreach (var categoryId in categoryIds)
                {
                    if (!IsValidObjectId(categoryId))
                    {
                        return ApiResponse<bool>.ErrorResponse($"Invalid category ID format: {categoryId}");
                    }

                    var category = await _bookRepository.GetCategoryByIdAsync(categoryId);
                    if (category == null)
                    {
                        return ApiResponse<bool>.ErrorResponse($"Category with ID {categoryId} not found");
                    }
                }
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error validating categories: {ex.Message}");
            }
        }

        // Adjusted for multi Id - Helper method to validate ObjectId format
        private static bool IsValidObjectId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length != 24)
                return false;
            
            // Check if all characters are valid hex
            return id.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }

        // Adjusted for multi Id - Parse JSON string to List<string>
        private static List<string> ParseJsonStringToList(List<string> input)
        {
            if (input == null || !input.Any())
                return new List<string>();

            var result = new List<string>();
            foreach (var item in input)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                // Check if it's a JSON array string
                if (item.StartsWith("[") && item.EndsWith("]"))
                {
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<string[]>(item);
                        if (parsed != null)
                        {
                            result.AddRange(parsed.Where(x => !string.IsNullOrEmpty(x)));
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, treat as single value
                        result.Add(item);
                    }
                }
                else
                {
                    // Single value, add directly
                    result.Add(item);
                }
            }
            return result;
        }
    }
}
