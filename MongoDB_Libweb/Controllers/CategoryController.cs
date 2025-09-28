using Microsoft.AspNetCore.Mvc;
using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Services;

namespace MongoDB_Libweb.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryById(string id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryByName(string name)
        {
            var result = await _categoryService.GetCategoryByNameAsync(name);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAllCategories([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await _categoryService.GetAllCategoriesAsync(page, limit);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(string id, [FromBody] CategoryUpdateDto dto)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(string id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<long>>> GetCategoryCount()
        {
            var result = await _categoryService.GetCategoryCountAsync();
            return Ok(result);
        }
    }
}
