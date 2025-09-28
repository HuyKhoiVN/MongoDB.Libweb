using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Services
{
    public interface IAuthorService
    {
        Task<ApiResponse<AuthorDto>> CreateAuthorAsync(AuthorCreateDto dto);
        Task<ApiResponse<AuthorDto>> GetAuthorByIdAsync(string id);
        Task<ApiResponse<AuthorDto>> GetAuthorByNameAsync(string name);
        Task<ApiResponse<List<AuthorDto>>> GetAllAuthorsAsync(int page = 1, int limit = 10);
        Task<ApiResponse<AuthorDto>> UpdateAuthorAsync(string id, AuthorUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAuthorAsync(string id);
        Task<ApiResponse<long>> GetAuthorCountAsync();
    }
}
