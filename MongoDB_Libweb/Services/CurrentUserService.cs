using MongoDB_Libweb.DTOs;
using System.Security.Claims;

namespace MongoDB_Libweb.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<UserDto>> GetCurrentUserAsync()
        {
            try
            {
                var username = GetCurrentUsername();
                if (string.IsNullOrEmpty(username))
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not authenticated");
                }

                return await _userService.GetUserByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Failed to get current user: {ex.Message}");
            }
        }

        public string? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }

        public string? GetCurrentUsername()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.Identity.Name;
            }
            return null;
        }

        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
