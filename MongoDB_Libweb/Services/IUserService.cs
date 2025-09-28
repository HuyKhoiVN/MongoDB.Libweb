using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Services
{
    public interface IUserService
    {
        Task<ApiResponse<UserDto>> RegisterUserAsync(UserCreateDto dto);
        Task<ApiResponse<UserDto>> LoginAsync(UserLoginDto dto);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string id);
        Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username);
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(int page = 1, int limit = 10);
        Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UserUpdateDto dto);
        Task<ApiResponse<bool>> DeleteUserAsync(string id);
        Task<ApiResponse<long>> GetUserCountAsync();
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
