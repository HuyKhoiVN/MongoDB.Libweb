using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileUploadRepository _fileUploadRepository;

        public FileUploadService(IFileUploadRepository fileUploadRepository)
        {
            _fileUploadRepository = fileUploadRepository;
        }

        public async Task<ApiResponse<FileUploadDto>> CreateFileUploadAsync(FileUploadCreateDto dto)
        {
            try
            {
                var fileUpload = new FileUpload
                {
                    Filename = dto.Filename,
                    Length = dto.Length,
                    BookId = dto.BookId
                };

                var createdFileUpload = await _fileUploadRepository.CreateAsync(fileUpload);
                var fileUploadDto = MapToDto(createdFileUpload);

                return ApiResponse<FileUploadDto>.SuccessResponse(fileUploadDto, "File upload created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<FileUploadDto>.ErrorResponse($"Failed to create file upload: {ex.Message}");
            }
        }

        public async Task<ApiResponse<FileUploadDto>> GetFileUploadByIdAsync(string id)
        {
            try
            {
                var fileUpload = await _fileUploadRepository.GetByIdAsync(id);
                if (fileUpload == null)
                {
                    return ApiResponse<FileUploadDto>.ErrorResponse("File upload not found");
                }

                var fileUploadDto = MapToDto(fileUpload);
                return ApiResponse<FileUploadDto>.SuccessResponse(fileUploadDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<FileUploadDto>.ErrorResponse($"Failed to get file upload: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<FileUploadDto>>> GetFileUploadsByBookIdAsync(string bookId)
        {
            try
            {
                var fileUploads = await _fileUploadRepository.GetByBookIdAsync(bookId);
                var fileUploadDtos = fileUploads.Select(MapToDto).ToList();
                return ApiResponse<List<FileUploadDto>>.SuccessResponse(fileUploadDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FileUploadDto>>.ErrorResponse($"Failed to get file uploads by book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<FileUploadDto>>> GetAllFileUploadsAsync(int page = 1, int limit = 10)
        {
            try
            {
                var fileUploads = await _fileUploadRepository.GetAllAsync(page, limit);
                var fileUploadDtos = fileUploads.Select(MapToDto).ToList();
                return ApiResponse<List<FileUploadDto>>.SuccessResponse(fileUploadDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FileUploadDto>>.ErrorResponse($"Failed to get file uploads: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileUploadAsync(string id)
        {
            try
            {
                var result = await _fileUploadRepository.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("File upload not found");
                }

                return ApiResponse<bool>.SuccessResponse(true, "File upload deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to delete file upload: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetFileUploadCountAsync()
        {
            try
            {
                var count = await _fileUploadRepository.CountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get file upload count: {ex.Message}");
            }
        }

        private static FileUploadDto MapToDto(FileUpload fileUpload)
        {
            return new FileUploadDto
            {
                Id = fileUpload.Id,
                Filename = fileUpload.Filename,
                Length = fileUpload.Length,
                UploadDate = fileUpload.UploadDate,
                BookId = fileUpload.BookId
            };
        }
    }
}
