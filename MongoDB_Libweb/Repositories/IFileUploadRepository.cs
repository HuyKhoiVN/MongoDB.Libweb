using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public interface IFileUploadRepository
    {
        Task<List<FileUpload>> GetAllAsync(int page = 1, int limit = 10);
        Task<FileUpload?> GetByIdAsync(string id);
        Task<List<FileUpload>> GetByBookIdAsync(string bookId);
        Task<FileUpload> CreateAsync(FileUpload fileUpload);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<bool> ExistsByBookIdAsync(string bookId);
    }
}
