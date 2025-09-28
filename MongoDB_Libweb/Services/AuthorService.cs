using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public async Task<ApiResponse<AuthorDto>> CreateAuthorAsync(AuthorCreateDto dto)
        {
            try
            {
                if (await _authorRepository.ExistsByNameAsync(dto.Name))
                {
                    return ApiResponse<AuthorDto>.ErrorResponse("Author name already exists");
                }

                var author = new Author
                {
                    Name = dto.Name,
                    Bio = dto.Bio
                };

                var createdAuthor = await _authorRepository.CreateAsync(author);
                var authorDto = MapToDto(createdAuthor);

                return ApiResponse<AuthorDto>.SuccessResponse(authorDto, "Author created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthorDto>.ErrorResponse($"Failed to create author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthorDto>> GetAuthorByIdAsync(string id)
        {
            try
            {
                var author = await _authorRepository.GetByIdAsync(id);
                if (author == null)
                {
                    return ApiResponse<AuthorDto>.ErrorResponse("Author not found");
                }

                var authorDto = MapToDto(author);
                return ApiResponse<AuthorDto>.SuccessResponse(authorDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthorDto>.ErrorResponse($"Failed to get author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthorDto>> GetAuthorByNameAsync(string name)
        {
            try
            {
                var author = await _authorRepository.GetByNameAsync(name);
                if (author == null)
                {
                    return ApiResponse<AuthorDto>.ErrorResponse("Author not found");
                }

                var authorDto = MapToDto(author);
                return ApiResponse<AuthorDto>.SuccessResponse(authorDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthorDto>.ErrorResponse($"Failed to get author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<AuthorDto>>> GetAllAuthorsAsync(int page = 1, int limit = 10)
        {
            try
            {
                var authors = await _authorRepository.GetAllAsync(page, limit);
                var authorDtos = authors.Select(MapToDto).ToList();
                return ApiResponse<List<AuthorDto>>.SuccessResponse(authorDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AuthorDto>>.ErrorResponse($"Failed to get authors: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthorDto>> UpdateAuthorAsync(string id, AuthorUpdateDto dto)
        {
            try
            {
                var existingAuthor = await _authorRepository.GetByIdAsync(id);
                if (existingAuthor == null)
                {
                    return ApiResponse<AuthorDto>.ErrorResponse("Author not found");
                }

                if (!string.IsNullOrEmpty(dto.Name))
                {
                    if (await _authorRepository.ExistsByNameAsync(dto.Name) && existingAuthor.Name != dto.Name)
                    {
                        return ApiResponse<AuthorDto>.ErrorResponse("Author name already exists");
                    }
                    existingAuthor.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.Bio))
                    existingAuthor.Bio = dto.Bio;

                var updatedAuthor = await _authorRepository.UpdateAsync(id, existingAuthor);
                if (updatedAuthor == null)
                {
                    return ApiResponse<AuthorDto>.ErrorResponse("Failed to update author");
                }

                var authorDto = MapToDto(updatedAuthor);
                return ApiResponse<AuthorDto>.SuccessResponse(authorDto, "Author updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthorDto>.ErrorResponse($"Failed to update author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAuthorAsync(string id)
        {
            try
            {
                var result = await _authorRepository.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Author not found");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Author deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to delete author: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetAuthorCountAsync()
        {
            try
            {
                var count = await _authorRepository.CountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get author count: {ex.Message}");
            }
        }

        private static AuthorDto MapToDto(Author author)
        {
            return new AuthorDto
            {
                Id = author.Id,
                Name = author.Name,
                Bio = author.Bio,
                CreatedAt = author.CreatedAt
            };
        }
    }
}
