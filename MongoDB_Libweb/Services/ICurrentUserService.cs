using MongoDB_Libweb.DTOs;

namespace MongoDB_Libweb.Services
{
    public interface ICurrentUserService
    {
        Task<ApiResponse<UserDto>> GetCurrentUserAsync();
        string? GetCurrentUserId();
        string? GetCurrentUsername();
        bool IsAuthenticated();
    }
}
