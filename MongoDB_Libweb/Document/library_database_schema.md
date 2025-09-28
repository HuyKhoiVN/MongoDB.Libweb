# Database Schema for Electronic Library System

Database Name: **libDB**

This document describes the MongoDB collections for the Electronic Library System. Each collection is listed with its purpose, document structure, and indexes. No sample data is included.

## Collection: User
- **Purpose**: Stores user information (Admin/User) for account management and authentication.
- **Document Structure**:
  ```json
  {
    "_id": ObjectId,
    "username": String,
    "passwordHash": String,
    "email": String,
    "fullName": String,
    "role": String, // "Admin" or "User"
    "createdAt": ISODate,
    "updatedAt": ISODate,
    "isActive": Boolean
  }
  ```
- **Indexes**:
  - `{ username: 1 }`, unique: true
  - `{ email: 1 }`, unique: true

## Collection: Book
- **Purpose**: Stores metadata and file path for electronic books.
- **Document Structure**:
  ```json
  {
    "_id": ObjectId,
    "title": String,
    "authors": [String],
    "categories": [String],
    "description": String,
    "publishYear": Number,
    "fileUrl": String,
    "fileFormat": String, // "PDF", "EPUB"
    "isAvailable": Boolean,
    "createdAt": ISODate,
    "updatedAt": ISODate
  }
  ```
- **Indexes**:
  - `{ title: "text", authors: "text", description: "text" }` (text index for full-text search)
  - `{ categories: 1 }`

## Collection: Borrow
- **Purpose**: Tracks book borrowing/returning history and status.
- **Document Structure**:
  ```json
  {
    "_id": ObjectId,
    "userId": ObjectId,
    "bookId": ObjectId,
    "borrowDate": ISODate,
    "dueDate": ISODate,
    "returnDate": ISODate,
    "status": String // "Borrowed", "Returned", "Overdue"
  }
  ```
- **Indexes**:
  - `{ userId: 1 }`
  - `{ bookId: 1 }`
  - `{ status: 1, dueDate: 1 }`

## Collection: Category
- **Purpose**: Stores book categories for classification.
- **Document Structure**:
  ```json
  {
    "_id": ObjectId,
    "name": String,
    "description": String,
    "createdAt": ISODate
  }
  ```
- **Indexes**:
  - `{ name: 1 }`, unique: true

## Collection: Author
- **Purpose**: Stores author information linked to books.
- **Document Structure**:
  ```json
  {
    "_id": ObjectId,
    "name": String,
    "bio": String,
    "createdAt": ISODate
  }
  ```
- **Indexes**:
  - `{ name: 1 }`, unique: true

## Collection: FileUpload
- **Purpose**: Stores metadata for book files (replacing GridFS).
- **Document Structure**:
  ```json
  {
    "_id": ObjectId,
    "filename": String,
    "length": Number,
    "uploadDate": ISODate,
    "bookId": ObjectId
  }
  ```
- **Indexes**:
  - `{ bookId: 1 }`

## Notes
- **Database**: MongoDB, with collections named in singular form (User, Book, etc.).
- **File Storage**: File collection stores metadata; actual files can be stored on server with fileUrl in Book collection or use MongoDB GridFS if needed.
- **Indexes**: Designed for fast queries, unique constraints, and full-text search (Book collection).
- **References**: Use ObjectId for linking (e.g., userId, bookId).

Generated on: September 28, 2025