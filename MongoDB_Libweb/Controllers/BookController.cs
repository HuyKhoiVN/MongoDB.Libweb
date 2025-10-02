using Microsoft.AspNetCore.Mvc;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Services;

namespace MongoDB_Libweb.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BookDto>>> CreateBook([FromBody] BookCreateDto dto)
        {
            var result = await _bookService.CreateBookAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetBookById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BookDto>>> GetBookById(string id)
        {
            var result = await _bookService.GetBookByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetAllBooks([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _bookService.GetAllBooksAsync(page, limit);
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<List<BookDto>>>> SearchBooks([FromBody] BookSearchDto searchDto)
        {
            var result = await _bookService.SearchBooksAsync(searchDto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<BookDto>>> UpdateBook(string id, [FromBody] BookUpdateDto dto)
        {
            var result = await _bookService.UpdateBookAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBook(string id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<long>>> GetBookCount()
        {
            var result = await _bookService.GetBookCountAsync();
            return Ok(result);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetBooksByCategory(string categoryId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _bookService.GetBooksByCategoryAsync(categoryId, page, limit);
            return Ok(result);
        }

        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetBooksByAuthor(string authorId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _bookService.GetBooksByAuthorAsync(authorId, page, limit);
            return Ok(result);
        }

        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetFeaturedBooks([FromQuery] int limit = 6)
        {
            var result = await _bookService.GetFeaturedBooksAsync(limit);
            return Ok(result);
        }

        [HttpPut("{id}/availability")]
        public async Task<ActionResult<ApiResponse<bool>>> SetBookAvailability(string id, [FromBody] SetAvailabilityRequest request)
        {
            var result = await _bookService.SetBookAvailabilityAsync(id, request.IsAvailable);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    public class SetAvailabilityRequest
    {
        public bool IsAvailable { get; set; }
    }
}
