using Microsoft.AspNetCore.Mvc;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Services;

namespace MongoDB_Libweb.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<FileUploadDto>>> CreateFileUpload([FromBody] FileUploadCreateDto dto)
        {
            var result = await _fileUploadService.CreateFileUploadAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetFileUploadById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FileUploadDto>>> GetFileUploadById(string id)
        {
            var result = await _fileUploadService.GetFileUploadByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<ApiResponse<List<FileUploadDto>>>> GetFileUploadsByBookId(string bookId)
        {
            var result = await _fileUploadService.GetFileUploadsByBookIdAsync(bookId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FileUploadDto>>>> GetAllFileUploads([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _fileUploadService.GetAllFileUploadsAsync(page, limit);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFileUpload(string id)
        {
            var result = await _fileUploadService.DeleteFileUploadAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<long>>> GetFileUploadCount()
        {
            var result = await _fileUploadService.GetFileUploadCountAsync();
            return Ok(result);
        }
    }
}
