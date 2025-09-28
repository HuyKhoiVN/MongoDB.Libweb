using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Services
{
    public interface IFileUploadService
    {
        Task<ApiResponse<FileUploadDto>> CreateFileUploadAsync(FileUploadCreateDto dto);
        Task<ApiResponse<FileUploadDto>> GetFileUploadByIdAsync(string id);
        Task<ApiResponse<List<FileUploadDto>>> GetFileUploadsByBookIdAsync(string bookId);
        Task<ApiResponse<List<FileUploadDto>>> GetAllFileUploadsAsync(int page = 1, int limit = 10);
        Task<ApiResponse<bool>> DeleteFileUploadAsync(string id);
        Task<ApiResponse<long>> GetFileUploadCountAsync();
    }
}
