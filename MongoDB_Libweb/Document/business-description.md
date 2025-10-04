# Electronic Library System - API Business Description

## Tổng quan
Hệ thống thư viện điện tử được xây dựng với ASP.NET Core 8.0 và MongoDB, cung cấp các API RESTful để quản lý người dùng, sách, mượn trả, danh mục, tác giả và file upload.

---

## 1. User Management APIs

### 1.1 POST /api/v1/users/register
**Mục đích**: Đăng ký tài khoản người dùng mới

**Business Logic**:
- Kiểm tra username và email đã tồn tại chưa
- Mã hóa mật khẩu bằng BCrypt
- Tạo user với role mặc định "User" và trạng thái active
- Tự động set thời gian tạo và cập nhật

**Input**: UserCreateDto (username, password, email, fullName, role)
**Output**: UserDto với thông tin user đã tạo
**Status Codes**: 201 Created, 400 Bad Request

### 1.2 POST /api/v1/users/login
**Mục đích**: Xác thực đăng nhập người dùng

**Business Logic**:
- Tìm user theo username
- Kiểm tra user có active không
- Xác thực mật khẩu bằng BCrypt
- Trả về thông tin user nếu đăng nhập thành công

**Input**: UserLoginDto (username, password)
**Output**: UserDto với thông tin user
**Status Codes**: 200 OK, 401 Unauthorized

### 1.3 GET /api/v1/users/{id}
**Mục đích**: Lấy thông tin user theo ID

**Business Logic**:
- Tìm user theo ID
- Trả về thông tin user nếu tồn tại

**Input**: User ID (string)
**Output**: UserDto
**Status Codes**: 200 OK, 404 Not Found

### 1.4 GET /api/v1/users/username/{username}
**Mục đích**: Lấy thông tin user theo username

**Business Logic**:
- Tìm user theo username
- Trả về thông tin user nếu tồn tại

**Input**: Username (string)
**Output**: UserDto
**Status Codes**: 200 OK, 404 Not Found

### 1.5 GET /api/v1/users
**Mục đích**: Lấy danh sách tất cả users với phân trang

**Business Logic**:
- Lấy danh sách users với pagination
- Chỉ trả về users đang active
- Hỗ trợ phân trang với page và limit

**Input**: Query parameters (page, limit)
**Output**: List<UserDto>
**Status Codes**: 200 OK

### 1.6 PUT /api/v1/users/{id}
**Mục đích**: Cập nhật thông tin user

**Business Logic**:
- Tìm user theo ID
- Kiểm tra email mới có trùng không (nếu có thay đổi)
- Cập nhật các field được cung cấp
- Tự động cập nhật thời gian updatedAt

**Input**: User ID, UserUpdateDto (fullName, email, isActive)
**Output**: UserDto đã cập nhật
**Status Codes**: 200 OK, 400 Bad Request, 404 Not Found

### 1.7 DELETE /api/v1/users/{id}
**Mục đích**: Xóa user

**Business Logic**:
- Xóa user theo ID
- Trả về kết quả thành công hoặc lỗi

**Input**: User ID
**Output**: Boolean result
**Status Codes**: 200 OK, 404 Not Found

### 1.8 GET /api/v1/users/count
**Mục đích**: Lấy tổng số lượng users

**Business Logic**:
- Đếm tổng số users active trong hệ thống

**Output**: Long (số lượng)
**Status Codes**: 200 OK

### 1.9 POST /api/v1/users/{id}/change-password
**Mục đích**: Đổi mật khẩu user

**Business Logic**:
- Tìm user theo ID
- Xác thực mật khẩu hiện tại
- Mã hóa mật khẩu mới bằng BCrypt
- Cập nhật password hash

**Input**: User ID, ChangePasswordRequest (currentPassword, newPassword)
**Output**: Boolean result
**Status Codes**: 200 OK, 400 Bad Request

---

## 2. Book Management APIs

### 2.1 POST /api/v1/book
**Mục đích**: Tạo sách mới

**Business Logic**:
- Tạo book với thông tin từ DTO
- Mặc định set IsAvailable = true
- Tự động set thời gian tạo và cập nhật

**Input**: BookCreateDto (title, authors, categories, description, publishYear, fileUrl, fileFormat)
**Output**: BookDto
**Status Codes**: 201 Created, 400 Bad Request

### 2.2 GET /api/v1/book/{id}
**Mục đích**: Lấy thông tin sách theo ID

**Business Logic**:
- Tìm book theo ID
- Trả về thông tin book nếu tồn tại

**Input**: Book ID
**Output**: BookDto
**Status Codes**: 200 OK, 404 Not Found

### 2.3 GET /api/v1/book
**Mục đích**: Lấy danh sách tất cả sách với phân trang

**Business Logic**:
- Lấy danh sách books với pagination
- Hỗ trợ phân trang với page và limit

**Input**: Query parameters (page, limit)
**Output**: List<BookDto>
**Status Codes**: 200 OK

### 2.4 POST /api/v1/book/search
**Mục đích**: Tìm kiếm sách với nhiều tiêu chí

**Business Logic**:
- Tìm kiếm theo text search (title, authors, description)
- Lọc theo categories, authors
- Lọc theo năm xuất bản (min/max)
- Lọc theo trạng thái available
- Hỗ trợ pagination

**Input**: BookSearchDto (searchQuery, categories, authors, minYear, maxYear, isAvailable, page, limit)
**Output**: List<BookDto>
**Status Codes**: 200 OK

### 2.5 PUT /api/v1/book/{id}
**Mục đích**: Cập nhật thông tin sách

**Business Logic**:
- Tìm book theo ID
- Cập nhật các field được cung cấp
- Tự động cập nhật thời gian updatedAt

**Input**: Book ID, BookUpdateDto (title, authors, categories, description, publishYear, fileUrl, fileFormat, isAvailable)
**Output**: BookDto đã cập nhật
**Status Codes**: 200 OK, 400 Bad Request, 404 Not Found

### 2.6 DELETE /api/v1/book/{id}
**Mục đích**: Xóa sách

**Business Logic**:
- Xóa book theo ID
- Trả về kết quả thành công hoặc lỗi

**Input**: Book ID
**Output**: Boolean result
**Status Codes**: 200 OK, 404 Not Found

### 2.7 GET /api/v1/book/count
**Mục đích**: Lấy tổng số lượng sách

**Business Logic**:
- Đếm tổng số books trong hệ thống

**Output**: Long (số lượng)
**Status Codes**: 200 OK

### 2.8 GET /api/v1/book/category/{categoryId}
**Mục đích**: Lấy sách theo danh mục

**Business Logic**:
- Tìm tất cả books có chứa categoryId trong danh sách categories

**Input**: Category ID
**Output**: List<BookDto>
**Status Codes**: 200 OK

### 2.9 GET /api/v1/book/author/{authorId}
**Mục đích**: Lấy sách theo tác giả

**Business Logic**:
- Tìm tất cả books có chứa authorId trong danh sách authors

**Input**: Author ID
**Output**: List<BookDto>
**Status Codes**: 200 OK

### 2.10 PUT /api/v1/book/{id}/availability
**Mục đích**: Thay đổi trạng thái khả dụng của sách

**Business Logic**:
- Cập nhật trạng thái IsAvailable của book
- Tự động cập nhật thời gian updatedAt

**Input**: Book ID, SetAvailabilityRequest (isAvailable)
**Output**: Boolean result
**Status Codes**: 200 OK, 400 Bad Request

---

## 3. Borrow Management APIs

### 3.1 POST /api/v1/borrows/borrow
**Mục đích**: Mượn sách

**Business Logic**:
- Kiểm tra user tồn tại và active
- Kiểm tra book tồn tại và available
- Kiểm tra user chưa mượn book này
- Tạo borrow record với status "Borrowed"
- Cập nhật book availability thành false

**Input**: BorrowCreateDto (userId, bookId, dueDate)
**Output**: BorrowDto
**Status Codes**: 201 Created, 400 Bad Request

### 3.2 POST /api/v1/borrows/return
**Mục đích**: Trả sách

**Business Logic**:
- Tìm borrow record theo ID
- Kiểm tra status là "Borrowed"
- Cập nhật returnDate và status thành "Returned"
- Cập nhật book availability thành true

**Input**: BorrowReturnDto (borrowId)
**Output**: BorrowDto
**Status Codes**: 200 OK, 400 Bad Request

### 3.3 GET /api/v1/borrows/{id}
**Mục đích**: Lấy thông tin mượn trả theo ID

**Business Logic**:
- Tìm borrow record theo ID
- Trả về thông tin borrow nếu tồn tại

**Input**: Borrow ID
**Output**: BorrowDto
**Status Codes**: 200 OK, 404 Not Found

### 3.4 GET /api/v1/borrows/user/{userId}
**Mục đích**: Lấy danh sách mượn trả của user

**Business Logic**:
- Tìm tất cả borrow records của user với pagination

**Input**: User ID, Query parameters (page, limit)
**Output**: List<BorrowDto>
**Status Codes**: 200 OK

### 3.5 GET /api/v1/borrows/book/{bookId}
**Mục đích**: Lấy danh sách mượn trả của sách

**Business Logic**:
- Tìm tất cả borrow records của book với pagination

**Input**: Book ID, Query parameters (page, limit)
**Output**: List<BorrowDto>
**Status Codes**: 200 OK

### 3.6 GET /api/v1/borrows/status/{status}
**Mục đích**: Lấy danh sách mượn trả theo trạng thái

**Business Logic**:
- Tìm tất cả borrow records theo status với pagination
- Status có thể là: "Borrowed", "Returned", "Overdue"

**Input**: Status, Query parameters (page, limit)
**Output**: List<BorrowDto>
**Status Codes**: 200 OK

### 3.7 GET /api/v1/borrows/overdue
**Mục đích**: Lấy danh sách sách quá hạn

**Business Logic**:
- Tìm tất cả borrow records có status "Borrowed" và dueDate < hiện tại

**Output**: List<BorrowDto>
**Status Codes**: 200 OK

### 3.8 GET /api/v1/borrows/count
**Mục đích**: Lấy tổng số lượng mượn trả

**Business Logic**:
- Đếm tổng số borrow records trong hệ thống

**Output**: Long (số lượng)
**Status Codes**: 200 OK

### 3.9 GET /api/v1/borrows/can-borrow
**Mục đích**: Kiểm tra user có thể mượn sách không

**Business Logic**:
- Kiểm tra user chưa có borrow record active cho book này

**Input**: Query parameters (userId, bookId)
**Output**: Boolean result
**Status Codes**: 200 OK

### 3.10 POST /api/v1/borrows/search
**Mục đích**: Tìm kiếm mượn trả với nhiều tiêu chí

**Business Logic**:
- Tìm kiếm theo userId, bookId, status
- Lọc theo khoảng thời gian (fromDate, toDate)
- Hỗ trợ pagination

**Input**: BorrowSearchDto (userId, bookId, status, fromDate, toDate, page, limit)
**Output**: List<BorrowDto>
**Status Codes**: 200 OK

---

## 4. Category Management APIs

### 4.1 POST /api/v1/categories
**Mục đích**: Tạo danh mục mới

**Business Logic**:
- Kiểm tra tên danh mục chưa tồn tại
- Tạo category với thông tin từ DTO
- Tự động set thời gian tạo

**Input**: CategoryCreateDto (name, description)
**Output**: CategoryDto
**Status Codes**: 201 Created, 400 Bad Request

### 4.2 GET /api/v1/categories/{id}
**Mục đích**: Lấy thông tin danh mục theo ID

**Business Logic**:
- Tìm category theo ID
- Trả về thông tin category nếu tồn tại

**Input**: Category ID
**Output**: CategoryDto
**Status Codes**: 200 OK, 404 Not Found

### 4.3 GET /api/v1/categories/name/{name}
**Mục đích**: Lấy thông tin danh mục theo tên

**Business Logic**:
- Tìm category theo name
- Trả về thông tin category nếu tồn tại

**Input**: Category name
**Output**: CategoryDto
**Status Codes**: 200 OK, 404 Not Found

### 4.4 GET /api/v1/categories
**Mục đích**: Lấy danh sách tất cả danh mục với phân trang

**Business Logic**:
- Lấy danh sách categories với pagination

**Input**: Query parameters (page, limit)
**Output**: List<CategoryDto>
**Status Codes**: 200 OK

### 4.5 PUT /api/v1/categories/{id}
**Mục đích**: Cập nhật thông tin danh mục

**Business Logic**:
- Tìm category theo ID
- Kiểm tra tên mới chưa trùng (nếu có thay đổi)
- Cập nhật các field được cung cấp

**Input**: Category ID, CategoryUpdateDto (name, description)
**Output**: CategoryDto đã cập nhật
**Status Codes**: 200 OK, 400 Bad Request, 404 Not Found

### 4.6 DELETE /api/v1/categories/{id}
**Mục đích**: Xóa danh mục

**Business Logic**:
- Xóa category theo ID
- Trả về kết quả thành công hoặc lỗi

**Input**: Category ID
**Output**: Boolean result
**Status Codes**: 200 OK, 404 Not Found

### 4.7 GET /api/v1/categories/count
**Mục đích**: Lấy tổng số lượng danh mục

**Business Logic**:
- Đếm tổng số categories trong hệ thống

**Output**: Long (số lượng)
**Status Codes**: 200 OK

---

## 5. Author Management APIs

### 5.1 POST /api/v1/authors
**Mục đích**: Tạo tác giả mới

**Business Logic**:
- Kiểm tra tên tác giả chưa tồn tại
- Tạo author với thông tin từ DTO
- Tự động set thời gian tạo

**Input**: AuthorCreateDto (name, bio)
**Output**: AuthorDto
**Status Codes**: 201 Created, 400 Bad Request

### 5.2 GET /api/v1/authors/{id}
**Mục đích**: Lấy thông tin tác giả theo ID

**Business Logic**:
- Tìm author theo ID
- Trả về thông tin author nếu tồn tại

**Input**: Author ID
**Output**: AuthorDto
**Status Codes**: 200 OK, 404 Not Found

### 5.3 GET /api/v1/authors/name/{name}
**Mục đích**: Lấy thông tin tác giả theo tên

**Business Logic**:
- Tìm author theo name
- Trả về thông tin author nếu tồn tại

**Input**: Author name
**Output**: AuthorDto
**Status Codes**: 200 OK, 404 Not Found

### 5.4 GET /api/v1/authors
**Mục đích**: Lấy danh sách tất cả tác giả với phân trang

**Business Logic**:
- Lấy danh sách authors với pagination

**Input**: Query parameters (page, limit)
**Output**: List<AuthorDto>
**Status Codes**: 200 OK

### 5.5 PUT /api/v1/authors/{id}
**Mục đích**: Cập nhật thông tin tác giả

**Business Logic**:
- Tìm author theo ID
- Kiểm tra tên mới chưa trùng (nếu có thay đổi)
- Cập nhật các field được cung cấp

**Input**: Author ID, AuthorUpdateDto (name, bio)
**Output**: AuthorDto đã cập nhật
**Status Codes**: 200 OK, 400 Bad Request, 404 Not Found

### 5.6 DELETE /api/v1/authors/{id}
**Mục đích**: Xóa tác giả

**Business Logic**:
- Xóa author theo ID
- Trả về kết quả thành công hoặc lỗi

**Input**: Author ID
**Output**: Boolean result
**Status Codes**: 200 OK, 404 Not Found

### 5.7 GET /api/v1/authors/count
**Mục đích**: Lấy tổng số lượng tác giả

**Business Logic**:
- Đếm tổng số authors trong hệ thống

**Output**: Long (số lượng)
**Status Codes**: 200 OK

---

## 6. File Upload Management APIs

### 6.1 POST /api/v1/fileuploads
**Mục đích**: Tạo metadata file upload

**Business Logic**:
- Tạo file upload record với thông tin từ DTO
- Tự động set thời gian upload

**Input**: FileUploadCreateDto (filename, length, bookId)
**Output**: FileUploadDto
**Status Codes**: 201 Created, 400 Bad Request

### 6.2 GET /api/v1/fileuploads/{id}
**Mục đích**: Lấy thông tin file upload theo ID

**Business Logic**:
- Tìm file upload theo ID
- Trả về thông tin file upload nếu tồn tại

**Input**: File upload ID
**Output**: FileUploadDto
**Status Codes**: 200 OK, 404 Not Found

### 6.3 GET /api/v1/fileuploads/book/{bookId}
**Mục đích**: Lấy danh sách file upload của sách

**Business Logic**:
- Tìm tất cả file uploads của book

**Input**: Book ID
**Output**: List<FileUploadDto>
**Status Codes**: 200 OK

### 6.4 GET /api/v1/fileuploads
**Mục đích**: Lấy danh sách tất cả file upload với phân trang

**Business Logic**:
- Lấy danh sách file uploads với pagination

**Input**: Query parameters (page, limit)
**Output**: List<FileUploadDto>
**Status Codes**: 200 OK

### 6.5 DELETE /api/v1/fileuploads/{id}
**Mục đích**: Xóa file upload

**Business Logic**:
- Xóa file upload theo ID
- Trả về kết quả thành công hoặc lỗi

**Input**: File upload ID
**Output**: Boolean result
**Status Codes**: 200 OK, 404 Not Found

### 6.6 GET /api/v1/fileuploads/count
**Mục đích**: Lấy tổng số lượng file upload

**Business Logic**:
- Đếm tổng số file uploads trong hệ thống

**Output**: Long (số lượng)
**Status Codes**: 200 OK

---

## 7. Common Response Format

Tất cả API đều trả về response theo format chuẩn:

```json
{
  "success": boolean,
  "data": object | array | null,
  "message": string | null,
  "error": string | null
}
```

## 8. Error Handling

- **400 Bad Request**: Dữ liệu đầu vào không hợp lệ
- **401 Unauthorized**: Chưa xác thực hoặc token không hợp lệ
- **403 Forbidden**: Không có quyền truy cập
- **404 Not Found**: Không tìm thấy resource
- **500 Internal Server Error**: Lỗi server

## 9. Authentication & Authorization

- Sử dụng JWT Bearer token cho authentication
- Role-based authorization (Admin, User)
- Admin có quyền truy cập tất cả APIs
- User có quyền hạn chế theo business rules

## 10. Pagination

Tất cả API list đều hỗ trợ pagination:
- `page`: Số trang (bắt đầu từ 1)
- `limit`: Số item mỗi trang (mặc định 10)

## 11. Business Rules

1. **User Management**: Username và email phải unique
2. **Book Management**: Mỗi book có thể có nhiều authors và categories
3. **Borrow Management**: User chỉ có thể mượn 1 copy của mỗi book
4. **Category/Author Management**: Tên phải unique
5. **File Management**: File metadata được link với book qua bookId
