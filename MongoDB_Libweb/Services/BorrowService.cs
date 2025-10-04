using MongoDB_Libweb.DTOs;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.Repositories;

namespace MongoDB_Libweb.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly IBorrowRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;

        public BorrowService(IBorrowRepository borrowRepository, IBookRepository bookRepository, IUserRepository userRepository)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Mượn sách - tạo record borrow mới
        /// Thực hiện các validation cần thiết và cập nhật trạng thái sách
        /// </summary>
        /// <param name="dto">Thông tin mượn sách</param>
        /// <returns>ApiResponse chứa thông tin borrow record đã tạo</returns>
        public async Task<ApiResponse<BorrowDto>> BorrowBookAsync(BorrowCreateDto dto)
        {
            try
            {
                // Validation 1: Kiểm tra user có tồn tại và đang active không
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("User not found or inactive");
                }

                // Validation 2: Kiểm tra sách có tồn tại và đang available không
                var book = await _bookRepository.GetByIdAsync(dto.BookId);
                if (book == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Book not found");
                }

                if (!book.IsAvailable)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Book is not available");
                }

                // Validation 3: Kiểm tra user đã mượn sách này chưa (tránh duplicate)
                if (await _borrowRepository.HasActiveBorrowAsync(dto.UserId, dto.BookId))
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("User already has this book borrowed");
                }

                // Tạo borrow record mới
                var borrow = new Borrow
                {
                    UserId = dto.UserId,
                    BookId = dto.BookId,
                    BorrowDate = DateTime.UtcNow,
                    DueDate = dto.DueDate,
                    Status = "Borrowed"
                };

                var createdBorrow = await _borrowRepository.CreateAsync(borrow);

                // Cập nhật trạng thái sách thành không available
                await _bookRepository.SetAvailabilityAsync(dto.BookId, false);

                var borrowDto = MapToDto(createdBorrow);
                return ApiResponse<BorrowDto>.SuccessResponse(borrowDto, "Book borrowed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BorrowDto>.ErrorResponse($"Failed to borrow book: {ex.Message}");
            }
        }

        /// <summary>
        /// Trả sách - cập nhật borrow record và trạng thái sách
        /// </summary>
        /// <param name="dto">Thông tin trả sách (chứa BorrowId)</param>
        /// <returns>ApiResponse chứa thông tin borrow record đã cập nhật</returns>
        public async Task<ApiResponse<BorrowDto>> ReturnBookAsync(BorrowReturnDto dto)
        {
            try
            {
                // Validation 1: Kiểm tra borrow record có tồn tại không
                var borrow = await _borrowRepository.GetByIdAsync(dto.BorrowId);
                if (borrow == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Borrow record not found");
                }

                // Validation 2: Kiểm tra sách có đang được mượn không
                if (borrow.Status != "Borrowed")
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Book is not currently borrowed");
                }

                // Cập nhật borrow record với thông tin trả sách
                borrow.ReturnDate = DateTime.UtcNow;
                borrow.Status = "Returned";

                var updatedBorrow = await _borrowRepository.UpdateAsync(dto.BorrowId, borrow);
                if (updatedBorrow == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Failed to update borrow record");
                }

                // Cập nhật trạng thái sách thành available
                await _bookRepository.SetAvailabilityAsync(borrow.BookId, true);

                var borrowDto = MapToDto(updatedBorrow);
                return ApiResponse<BorrowDto>.SuccessResponse(borrowDto, "Book returned successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BorrowDto>.ErrorResponse($"Failed to return book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BorrowDto>> GetBorrowByIdAsync(string id)
        {
            try
            {
                var borrow = await _borrowRepository.GetByIdAsync(id);
                if (borrow == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Borrow record not found");
                }

                var borrowDto = MapToDto(borrow);
                return ApiResponse<BorrowDto>.SuccessResponse(borrowDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BorrowDto>.ErrorResponse($"Failed to get borrow record: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy borrow records theo user (phiên bản cũ - chỉ thông tin cơ bản)
        /// </summary>
        public async Task<ApiResponse<List<BorrowDto>>> GetBorrowsByUserAsync(string userId, int page = 1, int limit = 10)
        {
            try
            {
                var borrows = await _borrowRepository.GetByUserIdAsync(userId, page, limit);
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get user borrows: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy borrow records theo user với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu - chỉ 1 lần query database
        /// </summary>
        public async Task<ApiResponse<List<BorrowDetailDto>>> GetBorrowsByUserWithDetailsAsync(string userId, int page = 1, int limit = 10)
        {
            try
            {
                var borrowDetailDtos = await _borrowRepository.GetByUserIdWithDetailsAsync(userId, page, limit);
                return ApiResponse<List<BorrowDetailDto>>.SuccessResponse(borrowDetailDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDetailDto>>.ErrorResponse($"Failed to get user borrows with details: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy borrow records theo book (phiên bản cũ - chỉ thông tin cơ bản)
        /// </summary>
        public async Task<ApiResponse<List<BorrowDto>>> GetBorrowsByBookAsync(string bookId, int page = 1, int limit = 10)
        {
            try
            {
                var borrows = await _borrowRepository.GetByBookIdAsync(bookId, page, limit);
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get book borrows: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy borrow records theo book với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu - chỉ 1 lần query database
        /// </summary>
        public async Task<ApiResponse<List<BorrowDetailDto>>> GetBorrowsByBookWithDetailsAsync(string bookId, int page = 1, int limit = 10)
        {
            try
            {
                var borrowDetailDtos = await _borrowRepository.GetByBookIdWithDetailsAsync(bookId, page, limit);
                return ApiResponse<List<BorrowDetailDto>>.SuccessResponse(borrowDetailDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDetailDto>>.ErrorResponse($"Failed to get book borrows with details: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy borrow records theo status (phiên bản cũ - chỉ thông tin cơ bản)
        /// </summary>
        public async Task<ApiResponse<List<BorrowDto>>> GetBorrowsByStatusAsync(string status, int page = 1, int limit = 10)
        {
            try
            {
                var borrows = await _borrowRepository.GetByStatusAsync(status, page, limit);
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get borrows by status: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy borrow records theo status với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu - chỉ 1 lần query database
        /// </summary>
        public async Task<ApiResponse<List<BorrowDetailDto>>> GetBorrowsByStatusWithDetailsAsync(string status, int page = 1, int limit = 10)
        {
            try
            {
                var borrowDetailDtos = await _borrowRepository.GetByStatusWithDetailsAsync(status, page, limit);
                return ApiResponse<List<BorrowDetailDto>>.SuccessResponse(borrowDetailDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDetailDto>>.ErrorResponse($"Failed to get borrows by status with details: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BorrowDto>>> GetOverdueBorrowsAsync()
        {
            try
            {
                var borrows = await _borrowRepository.GetOverdueBorrowsAsync();
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get overdue borrows: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetBorrowCountAsync()
        {
            try
            {
                var count = await _borrowRepository.CountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get borrow count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CanUserBorrowBookAsync(string userId, string bookId)
        {
            try
            {
                var canBorrow = !await _borrowRepository.HasActiveBorrowAsync(userId, bookId);
                return ApiResponse<bool>.SuccessResponse(canBorrow);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Failed to check borrow eligibility: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BorrowDto>>> SearchBorrowsAsync(BorrowSearchDto searchDto)
        {
            try
            {
                // This is a simplified search - in a real implementation, you'd need more complex filtering
                var borrows = await _borrowRepository.GetAllAsync(searchDto.Page, searchDto.Limit);
                
                // Apply filters
                if (!string.IsNullOrEmpty(searchDto.UserId))
                    borrows = borrows.Where(b => b.UserId == searchDto.UserId).ToList();

                if (!string.IsNullOrEmpty(searchDto.BookId))
                    borrows = borrows.Where(b => b.BookId == searchDto.BookId).ToList();

                if (!string.IsNullOrEmpty(searchDto.Status))
                    borrows = borrows.Where(b => b.Status == searchDto.Status).ToList();

                if (searchDto.FromDate.HasValue)
                    borrows = borrows.Where(b => b.BorrowDate >= searchDto.FromDate.Value).ToList();

                if (searchDto.ToDate.HasValue)
                    borrows = borrows.Where(b => b.BorrowDate <= searchDto.ToDate.Value).ToList();

                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to search borrows: {ex.Message}");
            }
        }

        /// <summary>
        /// Tìm kiếm borrow records với thông tin chi tiết User và Book
        /// Sử dụng MongoDB aggregation pipeline tối ưu - chỉ 1 lần query database
        /// </summary>
        /// <param name="searchDto">Điều kiện tìm kiếm</param>
        /// <returns>ApiResponse chứa danh sách BorrowDetailDto với thông tin đầy đủ</returns>
        public async Task<ApiResponse<List<BorrowDetailDto>>> SearchBorrowsWithDetailsAsync(BorrowSearchDto searchDto)
        {
            try
            {
                var borrowDetailDtos = await _borrowRepository.SearchBorrowsWithDetailsAsync(searchDto);
                return ApiResponse<List<BorrowDetailDto>>.SuccessResponse(borrowDetailDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDetailDto>>.ErrorResponse($"Failed to search borrows with details: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BorrowDto>>> GetBorrowsByUserIdAsync(string userId, int page = 1, int limit = 10)
        {
            try
            {
                var borrows = await _borrowRepository.GetByUserIdAsync(userId, page, limit);
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get borrows by user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BorrowDto>>> GetAllBorrowsAsync(int page = 1, int limit = 10)
        {
            try
            {
                var borrows = await _borrowRepository.GetAllAsync(page, limit);
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get all borrows: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tất cả borrow records với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu - chỉ 1 lần query database
        /// </summary>
        /// <param name="page">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="limit">Số lượng records mỗi trang</param>
        /// <returns>ApiResponse chứa danh sách BorrowDetailDto</returns>
        public async Task<ApiResponse<List<BorrowDetailDto>>> GetAllBorrowsWithDetailsAsync(int page = 1, int limit = 10)
        {
            try
            {
                // Sử dụng aggregation pipeline để lấy dữ liệu trong 1 lần query
                // Thay vì N+1 queries (1 query cho borrows + N queries cho users + N queries cho books)
                var borrowDetailDtos = await _borrowRepository.GetAllWithDetailsAsync(page, limit);

                return ApiResponse<List<BorrowDetailDto>>.SuccessResponse(borrowDetailDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDetailDto>>.ErrorResponse($"Failed to get all borrows with details: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetActiveBorrowsCountAsync()
        {
            try
            {
                var count = await _borrowRepository.GetActiveBorrowsCountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get active borrows count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<long>> GetOverdueBorrowsCountAsync()
        {
            try
            {
                var count = await _borrowRepository.GetOverdueBorrowsCountAsync();
                return ApiResponse<long>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<long>.ErrorResponse($"Failed to get overdue borrows count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BorrowDto>>> GetBorrowsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var borrows = await _borrowRepository.GetBorrowsByDateRangeAsync(startDate, endDate);
                var borrowDtos = borrows.Select(MapToDto).ToList();
                return ApiResponse<List<BorrowDto>>.SuccessResponse(borrowDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BorrowDto>>.ErrorResponse($"Failed to get borrows by date range: {ex.Message}");
            }
        }

        private static BorrowDto MapToDto(Borrow borrow)
        {
            return new BorrowDto
            {
                Id = borrow.Id,
                UserId = borrow.UserId,
                BookId = borrow.BookId,
                BorrowDate = borrow.BorrowDate,
                DueDate = borrow.DueDate,
                ReturnDate = borrow.ReturnDate,
                Status = borrow.Status
            };
        }

        private static UserDto MapUserToDto(User user)
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

        private static BookDto MapBookToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Authors = book.Authors,
                Categories = book.Categories,
                Description = book.Description,
                PublishYear = book.PublishYear,
                FileUrl = book.FileUrl,
                FileFormat = book.FileFormat,
                IsAvailable = book.IsAvailable,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }
    }
}
