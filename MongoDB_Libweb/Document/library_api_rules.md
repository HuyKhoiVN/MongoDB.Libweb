# Rules for RESTful API, Repository, Service, and CRUD Operations in Electronic Library System

This document defines the rules to ensure consistency in implementing RESTful APIs, Repository, Service layers, and CRUD operations for the **Electronic Library System** using ASP.NET MVC (.NET 8.0) and MongoDB. The goal is to maintain clean, scalable, and maintainable code.

Generated on: September 28, 2025

## 1. RESTful API Rules

### 1.1 API Design Principles
- Follow **RESTful conventions** for resource-based endpoints.
- Use **HTTP methods** consistently:
  - **GET**: Retrieve resources.
  - **POST**: Create new resources.
  - **PUT**: Update existing resources (full update).
  - **PATCH**: Partial update of resources.
  - **DELETE**: Remove resources.
- Return appropriate **HTTP status codes**:
  - `200 OK`: Successful GET, PUT, PATCH.
  - `201 Created`: Successful POST.
  - `204 No Content`: Successful DELETE.
  - `400 Bad Request`: Invalid input.
  - `401 Unauthorized`: Authentication required.
  - `403 Forbidden`: Insufficient permissions.
  - `404 Not Found`: Resource not found.
  - `500 Internal Server Error`: Server-side errors.
- Use **JSON** for request and response payloads.
- Implement **pagination** for list endpoints (e.g., `GET /api/books?page=1&limit=10`).
- Include **versioning** in the API path (e.g., `/api/v1/`).

### 1.2 Naming Conventions
- **Resource names**: Use lowercase, plural nouns (e.g., `/api/v1/users`, `/api/v1/book`).
- **Nested resources**: Use sub-paths for relationships (e.g., `/api/v1/users/{userId}/borrows`).
- **Query parameters**: Use camelCase (e.g., `page`, `limit`, `searchQuery`).
- **Route examples**:
  - `GET /api/v1/book`: List all books with pagination.
  - `GET /api/v1/book/{id}`: Get a specific book.
  - `POST /api/v1/book`: Create a new book.
  - `PUT /api/v1/book/{id}`: Update a book.
  - `DELETE /api/v1/book/{id}`: Delete a book.

### 1.3 Response Format
- Use a consistent JSON response structure:
  ```json
  {
    "success": Boolean,
    "data": Object | Array | null,
    "message": String | null,
    "error": String | null
  }
  ```
- Example for successful response:
  ```json
  {
    "success": true,
    "data": { "id": "123", "title": "Sample Book" },
    "message": "Book retrieved successfully"
  }
  ```
- Example for error response:
  ```json
  {
    "success": false,
    "data": null,
    "error": "Book not found"
  }
  ```

### 1.4 Authentication and Authorization
- Use **JWT (JSON Web Tokens)** for authentication.
- Include `Authorization: Bearer <token>` in headers for protected endpoints.
- Restrict endpoints by role:
  - Admin: Access to all CRUD operations (e.g., create/update/delete books).
  - User: Limited to read (GET) and borrow-related actions.
  - Guest: Only public endpoints (e.g., `GET /api/v1/book`).
- Validate roles in controllers using ` ` or custom middleware.

## 2. Repository Pattern Rules

### 2.1 Purpose
- **Repository** acts as an abstraction layer between the data access layer (MongoDB) and business logic (Service).
- Encapsulates MongoDB queries to ensure reusability and testability.

### 2.2 Structure
- Create one repository per collection (e.g., `UserRepository`, `BookRepository`).
- Define an interface for each repository (e.g., `IUserRepository`).
- Place repositories in a `Repositories` folder.

### 2.3 Naming Conventions
- Interface: `I<Entity>Repository` (e.g., `IUserRepository`).
- Implementation: `<Entity>Repository` (e.g., `UserRepository`).
- Methods: Use clear, action-based names:
  - `GetAllAsync`: Retrieve all entities (with optional filters/pagination).
  - `GetByIdAsync`: Retrieve entity by ID.
  - `CreateAsync`: Insert a new entity.
  - `UpdateAsync`: Update an existing entity.
  - `DeleteAsync`: Delete an entity by ID.

### 2.4 Implementation Rules
- Use `MongoDB.Driver` for queries.
- Inject `MongoDbContext` (from previous setup) into repositories.
- Use async/await for all database operations.
- Example interface (`IUserRepository.cs`):
  ```csharp
  public interface IUserRepository
  {
      Task<List<User>> GetAllAsync(int page, int limit);
      Task<User> GetByIdAsync(string id);
      Task CreateAsync(User user);
      Task UpdateAsync(string id, User user);
      Task DeleteAsync(string id);
  }
  ```
- Handle MongoDB exceptions (e.g., `MongoException`) and throw custom exceptions if needed.
- Use MongoDB filters (`Builders<T>.Filter`) for queries.

## 3. Service Layer Rules

### 3.1 Purpose
- **Service** layer handles business logic, validation, and coordination between repositories and controllers.
- Keeps controllers thin by offloading complex logic.

### 3.2 Structure
- Create one service per entity or feature (e.g., `UserService`, `BookService`).
- Define an interface for each service (e.g., `IUserService`).
- Place services in a `Services` folder.

### 3.3 Naming Conventions
- Interface: `I<Entity>Service` (e.g., `IUserService`).
- Implementation: `<Entity>Service` (e.g., `UserService`).
- Methods: Reflect business operations (e.g., `RegisterUserAsync`, `BorrowBookAsync`).

### 3.4 Implementation Rules
- Inject repositories into services via constructor.
- Validate input data before passing to repository.
- Handle business rules (e.g., check if a book is available before borrowing).
- Example service (`IUserService.cs`):
  ```csharp
  public interface IUserService
  {
      Task<User> RegisterUserAsync(UserRegisterDto dto);
      Task<User> GetUserByIdAsync(string id);
      Task UpdateUserAsync(string id, UserUpdateDto dto);
  }
  ```
- Use DTOs (Data Transfer Objects) for input/output to avoid exposing internal models.
- Return meaningful errors for invalid operations (e.g., duplicate username).

## 4. CRUD Operations with MongoDB

### 4.1 General Rules
- Use `MongoDB.Driver` methods for CRUD:
  - **Create**: `InsertOneAsync`
  - **Read**: `FindAsync`, `FindOneAsync`
  - **Update**: `FindOneAndUpdateAsync`, `UpdateOneAsync`
  - **Delete**: `FindOneAndDeleteAsync`, `DeleteOneAsync`
- Always use async/await for database operations.
- Use `ObjectId` for `_id` fields and validate format.
- Apply filters using `Builders<T>.Filter` for precise queries.
- Ensure indexes (as defined in database schema) are used effectively.

### 4.2 CRUD Operation Guidelines
- **Create**:
  - Validate input data in Service layer.
  - Set `createdAt` and `updatedAt` timestamps in Service or Repository.
  - Example: `await collection.InsertOneAsync(document);`
- **Read**:
  - Support pagination for list operations (use `Skip` and `Limit`).
  - Use text indexes for search (e.g., `Builders<Book>.Filter.Text(searchQuery)`).
  - Example: `await collection.Find(filter).Skip(page * limit).Limit(limit).ToListAsync();`
- **Update**:
  - Use `FindOneAndUpdateAsync` for atomic updates.
  - Update `updatedAt` timestamp.
  - Example: `await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions { ReturnDocument = ReturnDocument.After });`
- **Delete**:
  - Soft delete preferred (set `isActive: false` for users, or check borrow status for books).
  - Example: `await collection.DeleteOneAsync(filter);`

### 4.3 Collection-Specific Rules
- **User**:
  - Hash passwords using `BCrypt.Net` before storing.
  - Validate unique `username` and `email` (handled by indexes).
- **Book**:
  - Validate `fileUrl` and `fileFormat` before saving.
  - Check `isAvailable` before allowing borrows.
- **Borrow**:
  - Update `status` to "Overdue" if `dueDate` < current date (use cron job or service logic).
  - Prevent deletion of books if active borrows exist.
- **Category/Author**:
  - Ensure unique `name` (handled by indexes).
  - Use for filtering books in queries.
- **File**:
  - Store metadata only; actual files stored on server or GridFS (if needed).
  - Link to `Book` via `bookId`.

## 5. Additional Notes
- **Security**:
  - Use HTTPS for all API endpoints.
  - Sanitize inputs to prevent injection (MongoDB.Driver handles most cases).
  - Implement rate limiting for public endpoints.
- **Error Handling**:
  - Wrap MongoDB operations in try-catch to handle `MongoException`.
  - Return meaningful error messages in API responses.
- **Performance**:
  - Use indexes for frequent queries (already defined in schema).
  - Cache static data (e.g., categories) using `MemoryCache`.
  - Limit large file uploads (e.g., book files) to avoid performance issues.
- **Testing**:
  - Write unit tests for Services using Moq or in-memory MongoDB.
  - Test APIs with tools like Postman or Swagger (add Swashbuckle.AspNetCore for API documentation).
- **Scalability**:
  - Use dependency injection for repositories and services.
  - Design for horizontal scaling (MongoDB replica sets, load-balanced ASP.NET).

This document ensures consistency across the development of the Electronic Library System. Adhere to these rules to maintain clean code and efficient database interactions.