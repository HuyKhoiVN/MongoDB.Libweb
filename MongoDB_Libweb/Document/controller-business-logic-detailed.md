# Chi tiết Nghiệp vụ Controller - MongoDB Library Web API

## Tổng quan
Tài liệu này mô tả chi tiết nghiệp vụ của từng function trong các Controller của hệ thống thư viện MongoDB. Mỗi controller được thiết kế theo pattern API RESTful với các endpoint chuẩn.

## Cấu trúc Response API

Tất cả các API endpoint đều sử dụng class `ApiResponse<T>` để chuẩn hóa format trả về:

### ApiResponse<T> Structure
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }        // Trạng thái thành công/thất bại
    public T? Data { get; set; }             // Dữ liệu trả về (generic type)
    public string? Message { get; set; }     // Thông báo thành công
    public string? Error { get; set; }       // Thông báo lỗi
}
```

### Response Types

#### 1. Success Response (200 OK)
```json
{
    "success": true,
    "data": {
        // Dữ liệu thực tế (AuthorDto, BookDto, etc.)
    },
    "message": "Operation completed successfully",
    "error": null
}
```

#### 2. Error Response (400/404/500)
```json
{
    "success": false,
    "data": null,
    "message": "Additional error context",
    "error": "Detailed error message"
}
```

#### 3. List Response (200 OK)
```json
{
    "success": true,
    "data": [
        // Array of objects
    ],
    "message": null,
    "error": null
}
```

### HTTP Status Codes Mapping
- **200 OK**: Thành công (GET, PUT, DELETE)
- **201 Created**: Tạo mới thành công (POST)
- **400 Bad Request**: Dữ liệu đầu vào không hợp lệ
- **401 Unauthorized**: Xác thực thất bại
- **404 Not Found**: Không tìm thấy resource
- **500 Internal Server Error**: Lỗi server

---

## 1. AuthorController

### 1.1 POST /api/v1/author
**Chức năng**: Tạo mới tác giả
**Nghiệp vụ**:
- Kiểm tra tên tác giả đã tồn tại chưa (duplicate check)
- Tạo đối tượng Author với thông tin từ DTO
- Lưu vào database thông qua repository
- Trả về thông tin tác giả vừa tạo

**Validation**:
- Tên tác giả không được trùng lặp
- Tên tác giả và bio là bắt buộc

**Response**: 
- **201 Created** - ApiResponse<AuthorDto>
```json
{
    "success": true,
    "data": {
        "id": "author_id",
        "name": "Author Name",
        "bio": "Author Biography",
        "createdAt": "2024-01-01T00:00:00Z"
    },
    "message": "Author created successfully",
    "error": null
}
```

- **400 Bad Request** - ApiResponse<AuthorDto> (validation error)
```json
{
    "success": false,
    "data": null,
    "message": null,
    "error": "Author name already exists"
}
```

### 1.2 GET /api/v1/author/{id}
**Chức năng**: Lấy thông tin tác giả theo ID
**Nghiệp vụ**:
- Tìm kiếm tác giả trong database theo ID
- Trả về thông tin chi tiết nếu tìm thấy
- Trả về lỗi 404 nếu không tìm thấy

**Response**: 
- **200 OK** - ApiResponse<AuthorDto> (thành công)
```json
{
    "success": true,
    "data": {
        "id": "author_id",
        "name": "Author Name",
        "bio": "Author Biography",
        "createdAt": "2024-01-01T00:00:00Z"
    },
    "message": null,
    "error": null
}
```

- **404 Not Found** - ApiResponse<AuthorDto> (không tìm thấy)
```json
{
    "success": false,
    "data": null,
    "message": null,
    "error": "Author not found"
}
```

### 1.3 GET /api/v1/author/name/{name}
**Chức năng**: Lấy thông tin tác giả theo tên
**Nghiệp vụ**:
- Tìm kiếm tác giả trong database theo tên
- Trả về thông tin chi tiết nếu tìm thấy
- Trả về lỗi 404 nếu không tìm thấy

**Response**: 
- **200 OK** - ApiResponse<AuthorDto> (thành công)
- **404 Not Found** - ApiResponse<AuthorDto> (không tìm thấy)

### 1.4 GET /api/v1/author
**Chức năng**: Lấy danh sách tất cả tác giả (có phân trang)
**Nghiệp vụ**:
- Lấy danh sách tác giả với phân trang (page, limit)
- Mặc định page=1, limit=10
- Trả về danh sách tác giả đã được map sang DTO

**Parameters**:
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<AuthorDto>>
```json
{
    "success": true,
    "data": [
        {
            "id": "author_id_1",
            "name": "Author 1",
            "bio": "Bio 1",
            "createdAt": "2024-01-01T00:00:00Z"
        },
        {
            "id": "author_id_2",
            "name": "Author 2",
            "bio": "Bio 2",
            "createdAt": "2024-01-02T00:00:00Z"
        }
    ],
    "message": null,
    "error": null
}
```

### 1.5 PUT /api/v1/author/{id}
**Chức năng**: Cập nhật thông tin tác giả
**Nghiệp vụ**:
- Kiểm tra tác giả có tồn tại không
- Kiểm tra tên mới không trùng với tác giả khác (nếu có thay đổi tên)
- Cập nhật các trường được cung cấp
- Lưu thay đổi vào database

**Validation**:
- Tác giả phải tồn tại
- Tên mới không được trùng với tác giả khác

**Response**: 
- **200 OK** - ApiResponse<AuthorDto> (cập nhật thành công)
- **400 Bad Request** - ApiResponse<AuthorDto> (validation error)
- **404 Not Found** - ApiResponse<AuthorDto> (không tìm thấy tác giả)

### 1.6 DELETE /api/v1/author/{id}
**Chức năng**: Xóa tác giả
**Nghiệp vụ**:
- Xóa tác giả khỏi database theo ID
- Trả về lỗi nếu không tìm thấy tác giả

**Response**: 
- **200 OK** - ApiResponse<bool>
```json
{
    "success": true,
    "data": true,
    "message": "Author deleted successfully",
    "error": null
}
```

- **404 Not Found** - ApiResponse<bool>
```json
{
    "success": false,
    "data": false,
    "message": null,
    "error": "Author not found"
}
```

### 1.7 GET /api/v1/author/count
**Chức năng**: Lấy tổng số lượng tác giả
**Nghiệp vụ**:
- Đếm tổng số tác giả trong database
- Trả về số lượng

**Response**: 
- **200 OK** - ApiResponse<long>
```json
{
    "success": true,
    "data": 150,
    "message": null,
    "error": null
}
```

---

## 2. BookController

### 2.1 POST /api/v1/book
**Chức năng**: Tạo mới sách
**Nghiệp vụ**:
- Tạo đối tượng Book với thông tin từ DTO
- Mặc định sách được tạo với trạng thái Available = true
- Lưu vào database thông qua repository

**Response**: 
- **201 Created** - ApiResponse<BookDto>
```json
{
    "success": true,
    "data": {
        "id": "book_id",
        "title": "Book Title",
        "authors": ["author_id_1", "author_id_2"],
        "categories": ["category_id_1"],
        "description": "Book description",
        "publishYear": 2024,
        "fileUrl": "path/to/file.pdf",
        "fileFormat": "PDF",
        "isAvailable": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
    },
    "message": "Book created successfully",
    "error": null
}
```

### 2.2 GET /api/v1/book/{id}
**Chức năng**: Lấy thông tin sách theo ID
**Nghiệp vụ**:
- Tìm kiếm sách trong database theo ID
- Trả về thông tin chi tiết nếu tìm thấy
- Trả về lỗi 404 nếu không tìm thấy

**Response**: 
- **200 OK** - ApiResponse<BookDto> (thành công)
- **404 Not Found** - ApiResponse<BookDto> (không tìm thấy)

### 2.3 GET /api/v1/book
**Chức năng**: Lấy danh sách tất cả sách (có phân trang)
**Nghiệp vụ**:
- Lấy danh sách sách với phân trang (page, limit)
- Mặc định page=1, limit=10
- Trả về danh sách sách đã được map sang DTO

**Parameters**:
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BookDto>>

### 2.4 POST /api/v1/book/search
**Chức năng**: Tìm kiếm sách theo nhiều tiêu chí
**Nghiệp vụ**:
- Tìm kiếm sách theo từ khóa, danh mục, tác giả
- Lọc theo năm xuất bản (min/max)
- Lọc theo trạng thái có sẵn
- Hỗ trợ phân trang

**Search Criteria**:
- SearchQuery: Từ khóa tìm kiếm
- Categories: Danh sách danh mục
- Authors: Danh sách tác giả
- MinYear, MaxYear: Khoảng năm xuất bản
- IsAvailable: Trạng thái có sẵn
- Page, Limit: Phân trang

**Response**: 
- **200 OK** - ApiResponse<List<BookDto>>

### 2.5 PUT /api/v1/book/{id}
**Chức năng**: Cập nhật thông tin sách
**Nghiệp vụ**:
- Kiểm tra sách có tồn tại không
- Cập nhật các trường được cung cấp trong DTO
- Lưu thay đổi vào database

**Response**: 
- **200 OK** - ApiResponse<BookDto> (cập nhật thành công)
- **400 Bad Request** - ApiResponse<BookDto> (validation error)
- **404 Not Found** - ApiResponse<BookDto> (không tìm thấy sách)

### 2.6 DELETE /api/v1/book/{id}
**Chức năng**: Xóa sách
**Nghiệp vụ**:
- Xóa sách khỏi database theo ID
- Trả về lỗi nếu không tìm thấy sách

**Response**: 
- **200 OK** - ApiResponse<bool>
- **404 Not Found** - ApiResponse<bool>

### 2.7 GET /api/v1/book/count
**Chức năng**: Lấy tổng số lượng sách
**Nghiệp vụ**:
- Đếm tổng số sách trong database
- Trả về số lượng

**Response**: 
- **200 OK** - ApiResponse<long>

### 2.8 GET /api/v1/book/category/{categoryId}
**Chức năng**: Lấy sách theo danh mục
**Nghiệp vụ**:
- Tìm kiếm sách thuộc danh mục cụ thể
- Hỗ trợ phân trang

**Parameters**:
- categoryId: ID của danh mục
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BookDto>>

### 2.9 GET /api/v1/book/author/{authorId}
**Chức năng**: Lấy sách theo tác giả
**Nghiệp vụ**:
- Tìm kiếm sách của tác giả cụ thể
- Hỗ trợ phân trang

**Parameters**:
- authorId: ID của tác giả
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BookDto>>

### 2.10 GET /api/v1/book/featured
**Chức năng**: Lấy sách nổi bật
**Nghiệp vụ**:
- Lấy danh sách sách mới nhất làm sách nổi bật
- Mặc định limit = 6

**Parameters**:
- limit: Số lượng sách nổi bật (mặc định 6)

**Response**: 
- **200 OK** - ApiResponse<List<BookDto>>

### 2.11 PUT /api/v1/book/{id}/availability
**Chức năng**: Cập nhật trạng thái có sẵn của sách
**Nghiệp vụ**:
- Cập nhật trạng thái Available/Unavailable của sách
- Sử dụng để quản lý việc mượn/trả sách

**Request Body**:
- IsAvailable: true/false

**Response**: 
- **200 OK** - ApiResponse<bool>
```json
{
    "success": true,
    "data": true,
    "message": "Book availability updated successfully",
    "error": null
}
```

---

## 3. BorrowController

### 3.1 POST /api/v1/borrow/borrow
**Chức năng**: Mượn sách
**Nghiệp vụ**:
- Kiểm tra user có tồn tại và đang hoạt động
- Kiểm tra sách có tồn tại và đang có sẵn
- Kiểm tra user chưa mượn sách này
- Tạo record mượn sách với trạng thái "Borrowed"
- Cập nhật trạng thái sách thành không có sẵn

**Validation**:
- User phải tồn tại và active
- Sách phải tồn tại và available
- User chưa mượn sách này

**Response**: 
- **201 Created** - ApiResponse<BorrowDto>
```json
{
    "success": true,
    "data": {
        "id": "borrow_id",
        "userId": "user_id",
        "bookId": "book_id",
        "borrowDate": "2024-01-01T00:00:00Z",
        "dueDate": "2024-01-15T00:00:00Z",
        "returnDate": null,
        "status": "Borrowed"
    },
    "message": "Book borrowed successfully",
    "error": null
}
```

- **400 Bad Request** - ApiResponse<BorrowDto> (validation error)
```json
{
    "success": false,
    "data": null,
    "message": null,
    "error": "User already has this book borrowed"
}
```

### 3.2 POST /api/v1/borrow/return
**Chức năng**: Trả sách
**Nghiệp vụ**:
- Kiểm tra record mượn sách có tồn tại
- Kiểm tra sách đang được mượn (status = "Borrowed")
- Cập nhật ngày trả và trạng thái thành "Returned"
- Cập nhật trạng thái sách thành có sẵn

**Validation**:
- Record mượn sách phải tồn tại
- Sách phải đang được mượn

**Response**: 
- **200 OK** - ApiResponse<BorrowDto>
```json
{
    "success": true,
    "data": {
        "id": "borrow_id",
        "userId": "user_id",
        "bookId": "book_id",
        "borrowDate": "2024-01-01T00:00:00Z",
        "dueDate": "2024-01-15T00:00:00Z",
        "returnDate": "2024-01-10T00:00:00Z",
        "status": "Returned"
    },
    "message": "Book returned successfully",
    "error": null
}
```

### 3.3 GET /api/v1/borrow/{id}
**Chức năng**: Lấy thông tin mượn sách theo ID
**Nghiệp vụ**:
- Tìm kiếm record mượn sách theo ID
- Trả về thông tin chi tiết nếu tìm thấy

**Response**: 
- **200 OK** - ApiResponse<BorrowDto> (thành công)
- **404 Not Found** - ApiResponse<BorrowDto> (không tìm thấy)

### 3.4 GET /api/v1/borrow/user/{userId}
**Chức năng**: Lấy danh sách mượn sách của user
**Nghiệp vụ**:
- Tìm kiếm tất cả record mượn sách của user cụ thể
- Hỗ trợ phân trang

**Parameters**:
- userId: ID của user
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.5 GET /api/v1/borrow/user/{userId}/borrows
**Chức năng**: Lấy danh sách mượn sách của user (alias)
**Nghiệp vụ**:
- Tương tự như endpoint trên, đây là alias

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.6 GET /api/v1/borrow/book/{bookId}
**Chức năng**: Lấy danh sách mượn sách của sách
**Nghiệp vụ**:
- Tìm kiếm tất cả record mượn sách của sách cụ thể
- Hỗ trợ phân trang

**Parameters**:
- bookId: ID của sách
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.7 GET /api/v1/borrow/status/{status}
**Chức năng**: Lấy danh sách mượn sách theo trạng thái
**Nghiệp vụ**:
- Tìm kiếm record mượn sách theo trạng thái (Borrowed, Returned)
- Hỗ trợ phân trang

**Parameters**:
- status: Trạng thái mượn sách
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.8 GET /api/v1/borrow/all
**Chức năng**: Lấy tất cả danh sách mượn sách
**Nghiệp vụ**:
- Lấy tất cả record mượn sách với phân trang

**Parameters**:
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.9 GET /api/v1/borrow/overdue
**Chức năng**: Lấy danh sách sách quá hạn
**Nghiệp vụ**:
- Tìm kiếm các record mượn sách đã quá hạn trả
- Dựa trên DueDate < DateTime.UtcNow và status = "Borrowed"

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.10 GET /api/v1/borrow/count
**Chức năng**: Lấy tổng số lượng mượn sách
**Nghiệp vụ**:
- Đếm tổng số record mượn sách trong database

**Response**: 
- **200 OK** - ApiResponse<long>

### 3.11 GET /api/v1/borrow/count/active
**Chức năng**: Lấy số lượng mượn sách đang hoạt động
**Nghiệp vụ**:
- Đếm số record mượn sách có status = "Borrowed"

**Response**: 
- **200 OK** - ApiResponse<long>

### 3.12 GET /api/v1/borrow/count/overdue
**Chức năng**: Lấy số lượng sách quá hạn
**Nghiệp vụ**:
- Đếm số record mượn sách đã quá hạn trả

**Response**: 
- **200 OK** - ApiResponse<long>

### 3.13 GET /api/v1/borrow/date-range
**Chức năng**: Lấy mượn sách theo khoảng thời gian
**Nghiệp vụ**:
- Tìm kiếm record mượn sách trong khoảng thời gian cụ thể
- Dựa trên BorrowDate

**Parameters**:
- startDate: Ngày bắt đầu
- endDate: Ngày kết thúc

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

### 3.14 GET /api/v1/borrow/can-borrow
**Chức năng**: Kiểm tra user có thể mượn sách không
**Nghiệp vụ**:
- Kiểm tra user chưa mượn sách này (chưa có record active)

**Parameters**:
- userId: ID của user
- bookId: ID của sách

**Response**: 
- **200 OK** - ApiResponse<bool>
```json
{
    "success": true,
    "data": true,
    "message": null,
    "error": null
}
```

### 3.15 POST /api/v1/borrow/search
**Chức năng**: Tìm kiếm mượn sách theo nhiều tiêu chí
**Nghiệp vụ**:
- Tìm kiếm record mượn sách theo user, sách, trạng thái
- Lọc theo khoảng thời gian
- Hỗ trợ phân trang

**Search Criteria**:
- UserId: ID của user
- BookId: ID của sách
- Status: Trạng thái mượn sách
- FromDate, ToDate: Khoảng thời gian
- Page, Limit: Phân trang

**Response**: 
- **200 OK** - ApiResponse<List<BorrowDto>>

---

## 4. CategoryController

### 4.1 POST /api/v1/category
**Chức năng**: Tạo mới danh mục
**Nghiệp vụ**:
- Kiểm tra tên danh mục đã tồn tại chưa (duplicate check)
- Tạo đối tượng Category với thông tin từ DTO
- Lưu vào database thông qua repository

**Validation**:
- Tên danh mục không được trùng lặp

**Response**: 
- **201 Created** - ApiResponse<CategoryDto>
```json
{
    "success": true,
    "data": {
        "id": "category_id",
        "name": "Category Name",
        "description": "Category Description",
        "createdAt": "2024-01-01T00:00:00Z"
    },
    "message": "Category created successfully",
    "error": null
}
```

### 4.2 GET /api/v1/category/{id}
**Chức năng**: Lấy thông tin danh mục theo ID
**Nghiệp vụ**:
- Tìm kiếm danh mục trong database theo ID
- Trả về thông tin chi tiết nếu tìm thấy

**Response**: 
- **200 OK** - ApiResponse<CategoryDto> (thành công)
- **404 Not Found** - ApiResponse<CategoryDto> (không tìm thấy)

### 4.3 GET /api/v1/category/name/{name}
**Chức năng**: Lấy thông tin danh mục theo tên
**Nghiệp vụ**:
- Tìm kiếm danh mục trong database theo tên
- Trả về thông tin chi tiết nếu tìm thấy

**Response**: 
- **200 OK** - ApiResponse<CategoryDto> (thành công)
- **404 Not Found** - ApiResponse<CategoryDto> (không tìm thấy)

### 4.4 GET /api/v1/category
**Chức năng**: Lấy danh sách tất cả danh mục (có phân trang)
**Nghiệp vụ**:
- Lấy danh sách danh mục với phân trang (page, limit)
- Mặc định page=1, limit=10

**Parameters**:
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<CategoryDto>>

### 4.5 PUT /api/v1/category/{id}
**Chức năng**: Cập nhật thông tin danh mục
**Nghiệp vụ**:
- Kiểm tra danh mục có tồn tại không
- Kiểm tra tên mới không trùng với danh mục khác (nếu có thay đổi tên)
- Cập nhật các trường được cung cấp

**Validation**:
- Danh mục phải tồn tại
- Tên mới không được trùng với danh mục khác

**Response**: 
- **200 OK** - ApiResponse<CategoryDto> (cập nhật thành công)
- **400 Bad Request** - ApiResponse<CategoryDto> (validation error)
- **404 Not Found** - ApiResponse<CategoryDto> (không tìm thấy danh mục)

### 4.6 DELETE /api/v1/category/{id}
**Chức năng**: Xóa danh mục
**Nghiệp vụ**:
- Xóa danh mục khỏi database theo ID
- Trả về lỗi nếu không tìm thấy danh mục

**Response**: 
- **200 OK** - ApiResponse<bool>
- **404 Not Found** - ApiResponse<bool>

### 4.7 GET /api/v1/category/count
**Chức năng**: Lấy tổng số lượng danh mục
**Nghiệp vụ**:
- Đếm tổng số danh mục trong database

**Response**: 
- **200 OK** - ApiResponse<long>

---

## 5. FileUploadController

### 5.1 POST /api/v1/fileupload
**Chức năng**: Tạo mới file upload
**Nghiệp vụ**:
- Tạo đối tượng FileUpload với thông tin từ DTO
- Lưu thông tin file vào database
- Thường được sử dụng để lưu metadata của file đã upload

**Response**: 
- **201 Created** - ApiResponse<FileUploadDto>
```json
{
    "success": true,
    "data": {
        "id": "file_id",
        "filename": "document.pdf",
        "length": 1024000,
        "uploadDate": "2024-01-01T00:00:00Z",
        "bookId": "book_id"
    },
    "message": "File upload created successfully",
    "error": null
}
```

### 5.2 GET /api/v1/fileupload/{id}
**Chức năng**: Lấy thông tin file upload theo ID
**Nghiệp vụ**:
- Tìm kiếm file upload trong database theo ID
- Trả về thông tin chi tiết nếu tìm thấy

**Response**: 
- **200 OK** - ApiResponse<FileUploadDto> (thành công)
- **404 Not Found** - ApiResponse<FileUploadDto> (không tìm thấy)

### 5.3 GET /api/v1/fileupload/book/{bookId}
**Chức năng**: Lấy danh sách file upload của sách
**Nghiệp vụ**:
- Tìm kiếm tất cả file upload thuộc về sách cụ thể
- Trả về danh sách file upload

**Parameters**:
- bookId: ID của sách

**Response**: 
- **200 OK** - ApiResponse<List<FileUploadDto>>

### 5.4 GET /api/v1/fileupload
**Chức năng**: Lấy danh sách tất cả file upload (có phân trang)
**Nghiệp vụ**:
- Lấy danh sách file upload với phân trang (page, limit)
- Mặc định page=1, limit=10

**Parameters**:
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<FileUploadDto>>

### 5.5 DELETE /api/v1/fileupload/{id}
**Chức năng**: Xóa file upload
**Nghiệp vụ**:
- Xóa file upload khỏi database theo ID
- Trả về lỗi nếu không tìm thấy file upload

**Response**: 
- **200 OK** - ApiResponse<bool>
- **404 Not Found** - ApiResponse<bool>

### 5.6 GET /api/v1/fileupload/count
**Chức năng**: Lấy tổng số lượng file upload
**Nghiệp vụ**:
- Đếm tổng số file upload trong database

**Response**: 
- **200 OK** - ApiResponse<long>

---

## 6. UserController

### 6.1 POST /api/v1/user/register
**Chức năng**: Đăng ký user mới
**Nghiệp vụ**:
- Kiểm tra username đã tồn tại chưa
- Kiểm tra email đã tồn tại chưa
- Mã hóa password bằng BCrypt
- Tạo user mới với trạng thái active
- Lưu vào database

**Validation**:
- Username không được trùng lặp
- Email không được trùng lặp
- Password được mã hóa an toàn

**Response**: 
- **201 Created** - ApiResponse<UserDto>
```json
{
    "success": true,
    "data": {
        "id": "user_id",
        "username": "john_doe",
        "email": "john@example.com",
        "fullName": "John Doe",
        "role": "User",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z",
        "isActive": true
    },
    "message": "User registered successfully",
    "error": null
}
```

- **400 Bad Request** - ApiResponse<UserDto> (validation error)
```json
{
    "success": false,
    "data": null,
    "message": null,
    "error": "Username already exists"
}
```

### 6.2 POST /api/v1/user/login
**Chức năng**: Đăng nhập user
**Nghiệp vụ**:
- Tìm user theo username
- Kiểm tra user có active không
- Xác thực password bằng BCrypt
- Trả về thông tin user nếu đăng nhập thành công

**Validation**:
- User phải tồn tại và active
- Password phải đúng

**Response**: 
- **200 OK** - ApiResponse<UserDto> (đăng nhập thành công)
- **401 Unauthorized** - ApiResponse<UserDto> (thông tin đăng nhập sai)
```json
{
    "success": false,
    "data": null,
    "message": null,
    "error": "Invalid username or password"
}
```

### 6.3 GET /api/v1/user/{id}
**Chức năng**: Lấy thông tin user theo ID
**Nghiệp vụ**:
- Tìm kiếm user trong database theo ID
- Trả về thông tin chi tiết nếu tìm thấy

**Response**: 
- **200 OK** - ApiResponse<UserDto> (thành công)
- **404 Not Found** - ApiResponse<UserDto> (không tìm thấy)

### 6.4 GET /api/v1/user/username/{username}
**Chức năng**: Lấy thông tin user theo username
**Nghiệp vụ**:
- Tìm kiếm user trong database theo username
- Trả về thông tin chi tiết nếu tìm thấy

**Response**: 
- **200 OK** - ApiResponse<UserDto> (thành công)
- **404 Not Found** - ApiResponse<UserDto> (không tìm thấy)

### 6.5 GET /api/v1/user
**Chức năng**: Lấy danh sách tất cả user (có phân trang)
**Nghiệp vụ**:
- Lấy danh sách user với phân trang (page, limit)
- Mặc định page=1, limit=10

**Parameters**:
- page: Số trang (mặc định 1)
- limit: Số lượng item mỗi trang (mặc định 10)

**Response**: 
- **200 OK** - ApiResponse<List<UserDto>>

### 6.6 PUT /api/v1/user/{id}
**Chức năng**: Cập nhật thông tin user
**Nghiệp vụ**:
- Kiểm tra user có tồn tại không
- Kiểm tra email mới không trùng với user khác (nếu có thay đổi email)
- Cập nhật các trường được cung cấp
- Cập nhật trạng thái active nếu có

**Validation**:
- User phải tồn tại
- Email mới không được trùng với user khác

**Response**: 
- **200 OK** - ApiResponse<UserDto> (cập nhật thành công)
- **400 Bad Request** - ApiResponse<UserDto> (validation error)
- **404 Not Found** - ApiResponse<UserDto> (không tìm thấy user)

### 6.7 DELETE /api/v1/user/{id}
**Chức năng**: Xóa user
**Nghiệp vụ**:
- Xóa user khỏi database theo ID
- Trả về lỗi nếu không tìm thấy user

**Response**: 
- **200 OK** - ApiResponse<bool>
- **404 Not Found** - ApiResponse<bool>

### 6.8 GET /api/v1/user/count
**Chức năng**: Lấy tổng số lượng user
**Nghiệp vụ**:
- Đếm tổng số user trong database

**Response**: 
- **200 OK** - ApiResponse<long>

### 6.9 POST /api/v1/user/{id}/change-password
**Chức năng**: Đổi mật khẩu user
**Nghiệp vụ**:
- Kiểm tra user có tồn tại không
- Xác thực mật khẩu hiện tại
- Mã hóa mật khẩu mới bằng BCrypt
- Cập nhật mật khẩu mới

**Validation**:
- User phải tồn tại
- Mật khẩu hiện tại phải đúng

**Request Body**:
```json
{
    "currentPassword": "old_password",
    "newPassword": "new_password"
}
```

**Response**: 
- **200 OK** - ApiResponse<bool>
```json
{
    "success": true,
    "data": true,
    "message": "Password changed successfully",
    "error": null
}
```

- **400 Bad Request** - ApiResponse<bool>
```json
{
    "success": false,
    "data": false,
    "message": null,
    "error": "Current password is incorrect"
}
```

---

## Tổng kết

Hệ thống API được thiết kế theo chuẩn RESTful với các đặc điểm chính:

1. **CRUD Operations**: Đầy đủ các thao tác Create, Read, Update, Delete
2. **Pagination**: Hỗ trợ phân trang cho các endpoint list
3. **Search & Filter**: Tìm kiếm và lọc dữ liệu linh hoạt
4. **Validation**: Kiểm tra dữ liệu đầu vào nghiêm ngặt
5. **Error Handling**: Xử lý lỗi nhất quán với ApiResponse
6. **Security**: Mã hóa password, kiểm tra quyền truy cập
7. **Business Logic**: Logic nghiệp vụ phức tạp cho mượn/trả sách
8. **Consistent Response Format**: Tất cả API đều sử dụng ApiResponse<T> để chuẩn hóa format trả về

Tất cả các controller đều sử dụng pattern Service-Repository để tách biệt logic nghiệp vụ và truy cập dữ liệu, đảm bảo code dễ bảo trì và mở rộng.
