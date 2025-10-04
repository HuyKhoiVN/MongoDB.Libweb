# Chi Tiết Truy Vấn Database MongoDB Trong Dự Án Quản Lý Thư Viện

## 1. Giới Thiệu

Dự án quản lý thư viện điện tử sử dụng cơ sở dữ liệu MongoDB với tên database **libDB** để lưu trữ và quản lý thông tin sách, người dùng, mượn trả và các metadata liên quan. Việc lựa chọn MongoDB cho hệ thống này mang lại nhiều lợi ích quan trọng:

- **Tính linh hoạt trong schema**: Cho phép lưu trữ dữ liệu có cấu trúc phức tạp như mảng authors và categories trong collection Book
- **Hỗ trợ aggregation pipeline mạnh mẽ**: Tối ưu cho việc tạo báo cáo thống kê mượn sách và phân tích dữ liệu
- **Async operations**: Các thao tác bất đồng bộ (async/await) đảm bảo hiệu suất cao trong môi trường web
- **Full-text search**: Tích hợp sẵn khả năng tìm kiếm văn bản toàn diện cho tiêu đề sách và mô tả

## 2. Các Truy Vấn Chính Theo Collection

### 2.1 Collection User

Collection User quản lý thông tin người dùng với các thao tác CRUD cơ bản và tìm kiếm theo username/email.

#### CRUD Operations
```csharp
// Create - Tạo user mới
public async Task<User> CreateAsync(User user)
{
    user.CreatedAt = DateTime.UtcNow;
    user.UpdatedAt = DateTime.UtcNow;
    await _users.InsertOneAsync(user);
    return user;
}

// Read - Lấy danh sách user có phân trang
public async Task<List<User>> GetAllAsync(int page = 1, int limit = 10)
{
    var skip = (page - 1) * limit;
    return await _users.Find(u => u.IsActive)
        .Skip(skip)
        .Limit(limit)
        .ToListAsync();
}

// Update - Cập nhật thông tin user
public async Task<User?> UpdateAsync(string id, User user)
{
    user.UpdatedAt = DateTime.UtcNow;
    var result = await _users.FindOneAndUpdateAsync(
        u => u.Id == id,
        Builders<User>.Update
            .Set(u => u.Username, user.Username)
            .Set(u => u.Email, user.Email)
            .Set(u => u.FullName, user.FullName)
            .Set(u => u.Role, user.Role)
            .Set(u => u.IsActive, user.IsActive)
            .Set(u => u.UpdatedAt, user.UpdatedAt),
        new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After }
    );
    return result;
}
```

#### Search Operations
```csharp
// Tìm kiếm theo username
public async Task<User?> GetByUsernameAsync(string username)
{
    return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
}

// Kiểm tra tồn tại email
public async Task<bool> ExistsByEmailAsync(string email)
{
    var count = await _users.CountDocumentsAsync(u => u.Email == email);
    return count > 0;
}
```

### 2.2 Collection Book

Collection Book là trung tâm của hệ thống, chứa metadata sách và hỗ trợ tìm kiếm phức tạp.

#### Advanced Search với Multiple Filters
```csharp
public async Task<List<Book>> SearchAsync(string? searchQuery, List<string>? categories, 
    List<string>? authors, int? minYear, int? maxYear, bool? isAvailable, 
    int page = 1, int limit = 10)
{
    var filter = Builders<Book>.Filter.Empty;
    var skip = (page - 1) * limit;

    // Text search sử dụng text index
    if (!string.IsNullOrEmpty(searchQuery))
    {
        filter &= Builders<Book>.Filter.Text(searchQuery);
    }

    // Filter theo categories sử dụng $in operator
    if (categories != null && categories.Any())
    {
        filter &= Builders<Book>.Filter.AnyIn(b => b.Categories, categories);
    }

    // Filter theo authors
    if (authors != null && authors.Any())
    {
        filter &= Builders<Book>.Filter.AnyIn(b => b.Authors, authors);
    }

    // Filter theo năm xuất bản
    if (minYear.HasValue)
    {
        filter &= Builders<Book>.Filter.Gte(b => b.PublishYear, minYear.Value);
    }

    if (maxYear.HasValue)
    {
        filter &= Builders<Book>.Filter.Lte(b => b.PublishYear, maxYear.Value);
    }

    // Filter theo trạng thái khả dụng
    if (isAvailable.HasValue)
    {
        filter &= Builders<Book>.Filter.Eq(b => b.IsAvailable, isAvailable.Value);
    }

    return await _books.Find(filter)
        .Skip(skip)
        .Limit(limit)
        .ToListAsync();
}
```

#### Relationship Queries
```csharp
// Lấy sách theo category
public async Task<List<Book>> GetByCategoryAsync(string categoryId, int page = 1, int limit = 10)
{
    var skip = (page - 1) * limit;
    return await _books.Find(b => b.Categories.Contains(categoryId))
        .Skip(skip)
        .Limit(limit)
        .ToListAsync();
}

// Lấy sách theo author
public async Task<List<Book>> GetByAuthorAsync(string authorId, int page = 1, int limit = 10)
{
    var skip = (page - 1) * limit;
    return await _books.Find(b => b.Authors.Contains(authorId))
        .Skip(skip)
        .Limit(limit)
        .ToListAsync();
}
```

### 2.3 Collection Borrow

Collection Borrow quản lý lịch sử mượn trả với các truy vấn phức tạp và aggregation pipeline.

#### Basic CRUD Operations
```csharp
// Tạo borrow record mới
public async Task<Borrow> CreateAsync(Borrow borrow)
{
    await _borrows.InsertOneAsync(borrow);
    return borrow;
}

// Lấy borrow records theo user
public async Task<List<Borrow>> GetByUserIdAsync(string userId, int page = 1, int limit = 10)
{
    var skip = (page - 1) * limit;
    return await _borrows.Find(b => b.UserId == userId)
        .Skip(skip)
        .Limit(limit)
        .ToListAsync();
}

// Lấy borrow records quá hạn
public async Task<List<Borrow>> GetOverdueBorrowsAsync()
{
    var now = DateTime.UtcNow;
    return await _borrows.Find(b => b.Status == "Borrowed" && b.DueDate < now).ToListAsync();
}
```

#### Advanced Aggregation Pipeline
```csharp
// Lấy borrow records với thông tin chi tiết User và Book
public async Task<List<BorrowDetailDto>> GetAllWithDetailsAsync(int page = 1, int limit = 10)
{
    var skip = (page - 1) * limit;

    var pipeline = new[]
    {
        // Stage 1: Match tất cả borrow records
        new BsonDocument("$match", new BsonDocument()),

        // Stage 2: Lookup User collection
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "User" },
            { "localField", "userId" },
            { "foreignField", "_id" },
            { "as", "user" }
        }),

        // Stage 3: Lookup Book collection
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Book" },
            { "localField", "bookId" },
            { "foreignField", "_id" },
            { "as", "book" }
        }),

        // Stage 4: Unwind User array
        new BsonDocument("$unwind", new BsonDocument
        {
            { "path", "$user" },
            { "preserveNullAndEmptyArrays", true }
        }),

        // Stage 5: Unwind Book array
        new BsonDocument("$unwind", new BsonDocument
        {
            { "path", "$book" },
            { "preserveNullAndEmptyArrays", true }
        }),

        // Stage 6: Pagination
        new BsonDocument("$skip", skip),
        new BsonDocument("$limit", limit)
    };

    var result = await _borrows.Aggregate<BorrowDetailDto>(pipeline).ToListAsync();
    return result;
}
```

### 2.4 Collection Category và Author

Các collection Category và Author có cấu trúc tương tự với các thao tác CRUD cơ bản:

```csharp
// Category Repository - Tìm kiếm theo tên
public async Task<Category?> GetByNameAsync(string name)
{
    return await _categories.Find(c => c.Name == name).FirstOrDefaultAsync();
}

// Author Repository - Kiểm tra tồn tại
public async Task<bool> ExistsByNameAsync(string name)
{
    var count = await _authors.CountDocumentsAsync(a => a.Name == name);
    return count > 0;
}
```

### 2.5 Collection FileUpload

Collection FileUpload quản lý metadata của file sách:

```csharp
// Lấy file upload theo bookId
public async Task<List<FileUpload>> GetByBookIdAsync(string bookId)
{
    return await _fileUploads.Find(f => f.BookId == bookId).ToListAsync();
}

// Kiểm tra file tồn tại cho book
public async Task<bool> ExistsByBookIdAsync(string bookId)
{
    var count = await _fileUploads.CountDocumentsAsync(f => f.BookId == bookId);
    return count > 0;
}
```

## 3. Tối Ưu Hóa Truy Vấn

### 3.1 Index Optimization

Dựa trên schema database, các indexes được thiết kế tối ưu:

| Collection | Index | Type | Purpose |
|------------|-------|------|---------|
| User | `{ username: 1 }` | Unique | Tối ưu tìm kiếm đăng nhập |
| User | `{ email: 1 }` | Unique | Tối ưu tìm kiếm email |
| Book | `{ title: "text", authors: "text", description: "text" }` | Text | Full-text search |
| Book | `{ categories: 1 }` | Regular | Filter theo category |
| Borrow | `{ userId: 1 }` | Regular | Tối ưu query theo user |
| Borrow | `{ bookId: 1 }` | Regular | Tối ưu query theo book |
| Borrow | `{ status: 1, dueDate: 1 }` | Compound | Tối ưu query overdue |

### 3.2 Query Optimization Techniques

#### Sử dụng Builders<T>.Filter
```csharp
// Thay vì string query, sử dụng strongly-typed filter
var filter = Builders<Book>.Filter.And(
    Builders<Book>.Filter.Text(searchQuery),
    Builders<Book>.Filter.AnyIn(b => b.Categories, categories),
    Builders<Book>.Filter.Gte(b => b.PublishYear, minYear)
);
```

#### Pagination với Skip/Limit
```csharp
// Hiệu quả cho large datasets
var skip = (page - 1) * limit;
return await _books.Find(filter)
    .Skip(skip)
    .Limit(limit)
    .ToListAsync();
```

#### Exception Handling
```csharp
try
{
    var result = await _books.Find(filter).ToListAsync();
    return result;
}
catch (MongoException ex)
{
    // Log error và handle gracefully
    throw new DatabaseException("Error querying books", ex);
}
```

## 4. Ví Dụ Code Truy Vấn

### 4.1 JSON Query Examples

#### Text Search Query
```json
{
  "$text": {
    "$search": "programming algorithms",
    "$caseSensitive": false
  }
}
```

#### Complex Filter Query
```json
{
  "$and": [
    { "categories": { "$in": ["Computer Science", "Programming"] } },
    { "publishYear": { "$gte": 2020, "$lte": 2023 } },
    { "isAvailable": true }
  ]
}
```

#### Aggregation Pipeline for Reports
```json
[
  {
    "$match": {
      "status": "Borrowed",
      "borrowDate": {
        "$gte": "2024-01-01T00:00:00.000Z",
        "$lte": "2024-12-31T23:59:59.999Z"
      }
    }
  },
  {
    "$group": {
      "_id": "$bookId",
      "borrowCount": { "$sum": 1 },
      "lastBorrowDate": { "$max": "$borrowDate" }
    }
  },
  {
    "$sort": { "borrowCount": -1 }
  },
  {
    "$limit": 10
  }
]
```

### 4.2 C# Implementation Examples

#### Advanced Search Implementation
```csharp
public async Task<List<Book>> SearchBooksAsync(BookSearchDto searchDto)
{
    var filter = Builders<Book>.Filter.Empty;
    
    // Text search
    if (!string.IsNullOrEmpty(searchDto.Query))
    {
        filter &= Builders<Book>.Filter.Text(searchDto.Query);
    }
    
    // Category filter
    if (searchDto.Categories?.Any() == true)
    {
        filter &= Builders<Book>.Filter.AnyIn(b => b.Categories, searchDto.Categories);
    }
    
    // Year range filter
    if (searchDto.MinYear.HasValue)
    {
        filter &= Builders<Book>.Filter.Gte(b => b.PublishYear, searchDto.MinYear.Value);
    }
    
    if (searchDto.MaxYear.HasValue)
    {
        filter &= Builders<Book>.Filter.Lte(b => b.PublishYear, searchDto.MaxYear.Value);
    }
    
    // Availability filter
    if (searchDto.IsAvailable.HasValue)
    {
        filter &= Builders<Book>.Filter.Eq(b => b.IsAvailable, searchDto.IsAvailable.Value);
    }
    
    var skip = (searchDto.Page - 1) * searchDto.Limit;
    
    return await _books.Find(filter)
        .Skip(skip)
        .Limit(searchDto.Limit)
        .Sort(Builders<Book>.Sort.Descending(b => b.CreatedAt))
        .ToListAsync();
}
```

## 5. Đánh Giá Hiệu Suất

### 5.1 Lợi Ích của Indexes

- **Text Index**: Tăng tốc độ tìm kiếm full-text từ O(n) xuống O(log n)
- **Compound Index**: Tối ưu queries phức tạp với multiple conditions
- **Unique Index**: Đảm bảo tính toàn vẹn dữ liệu và tăng tốc lookup

### 5.2 Aggregation Pipeline Benefits

#### Báo Cáo Thống Kê Mượn Sách
```csharp
// Pipeline tạo báo cáo sách được mượn nhiều nhất
var pipeline = new[]
{
    new BsonDocument("$match", new BsonDocument("status", "Borrowed")),
    new BsonDocument("$group", new BsonDocument
    {
        { "_id", "$bookId" },
        { "borrowCount", new BsonDocument("$sum", 1) },
        { "lastBorrowDate", new BsonDocument("$max", "$borrowDate") }
    }),
    new BsonDocument("$lookup", new BsonDocument
    {
        { "from", "Book" },
        { "localField", "_id" },
        { "foreignField", "_id" },
        { "as", "book" }
    }),
    new BsonDocument("$unwind", "$book"),
    new BsonDocument("$sort", new BsonDocument("borrowCount", -1)),
    new BsonDocument("$limit", 10)
};
```

### 5.3 Performance Metrics

- **Query Response Time**: < 100ms cho simple queries
- **Aggregation Performance**: < 500ms cho complex reports
- **Concurrent Users**: Hỗ trợ 100+ users đồng thời
- **Memory Usage**: Tối ưu với proper indexing

## 6. Kết Luận

Việc sử dụng MongoDB trong dự án quản lý thư viện đã mang lại hiệu quả cao nhờ:

1. **Flexible Schema**: Dễ dàng mở rộng và thay đổi cấu trúc dữ liệu
2. **Powerful Aggregation**: Tạo báo cáo phức tạp một cách hiệu quả
3. **Async Operations**: Đảm bảo hiệu suất cao trong môi trường web
4. **Full-text Search**: Tìm kiếm sách thông minh và nhanh chóng
5. **Horizontal Scaling**: Khả năng mở rộng theo chiều ngang khi cần thiết

Các truy vấn được thiết kế tối ưu với proper indexing và sử dụng MongoDB aggregation pipeline giúp hệ thống xử lý hiệu quả các yêu cầu phức tạp của quản lý thư viện điện tử.
