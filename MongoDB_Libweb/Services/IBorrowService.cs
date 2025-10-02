using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Services
{
    public interface IBorrowService
    {
        Task<ApiResponse<BorrowDto>> BorrowBookAsync(BorrowCreateDto dto);
        Task<ApiResponse<BorrowDto>> ReturnBookAsync(BorrowReturnDto dto);
        Task<ApiResponse<BorrowDto>> GetBorrowByIdAsync(string id);
        Task<ApiResponse<List<BorrowDto>>> GetBorrowsByUserAsync(string userId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDto>>> GetBorrowsByUserIdAsync(string userId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDto>>> GetBorrowsByBookAsync(string bookId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDto>>> GetBorrowsByStatusAsync(string status, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDto>>> GetAllBorrowsAsync(int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDetailDto>>> GetAllBorrowsWithDetailsAsync(int page = 1, int limit = 10);
        
        // Methods tối ưu với aggregation pipeline - chỉ 1 lần query database
        Task<ApiResponse<List<BorrowDetailDto>>> GetBorrowsByUserWithDetailsAsync(string userId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDetailDto>>> GetBorrowsByBookWithDetailsAsync(string bookId, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDetailDto>>> GetBorrowsByStatusWithDetailsAsync(string status, int page = 1, int limit = 10);
        Task<ApiResponse<List<BorrowDto>>> GetOverdueBorrowsAsync();
        Task<ApiResponse<long>> GetBorrowCountAsync();
        Task<ApiResponse<long>> GetActiveBorrowsCountAsync();
        Task<ApiResponse<long>> GetOverdueBorrowsCountAsync();
        Task<ApiResponse<List<BorrowDto>>> GetBorrowsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<bool>> CanUserBorrowBookAsync(string userId, string bookId);
        Task<ApiResponse<List<BorrowDto>>> SearchBorrowsAsync(BorrowSearchDto searchDto);
    }
}
