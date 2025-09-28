using Microsoft.AspNetCore.Mvc;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Services;

namespace MongoDB_Libweb.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> CreateAuthor([FromBody] AuthorCreateDto dto)
        {
            var result = await _authorService.CreateAuthorAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetAuthorById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> GetAuthorById(string id)
        {
            var result = await _authorService.GetAuthorByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> GetAuthorByName(string name)
        {
            var result = await _authorService.GetAuthorByNameAsync(name);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AuthorDto>>>> GetAllAuthors([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _authorService.GetAllAuthorsAsync(page, limit);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AuthorDto>>> UpdateAuthor(string id, [FromBody] AuthorUpdateDto dto)
        {
            var result = await _authorService.UpdateAuthorAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAuthor(string id)
        {
            var result = await _authorService.DeleteAuthorAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<long>>> GetAuthorCount()
        {
            var result = await _authorService.GetAuthorCountAsync();
            return Ok(result);
        }
    }
}
