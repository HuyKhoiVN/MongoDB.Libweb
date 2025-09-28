using BCrypt.Net;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<UserDto>> RegisterUserAsync(UserCreateDto dto)
        {
            try
            {
                // Check if username already exists
                if (await _userRepository.ExistsByUsernameAsync(dto.Username))
                {
                    return ApiResponse<UserDto>.ErrorResponse("Username already exists");
                }

                // Check if email already exists
                if (await _userRepository.ExistsByEmailAsync(dto.Email))
                {
                    return ApiResponse<UserDto>.ErrorResponse("Email already exists");
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                // Create user
                var user = new User
                {
                    Username = dto.Username,
                    PasswordHash = passwordHash,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    Role = dto.Role,
                    IsActive = true
                };

                var createdUser = await _userRepository.CreateAsync(user);
                var userDto = MapToDto(createdUser);

                return ApiResponse<UserDto>.SuccessResponse(userDto, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> LoginAsync(UserLoginDto dto)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(dto.Username);
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Invalid username or password");
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    return ApiResponse<UserDto>.ErrorResponse("Invalid username or password");
                }

                var userDto = MapToDto(user);
                return ApiResponse<UserDto>.SuccessResponse(userDto, "Login successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var userDto = MapToDto(user);
                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Failed to get user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var userDto = MapToDto(user);
                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Failed to get user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(int page = 1, int limit = 10)
        {
            try
            {
                var users = await _userRepository.GetAllAsync(page, limit);
                var userDtos = users.Select(MapToDto).ToList();
                return ApiResponse<List<UserDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserDto>>.ErrorResponse($"Failed to get users: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UserUpdateDto dto)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(dto.FullName))
                    existingUser.FullName = dto.FullName;

                if (!string.IsNullOrEmpty(dto.Email))
                {
                    if (await _userRepository.ExistsByEmailAsync(dto.Email) && existingUser.Email != dto.Email)
                    {
                        return ApiResponse<UserDto>.ErrorResponse("Email already exists");
                    }
                    existingUser.Email = dto.Email;
                }

                if (dto.IsActive.HasValue)
                    existingUser.IsActive = dto.IsActive.Value;

                var updatedUser = await _userRepository.UpdateAsync(id, existingUser);
                if (updatedUser == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Failed to update user");
                }

                var userDto = MapToDto(updatedUser);
                return ApiResponse<UserDto>.SuccessResponse(userDto, "User updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Failed to update user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
        {
            try
            {
                var result = await _userRepository.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to delete user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetUserCountAsync()
        {
            try
            {
                var count = await _userRepository.CountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get user count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    return ApiResponse<bool>.ErrorResponse("Current password is incorrect");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _userRepository.UpdateAsync(userId, user);

                return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to change password: {ex.Message}");
            }
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
