using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryCreateDto dto)
        {
            try
            {
                if (await _categoryRepository.ExistsByNameAsync(dto.Name))
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category name already exists");
                }

                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description
                };

                var createdCategory = await _categoryRepository.CreateAsync(category);
                var categoryDto = MapToDto(createdCategory);

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Failed to create category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(string id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                var categoryDto = MapToDto(category);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByNameAsync(string name)
        {
            try
            {
                var category = await _categoryRepository.GetByNameAsync(name);
                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                var categoryDto = MapToDto(category);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync(int page = 1, int limit = 10)
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync(page, limit);
                var categoryDtos = categories.Select(MapToDto).ToList();
                return ApiResponse<List<CategoryDto>>.SuccessResponse(categoryDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CategoryDto>>.ErrorResponse($"Failed to get categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(string id, CategoryUpdateDto dto)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                if (!string.IsNullOrEmpty(dto.Name))
                {
                    if (await _categoryRepository.ExistsByNameAsync(dto.Name) && existingCategory.Name != dto.Name)
                    {
                        return ApiResponse<CategoryDto>.ErrorResponse("Category name already exists");
                    }
                    existingCategory.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                    existingCategory.Description = dto.Description;

                var updatedCategory = await _categoryRepository.UpdateAsync(id, existingCategory);
                if (updatedCategory == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Failed to update category");
                }

                var categoryDto = MapToDto(updatedCategory);
                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResponse($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(string id)
        {
            try
            {
                var result = await _categoryRepository.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Category not found");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to delete category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetCategoryCountAsync()
        {
            try
            {
                var count = await _categoryRepository.CountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get category count: {ex.Message}");
            }
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
