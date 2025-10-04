# Book Management System - Enhanced Features

## Tổng quan
Hệ thống quản lý sách đã được nâng cấp với các tính năng mới:
- Multi-select authors và categories
- Upload file PDF/EPUB với validation
- Multi-filter search với pagination
- RESTful API endpoints mới
- UI hiện đại với Select2 và drag-drop file upload

## Các tính năng mới

### 1. Multi-select Authors và Categories
- **Authors**: Chọn nhiều tác giả từ dropdown có thể tìm kiếm
- **Categories**: Chọn nhiều danh mục từ dropdown có thể tìm kiếm
- **UI**: Sử dụng Select2 với theme Bootstrap 5
- **Backend**: Hỗ trợ array trong MongoDB queries

### 2. File Upload System
- **File Types**: Chỉ chấp nhận PDF và EPUB
- **File Size**: Giới hạn 50MB
- **Storage**: Lưu trong `wwwroot/files/books/` với tên file unique
- **Validation**: Client-side và server-side validation
- **Metadata**: Lưu thông tin file trong FileUpload collection

### 3. Multi-filter Search
- **Text Search**: Full-text search trên title, authors, description
- **Authors Filter**: Lọc theo nhiều tác giả
- **Categories Filter**: Lọc theo nhiều danh mục
- **Year Range**: Lọc theo khoảng năm xuất bản
- **Status Filter**: Lọc theo trạng thái available/unavailable
- **Pagination**: Hỗ trợ phân trang với limit tùy chọn

### 4. API Endpoints mới

#### GET /api/v1/book/authors
Lấy danh sách tất cả authors từ database
```json
{
  "success": true,
  "data": ["Author 1", "Author 2", "Author 3"]
}
```

#### GET /api/v1/book/categories
Lấy danh sách tất cả categories từ database
```json
{
  "success": true,
  "data": ["Category 1", "Category 2", "Category 3"]
}
```

#### POST /api/v1/book/search/count
Đếm số lượng kết quả search
```json
{
  "success": true,
  "data": 25
}
```

#### POST /api/v1/book/validate-file
Validate file trước khi upload
```json
{
  "success": true,
  "data": true
}
```

### 5. Cập nhật DTOs

#### BookCreateDto
```csharp
public class BookCreateDto
{
    [Required] public string Title { get; set; }
    [Required] public List<string> Authors { get; set; }
    [Required] public List<string> Categories { get; set; }
    [Required] public string Description { get; set; }
    [Range(1900, 2100)] public int? PublishYear { get; set; }
    [Required] public IFormFile File { get; set; }
}
```

#### BookUpdateDto
```csharp
public class BookUpdateDto
{
    public string? Title { get; set; }
    public List<string>? Authors { get; set; }
    public List<string>? Categories { get; set; }
    public string? Description { get; set; }
    [Range(1900, 2100)] public int? PublishYear { get; set; }
    public IFormFile? File { get; set; }
    public bool? IsAvailable { get; set; }
    public bool ReplaceFile { get; set; } = false;
}
```

#### BookSearchDto
```csharp
public class BookSearchDto
{
    public string? SearchQuery { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Authors { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public bool? IsAvailable { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}
```

## Cấu hình

### 1. Dependencies
```xml
<PackageReference Include="MongoDB.Driver" Version="2.19.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
```

### 2. CDN cho UI
```html
<!-- Select2 CSS -->
<link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css" rel="stylesheet" />

<!-- jQuery và Select2 JS -->
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
```

### 3. JWT Configuration
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ElectronicLibrarySystem",
    "Audience": "ElectronicLibrarySystemUsers",
    "ExpiryMinutes": 60
  }
}
```

## Sử dụng

### 1. Khởi động ứng dụng
```bash
dotnet run
```

### 2. Truy cập Book Management
- URL: `https://localhost:7000/Admin/BookManagement`
- Yêu cầu: Admin role

### 3. Các thao tác chính

#### Thêm sách mới
1. Click "Add New Book"
2. Điền thông tin sách
3. Chọn authors và categories (multi-select)
4. Upload file PDF/EPUB
5. Click "Save Book"

#### Tìm kiếm sách
1. Nhập từ khóa tìm kiếm
2. Chọn authors và categories để lọc
3. Đặt khoảng năm xuất bản
4. Chọn trạng thái available/unavailable
5. Click "Search"

#### Chỉnh sửa sách
1. Click "Edit" trên sách cần sửa
2. Cập nhật thông tin
3. Upload file mới nếu cần (check "Replace file?")
4. Click "Update Book"

#### Xóa sách
1. Click "Delete" trên sách cần xóa
2. Xác nhận xóa
3. Hệ thống sẽ kiểm tra xem sách có đang được mượn không

## Lưu ý

### 1. File Upload
- File được lưu với tên unique: `{Guid}_{original_filename}`
- Metadata được lưu trong FileUpload collection
- Khi xóa sách, file vật lý cũng bị xóa

### 2. Multi-select
- Sử dụng Select2 với theme Bootstrap 5
- Hỗ trợ tìm kiếm trong dropdown
- Có thể clear tất cả selections

### 3. Validation
- Client-side: JavaScript validation cho file type và size
- Server-side: C# validation cho tất cả fields
- File validation: Kiểm tra type và size trước khi lưu

### 4. Security
- JWT authentication cho API endpoints
- Role-based authorization (Admin only cho write operations)
- File type validation để tránh upload malicious files

## Troubleshooting

### 1. File upload không hoạt động
- Kiểm tra quyền ghi vào thư mục `wwwroot/files/books/`
- Kiểm tra file size và type
- Kiểm tra network connection

### 2. Multi-select không hiển thị
- Kiểm tra jQuery và Select2 đã load chưa
- Kiểm tra console errors
- Kiểm tra data từ API

### 3. Search không trả về kết quả
- Kiểm tra MongoDB text index
- Kiểm tra query parameters
- Kiểm tra database connection

## Kết luận

Hệ thống quản lý sách đã được nâng cấp với đầy đủ các tính năng hiện đại:
- Multi-select UI với Select2
- File upload với validation
- Multi-filter search với pagination
- RESTful API với JWT authentication
- Responsive design với Bootstrap

Tất cả các tính năng đều tuân thủ theo các rules đã định trong `library_api_rules.md` và `ui_rules_admin_screens_flow.md`.

