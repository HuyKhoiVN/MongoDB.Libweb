using Microsoft.AspNetCore.Mvc;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Services;

namespace MongoDB_Libweb.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        [HttpPost("borrow")]
        public async Task<ActionResult<ApiResponse<BorrowDto>>> BorrowBook([FromBody] BorrowCreateDto dto)
        {
            var result = await _borrowService.BorrowBookAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetBorrowById), new { id = result.Data!.Id }, result);
        }

        [HttpPost("return")]
        public async Task<ActionResult<ApiResponse<BorrowDto>>> ReturnBook([FromBody] BorrowReturnDto dto)
        {
            var result = await _borrowService.ReturnBookAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BorrowDto>>> GetBorrowById(string id)
        {
            var result = await _borrowService.GetBorrowByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<BorrowDetailDto>>>> GetBorrowsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _borrowService.GetBorrowsByUserWithDetailsAsync(userId, page, limit);
            return Ok(result);
        }

        [HttpGet("user/{userId}/borrows")]
        public async Task<ActionResult<ApiResponse<List<BorrowDto>>>> GetBorrowsByUserId(string userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _borrowService.GetBorrowsByUserIdAsync(userId, page, limit);
            return Ok(result);
        }

        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<ApiResponse<List<BorrowDetailDto>>>> GetBorrowsByBook(string bookId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _borrowService.GetBorrowsByBookWithDetailsAsync(bookId, page, limit);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<BorrowDetailDto>>>> GetBorrowsByStatus(string status, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _borrowService.GetBorrowsByStatusWithDetailsAsync(status, page, limit);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<BorrowDetailDto>>>> GetAllBorrows([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _borrowService.GetAllBorrowsWithDetailsAsync(page, limit);
            return Ok(result);
        }

        [HttpGet("all/basic")]
        public async Task<ActionResult<ApiResponse<List<BorrowDto>>>> GetAllBorrowsBasic([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _borrowService.GetAllBorrowsAsync(page, limit);
            return Ok(result);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<ApiResponse<List<BorrowDto>>>> GetOverdueBorrows()
        {
            var result = await _borrowService.GetOverdueBorrowsAsync();
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<long>>> GetBorrowCount()
        {
            var result = await _borrowService.GetBorrowCountAsync();
            return Ok(result);
        }

        [HttpGet("count/active")]
        public async Task<ActionResult<ApiResponse<long>>> GetActiveBorrowsCount()
        {
            var result = await _borrowService.GetActiveBorrowsCountAsync();
            return Ok(result);
        }

        [HttpGet("count/overdue")]
        public async Task<ActionResult<ApiResponse<long>>> GetOverdueBorrowsCount()
        {
            var result = await _borrowService.GetOverdueBorrowsCountAsync();
            return Ok(result);
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<ApiResponse<List<BorrowDto>>>> GetBorrowsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _borrowService.GetBorrowsByDateRangeAsync(startDate, endDate);
            return Ok(result);
        }

        [HttpGet("can-borrow")]
        public async Task<ActionResult<ApiResponse<bool>>> CanUserBorrowBook([FromQuery] string userId, [FromQuery] string bookId)
        {
            var result = await _borrowService.CanUserBorrowBookAsync(userId, bookId);
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<List<BorrowDto>>>> SearchBorrows([FromBody] BorrowSearchDto searchDto)
        {
            var result = await _borrowService.SearchBorrowsWithDetailsAsync(searchDto);
            return Ok(result);
        }

        [HttpPost("search-origin")]
        public async Task<ActionResult<ApiResponse<List<BorrowDetailDto>>>> SearchBorrowsOrigin([FromBody] BorrowSearchDto searchDto)
        {
            var result = await _borrowService.SearchBorrowsAsync(searchDto);
            return Ok(result);
        }
    }
}
