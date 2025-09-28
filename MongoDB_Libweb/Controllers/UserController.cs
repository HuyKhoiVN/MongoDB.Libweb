using Microsoft.AspNetCore.Mvc;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Services;

namespace MongoDB_Libweb.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] UserCreateDto dto)
        {
            var result = await _userService.RegisterUserAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetUserById), new { id = result.Data!.Id }, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Login([FromBody] UserLoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(string id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByUsername(string username)
        {
            var result = await _userService.GetUserByUsernameAsync(username);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _userService.GetAllUsersAsync(page, limit);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            var result = await _userService.UpdateUserAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<long>>> GetUserCount()
        {
            var result = await _userService.GetUserCountAsync();
            return Ok(result);
        }

        [HttpPost("{id}/change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            var result = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
