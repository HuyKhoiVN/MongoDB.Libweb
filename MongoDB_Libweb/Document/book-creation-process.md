# Quy Trình Tạo Sách và Mối Liên Hệ Book-Author

## 📚 Tổng Quan Mối Liên Hệ

Trong hệ thống Electronic Library, mối liên hệ giữa **Book** và **Author** được thiết kế theo mô hình **Many-to-Many** thông qua việc sử dụng **Array of Author IDs** trong Book model.

### 🔗 Cấu Trúc Liên Kết

```
Author (1) ←→ (N) Book
```

- **1 Author** có thể viết **nhiều Books**
- **1 Book** có thể có **nhiều Authors** (đồng tác giả)
- Liên kết được thực hiện qua `List<string> Authors` trong Book model
- Mỗi string trong list là **Author ID** (ObjectId của Author)

---

## 🏗️ Cấu Trúc Dữ Liệu

### Book Model
```csharp
public class Book
{
    public string Id { get; set; }                    // Primary Key
    public string Title { get; set; }                 // Tên sách
    public List<string> Authors { get; set; }         // Danh sách Author IDs
    public List<string> Categories { get; set; }      // Danh sách Category IDs
    public string Description { get; set; }           // Mô tả sách
    public int PublishYear { get; set; }              // Năm xuất bản
    public string FileUrl { get; set; }               // URL file sách
    public string FileFormat { get; set; }            // Định dạng file (PDF, EPUB)
    public bool IsAvailable { get; set; }             // Trạng thái khả dụng
    public DateTime CreatedAt { get; set; }           // Thời gian tạo
    public DateTime UpdatedAt { get; set; }           // Thời gian cập nhật
}
```

### Author Model
```csharp
public class Author
{
    public string Id { get; set; }                    // Primary Key
    public string Name { get; set; }                  // Tên tác giả
    public string Bio { get; set; }                   // Tiểu sử tác giả
    public DateTime CreatedAt { get; set; }           // Thời gian tạo
}
```

---

## 🔄 Quy Trình Tạo Sách Chi Tiết

### Bước 1: Chuẩn Bị Dữ Liệu Tác Giả

#### 1.1 Tạo Tác Giả Mới (nếu chưa có)
```http
POST /api/v1/authors
Content-Type: application/json

{
    "name": "Nguyễn Văn A",
    "bio": "Tác giả nổi tiếng với nhiều tác phẩm văn học"
}
```

**Response:**
```json
{
    "success": true,
    "data": {
        "id": "507f1f77bcf86cd799439011",
        "name": "Nguyễn Văn A",
        "bio": "Tác giả nổi tiếng với nhiều tác phẩm văn học",
        "createdAt": "2024-01-15T10:30:00Z"
    },
    "message": "Author created successfully"
}
```

#### 1.2 Lấy Danh Sách Tác Giả Hiện Có
```http
GET /api/v1/authors?page=1&limit=50
```

### Bước 2: Chuẩn Bị Dữ Liệu Danh Mục

#### 2.1 Tạo Danh Mục Mới (nếu cần)
```http
POST /api/v1/categories
Content-Type: application/json

{
    "name": "Văn Học",
    "description": "Các tác phẩm văn học Việt Nam và thế giới"
}
```

#### 2.2 Lấy Danh Sách Danh Mục
```http
GET /api/v1/categories?page=1&limit=50
```

### Bước 3: Tạo Sách

#### 3.1 API Call
```http
POST /api/v1/book
Content-Type: application/json

{
    "title": "Truyện Kiều - Tác Phẩm Kinh Điển",
    "authors": [
        "507f1f77bcf86cd799439011",  // Nguyễn Văn A
        "507f1f77bcf86cd799439012"   // Nguyễn Du (nếu có)
    ],
    "categories": [
        "507f1f77bcf86cd799439021",  // Văn Học
        "507f1f77bcf86cd799439022"   // Kinh Điển
    ],
    "description": "Truyện Kiều là một trong những tác phẩm văn học kinh điển của Việt Nam...",
    "publishYear": 2020,
    "fileUrl": "https://library.com/books/truyen-kieu.pdf",
    "fileFormat": "PDF"
}
```

#### 3.2 Business Logic Xử Lý

**Trong BookService.CreateBookAsync():**

1. **Validation Input Data**
   - Kiểm tra Title không null/empty
   - Kiểm tra Authors list không null/empty
   - Kiểm tra Categories list không null/empty
   - Kiểm tra Description không null/empty
   - Kiểm tra PublishYear trong khoảng 1000-2025
   - Kiểm tra FileUrl và FileFormat không null/empty

2. **Tạo Book Object**
   ```csharp
   var book = new Book
   {
       Title = dto.Title,                    // "Truyện Kiều - Tác Phẩm Kinh Điển"
       Authors = dto.Authors,                // ["507f1f77bcf86cd799439011", "507f1f77bcf86cd799439012"]
       Categories = dto.Categories,          // ["507f1f77bcf86cd799439021", "507f1f77bcf86cd799439022"]
       Description = dto.Description,        // "Truyện Kiều là một trong những..."
       PublishYear = dto.PublishYear,        // 2020
       FileUrl = dto.FileUrl,                // "https://library.com/books/truyen-kieu.pdf"
       FileFormat = dto.FileFormat,          // "PDF"
       IsAvailable = true,                   // Mặc định = true
       CreatedAt = DateTime.UtcNow,          // Tự động set
       UpdatedAt = DateTime.UtcNow           // Tự động set
   };
   ```

3. **Lưu Vào Database**
   - Gọi `_bookRepository.CreateAsync(book)`
   - MongoDB tự động tạo ObjectId cho book
   - Lưu book vào collection "Book"

4. **Trả Về Response**
   ```json
   {
       "success": true,
       "data": {
           "id": "507f1f77bcf86cd799439031",
           "title": "Truyện Kiều - Tác Phẩm Kinh Điển",
           "authors": [
               "507f1f77bcf86cd799439011",
               "507f1f77bcf86cd799439012"
           ],
           "categories": [
               "507f1f77bcf86cd799439021",
               "507f1f77bcf86cd799439022"
           ],
           "description": "Truyện Kiều là một trong những...",
           "publishYear": 2020,
           "fileUrl": "https://library.com/books/truyen-kieu.pdf",
           "fileFormat": "PDF",
           "isAvailable": true,
           "createdAt": "2024-01-15T10:35:00Z",
           "updatedAt": "2024-01-15T10:35:00Z"
       },
       "message": "Book created successfully"
   }
   ```

---

## 🔍 Truy Vấn và Liên Kết Dữ Liệu

### Lấy Sách Theo Tác Giả
```http
GET /api/v1/book/author/{authorId}
```

**Business Logic:**
- Tìm tất cả books có `authorId` trong `Authors` array
- Sử dụng MongoDB query: `{ "authors": { "$in": [authorId] } }`

### Lấy Tác Giả Theo Sách
```http
GET /api/v1/authors/{authorId}
```

**Business Logic:**
- Lấy thông tin chi tiết của author từ Author collection
- Có thể kết hợp với Book collection để lấy danh sách sách của author

### Tìm Kiếm Sách Nâng Cao
```http
POST /api/v1/book/search
Content-Type: application/json

{
    "searchQuery": "truyện kiều",
    "authors": ["507f1f77bcf86cd799439011"],
    "categories": ["507f1f77bcf86cd799439021"],
    "minYear": 2015,
    "maxYear": 2025,
    "isAvailable": true,
    "page": 1,
    "limit": 10
}
```

**Business Logic:**
- Text search trong title, authors, description
- Filter theo authors array
- Filter theo categories array
- Filter theo năm xuất bản
- Filter theo trạng thái available

---

## 📊 Ví Dụ Thực Tế

### Scenario: Tạo Sách Có Nhiều Tác Giả

1. **Tác giả 1:** Nguyễn Du (ID: `507f1f77bcf86cd799439011`)
2. **Tác giả 2:** Nguyễn Văn A (ID: `507f1f77bcf86cd799439012`)
3. **Danh mục:** Văn Học (ID: `507f1f77bcf86cd799439021`)

**Request:**
```json
{
    "title": "Truyện Kiều - Phiên Bản Chú Giải",
    "authors": [
        "507f1f77bcf86cd799439011",  // Nguyễn Du (tác giả gốc)
        "507f1f77bcf86cd799439012"   // Nguyễn Văn A (người chú giải)
    ],
    "categories": ["507f1f77bcf86cd799439021"],
    "description": "Truyện Kiều với phần chú giải chi tiết...",
    "publishYear": 2023,
    "fileUrl": "https://library.com/books/truyen-kieu-chu-giai.pdf",
    "fileFormat": "PDF"
}
```

**Kết quả trong Database:**
```json
{
    "_id": "507f1f77bcf86cd799439031",
    "title": "Truyện Kiều - Phiên Bản Chú Giải",
    "authors": [
        "507f1f77bcf86cd799439011",
        "507f1f77bcf86cd799439012"
    ],
    "categories": ["507f1f77bcf86cd799439021"],
    "description": "Truyện Kiều với phần chú giải chi tiết...",
    "publishYear": 2023,
    "fileUrl": "https://library.com/books/truyen-kieu-chu-giai.pdf",
    "fileFormat": "PDF",
    "isAvailable": true,
    "createdAt": "2024-01-15T10:35:00Z",
    "updatedAt": "2024-01-15T10:35:00Z"
}
```

---

## ⚠️ Lưu Ý Quan Trọng

### 1. Validation
- **Authors array** phải chứa ít nhất 1 author ID
- **Author IDs** phải tồn tại trong Author collection
- **Category IDs** phải tồn tại trong Category collection

### 2. Performance
- Sử dụng **Array Index** cho field `authors` để tối ưu query
- Có thể tạo **Compound Index** cho `authors` và `categories`

### 3. Data Integrity
- Khi xóa Author, cần kiểm tra xem có Book nào đang reference không
- Có thể implement **Cascade Update** khi thay đổi Author ID

### 4. Search Optimization
- Sử dụng **Text Index** cho full-text search
- **Array Index** cho efficient filtering theo authors/categories

---

## 🎯 Kết Luận

Mối liên hệ Book-Author trong hệ thống được thiết kế linh hoạt để hỗ trợ:
- **Nhiều tác giả cho 1 sách** (đồng tác giả)
- **Nhiều sách cho 1 tác giả**
- **Tìm kiếm và lọc hiệu quả**
- **Mở rộng dễ dàng** trong tương lai

Quy trình tạo sách được thiết kế đơn giản nhưng đầy đủ validation và error handling để đảm bảo tính toàn vẹn dữ liệu.
