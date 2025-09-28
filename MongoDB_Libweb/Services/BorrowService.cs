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

        public async Task<ApiResponse<BorrowDto>> BorrowBookAsync(BorrowCreateDto dto)
        {
            try
            {
                // Check if user exists
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("User not found or inactive");
                }

                // Check if book exists and is available
                var book = await _bookRepository.GetByIdAsync(dto.BookId);
                if (book == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Book not found");
                }

                if (!book.IsAvailable)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Book is not available");
                }

                // Check if user already has this book borrowed
                if (await _borrowRepository.HasActiveBorrowAsync(dto.UserId, dto.BookId))
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("User already has this book borrowed");
                }

                // Create borrow record
                var borrow = new Borrow
                {
                    UserId = dto.UserId,
                    BookId = dto.BookId,
                    BorrowDate = DateTime.UtcNow,
                    DueDate = dto.DueDate,
                    Status = "Borrowed"
                };

                var createdBorrow = await _borrowRepository.CreateAsync(borrow);

                // Update book availability
                await _bookRepository.SetAvailabilityAsync(dto.BookId, false);

                var borrowDto = MapToDto(createdBorrow);
                return ApiResponse<BorrowDto>.SuccessResponse(borrowDto, "Book borrowed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BorrowDto>.ErrorResponse($"Failed to borrow book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BorrowDto>> ReturnBookAsync(BorrowReturnDto dto)
        {
            try
            {
                var borrow = await _borrowRepository.GetByIdAsync(dto.BorrowId);
                if (borrow == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Borrow record not found");
                }

                if (borrow.Status != "Borrowed")
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Book is not currently borrowed");
                }

                // Update borrow record
                borrow.ReturnDate = DateTime.UtcNow;
                borrow.Status = "Returned";

                var updatedBorrow = await _borrowRepository.UpdateAsync(dto.BorrowId, borrow);
                if (updatedBorrow == null)
                {
                    return ApiResponse<BorrowDto>.ErrorResponse("Failed to update borrow record");
                }

                // Update book availability
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
    }
}
