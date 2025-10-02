using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;
using MongoDB_Libweb.DTOs;

namespace MongoDB_Libweb.Repositories
{
    public class BorrowRepository : IBorrowRepository
    {
        private readonly IMongoCollection<Borrow> _borrows;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Book> _books;

        public BorrowRepository(MongoDbContext context)
        {
            _borrows = context.Borrows;
            _users = context.Users;
            _books = context.Books;
        }

        public async Task<List<Borrow>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _borrows.Find(_ => true)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Borrow?> GetByIdAsync(string id)
        {
            return await _borrows.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Borrow>> GetByUserIdAsync(string userId, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _borrows.Find(b => b.UserId == userId)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<Borrow>> GetByBookIdAsync(string bookId, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _borrows.Find(b => b.BookId == bookId)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<Borrow>> GetByStatusAsync(string status, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _borrows.Find(b => b.Status == status)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<Borrow>> GetOverdueBorrowsAsync()
        {
            var now = DateTime.UtcNow;
            return await _borrows.Find(b => b.Status == "Borrowed" && b.DueDate < now).ToListAsync();
        }

        public async Task<Borrow> CreateAsync(Borrow borrow)
        {
            await _borrows.InsertOneAsync(borrow);
            return borrow;
        }

        public async Task<Borrow?> UpdateAsync(string id, Borrow borrow)
        {
            var result = await _borrows.FindOneAndUpdateAsync(
                b => b.Id == id,
                Builders<Borrow>.Update
                    .Set(b => b.UserId, borrow.UserId)
                    .Set(b => b.BookId, borrow.BookId)
                    .Set(b => b.BorrowDate, borrow.BorrowDate)
                    .Set(b => b.DueDate, borrow.DueDate)
                    .Set(b => b.ReturnDate, borrow.ReturnDate)
                    .Set(b => b.Status, borrow.Status),
                new FindOneAndUpdateOptions<Borrow> { ReturnDocument = ReturnDocument.After }
            );
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _borrows.DeleteOneAsync(b => b.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _borrows.CountDocumentsAsync(_ => true);
        }

        public async Task<long> GetActiveBorrowsCountAsync()
        {
            return await _borrows.CountDocumentsAsync(b => b.Status == "Borrowed");
        }

        public async Task<long> GetOverdueBorrowsCountAsync()
        {
            var now = DateTime.UtcNow;
            return await _borrows.CountDocumentsAsync(b => b.Status == "Borrowed" && b.DueDate < now);
        }

        public async Task<List<Borrow>> GetBorrowsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _borrows.Find(b => b.BorrowDate >= startDate && b.BorrowDate <= endDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveBorrowAsync(string userId, string bookId)
        {
            var count = await _borrows.CountDocumentsAsync(b => b.UserId == userId && b.BookId == bookId && b.Status == "Borrowed");
            return count > 0;
        }

        public async Task<List<Borrow>> GetActiveBorrowsByUserAsync(string userId)
        {
            return await _borrows.Find(b => b.UserId == userId && b.Status == "Borrowed").ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả borrow records với thông tin chi tiết User và Book trong một lần query
        /// Sử dụng MongoDB aggregation pipeline để join dữ liệu từ 3 collections
        /// </summary>
        /// <param name="page">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="limit">Số lượng records mỗi trang</param>
        /// <returns>Danh sách BorrowDetailDto với thông tin đầy đủ</returns>
        public async Task<List<BorrowDetailDto>> GetAllWithDetailsAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;

            /*
             * MongoDB Aggregation Pipeline Query:
             * 
             * [
             *   // Stage 1: Match tất cả borrow records
             *   { $match: {} },
             *   
             *   // Stage 2: Convert userId string thành ObjectId để join với users collection
             *   { $addFields: { 
             *       userIdObjectId: { $toObjectId: "$userId" } 
             *   }},
             *   
             *   // Stage 3: Convert bookId string thành ObjectId để join với books collection  
             *   { $addFields: { 
             *       bookIdObjectId: { $toObjectId: "$bookId" } 
             *   }},
             *   
             *   // Stage 4: Lookup User collection sử dụng ObjectId
             *   { $lookup: {
             *       from: "users",
             *       localField: "userIdObjectId", 
             *       foreignField: "_id",
             *       as: "user"
             *   }},
             *   
             *   // Stage 5: Lookup Book collection sử dụng ObjectId
             *   { $lookup: {
             *       from: "books",
             *       localField: "bookIdObjectId",
             *       foreignField: "_id", 
             *       as: "book"
             *   }},
             *   
             *   // Stage 6: Unwind User array (giữ lại records không có user)
             *   { $unwind: { 
             *       path: "$user", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 7: Unwind Book array (giữ lại records không có book)
             *   { $unwind: { 
             *       path: "$book", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 8: Remove temporary ObjectId fields
             *   { $unset: ["userIdObjectId", "bookIdObjectId"] },
             *   
             *   // Stage 9: Skip và Limit cho phân trang
             *   { $skip: skip },
             *   { $limit: limit }
             * ]
             */

            var pipeline = new[]
            {
                // Stage 1: Match tất cả borrow records
                new BsonDocument("$match", new BsonDocument()),
                
                // Stage 2: Convert userId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "userIdObjectId", new BsonDocument("$toObjectId", "$userId") }
                }),
                
                // Stage 3: Convert bookId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "bookIdObjectId", new BsonDocument("$toObjectId", "$bookId") }
                }),
                
                // Stage 4: Lookup User collection sử dụng ObjectId
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "user" }
                }),
                
                // Stage 5: Lookup Book collection sử dụng ObjectId
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "books" },
                    { "localField", "bookIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "book" }
                }),
                
                // Stage 6: Unwind User array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$user" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 7: Unwind Book array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$book" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 8: Remove temporary ObjectId fields
                new BsonDocument("$unset", new BsonArray { "userIdObjectId", "bookIdObjectId" }),
                
                // Stage 9: Skip và Limit cho phân trang
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", limit)
            };

            var result = await _borrows.Aggregate<BorrowDetailDto>(pipeline).ToListAsync();
            return result;
        }

        /// <summary>
        /// Lấy borrow records theo user với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu
        /// </summary>
        public async Task<List<BorrowDetailDto>> GetByUserIdWithDetailsAsync(string userId, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;

            /*
             * MongoDB Aggregation Pipeline Query:
             * 
             * [
             *   // Stage 1: Match borrow records của user cụ thể (userId là string)
             *   { $match: { userId: "userIdString" } },
             *   
             *   // Stage 2: Convert userId string thành ObjectId
             *   { $addFields: { 
             *       userIdObjectId: { $toObjectId: "$userId" } 
             *   }},
             *   
             *   // Stage 3: Convert bookId string thành ObjectId
             *   { $addFields: { 
             *       bookIdObjectId: { $toObjectId: "$bookId" } 
             *   }},
             *   
             *   // Stage 4: Lookup User collection
             *   { $lookup: {
             *       from: "users",
             *       localField: "userIdObjectId", 
             *       foreignField: "_id",
             *       as: "user"
             *   }},
             *   
             *   // Stage 5: Lookup Book collection
             *   { $lookup: {
             *       from: "books",
             *       localField: "bookIdObjectId",
             *       foreignField: "_id", 
             *       as: "book"
             *   }},
             *   
             *   // Stage 6: Unwind User array
             *   { $unwind: { 
             *       path: "$user", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 7: Unwind Book array
             *   { $unwind: { 
             *       path: "$book", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 8: Remove temporary ObjectId fields
             *   { $unset: ["userIdObjectId", "bookIdObjectId"] },
             *   
             *   // Stage 9: Skip và Limit
             *   { $skip: skip },
             *   { $limit: limit }
             * ]
             */

            var pipeline = new[]
            {
                // Stage 1: Match borrow records của user cụ thể
                new BsonDocument("$match", new BsonDocument("userId", userId)),
                
                // Stage 2: Convert userId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "userIdObjectId", new BsonDocument("$toObjectId", "$userId") }
                }),
                
                // Stage 3: Convert bookId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "bookIdObjectId", new BsonDocument("$toObjectId", "$bookId") }
                }),
                
                // Stage 4: Lookup User collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "user" }
                }),
                
                // Stage 5: Lookup Book collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "books" },
                    { "localField", "bookIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "book" }
                }),
                
                // Stage 6: Unwind User array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$user" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 7: Unwind Book array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$book" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 8: Remove temporary ObjectId fields
                new BsonDocument("$unset", new BsonArray { "userIdObjectId", "bookIdObjectId" }),
                
                // Stage 9: Skip và Limit
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", limit)
            };

            var result = await _borrows.Aggregate<BorrowDetailDto>(pipeline).ToListAsync();
            return result;
        }

        /// <summary>
        /// Lấy borrow records theo book với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu
        /// </summary>
        public async Task<List<BorrowDetailDto>> GetByBookIdWithDetailsAsync(string bookId, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;

            /*
             * MongoDB Aggregation Pipeline Query:
             * 
             * [
             *   // Stage 1: Match borrow records của book cụ thể (bookId là string)
             *   { $match: { bookId: "bookIdString" } },
             *   
             *   // Stage 2: Convert userId string thành ObjectId
             *   { $addFields: { 
             *       userIdObjectId: { $toObjectId: "$userId" } 
             *   }},
             *   
             *   // Stage 3: Convert bookId string thành ObjectId
             *   { $addFields: { 
             *       bookIdObjectId: { $toObjectId: "$bookId" } 
             *   }},
             *   
             *   // Stage 4: Lookup User collection
             *   { $lookup: {
             *       from: "users",
             *       localField: "userIdObjectId", 
             *       foreignField: "_id",
             *       as: "user"
             *   }},
             *   
             *   // Stage 5: Lookup Book collection
             *   { $lookup: {
             *       from: "books",
             *       localField: "bookIdObjectId",
             *       foreignField: "_id", 
             *       as: "book"
             *   }},
             *   
             *   // Stage 6: Unwind User array
             *   { $unwind: { 
             *       path: "$user", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 7: Unwind Book array
             *   { $unwind: { 
             *       path: "$book", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 8: Remove temporary ObjectId fields
             *   { $unset: ["userIdObjectId", "bookIdObjectId"] },
             *   
             *   // Stage 9: Skip và Limit
             *   { $skip: skip },
             *   { $limit: limit }
             * ]
             */

            var pipeline = new[]
            {
                // Stage 1: Match borrow records của book cụ thể
                new BsonDocument("$match", new BsonDocument("bookId", bookId)),
                
                // Stage 2: Convert userId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "userIdObjectId", new BsonDocument("$toObjectId", "$userId") }
                }),
                
                // Stage 3: Convert bookId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "bookIdObjectId", new BsonDocument("$toObjectId", "$bookId") }
                }),
                
                // Stage 4: Lookup User collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "user" }
                }),
                
                // Stage 5: Lookup Book collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "books" },
                    { "localField", "bookIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "book" }
                }),
                
                // Stage 6: Unwind User array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$user" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 7: Unwind Book array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$book" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 8: Remove temporary ObjectId fields
                new BsonDocument("$unset", new BsonArray { "userIdObjectId", "bookIdObjectId" }),
                
                // Stage 9: Skip và Limit
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", limit)
            };

            var result = await _borrows.Aggregate<BorrowDetailDto>(pipeline).ToListAsync();
            return result;
        }

        /// <summary>
        /// Lấy borrow records theo status với thông tin chi tiết User và Book
        /// Sử dụng aggregation pipeline tối ưu
        /// </summary>
        public async Task<List<BorrowDetailDto>> GetByStatusWithDetailsAsync(string status, int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;

            /*
             * MongoDB Aggregation Pipeline Query:
             * 
             * [
             *   // Stage 1: Match borrow records theo status
             *   { $match: { status: "Borrowed" } },
             *   
             *   // Stage 2: Convert userId string thành ObjectId
             *   { $addFields: { 
             *       userIdObjectId: { $toObjectId: "$userId" } 
             *   }},
             *   
             *   // Stage 3: Convert bookId string thành ObjectId
             *   { $addFields: { 
             *       bookIdObjectId: { $toObjectId: "$bookId" } 
             *   }},
             *   
             *   // Stage 4: Lookup User collection
             *   { $lookup: {
             *       from: "users",
             *       localField: "userIdObjectId", 
             *       foreignField: "_id",
             *       as: "user"
             *   }},
             *   
             *   // Stage 5: Lookup Book collection
             *   { $lookup: {
             *       from: "books",
             *       localField: "bookIdObjectId",
             *       foreignField: "_id", 
             *       as: "book"
             *   }},
             *   
             *   // Stage 6: Unwind User array
             *   { $unwind: { 
             *       path: "$user", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 7: Unwind Book array
             *   { $unwind: { 
             *       path: "$book", 
             *       preserveNullAndEmptyArrays: true 
             *   }},
             *   
             *   // Stage 8: Remove temporary ObjectId fields
             *   { $unset: ["userIdObjectId", "bookIdObjectId"] },
             *   
             *   // Stage 9: Skip và Limit
             *   { $skip: skip },
             *   { $limit: limit }
             * ]
             */

            var pipeline = new[]
            {
                // Stage 1: Match borrow records theo status
                new BsonDocument("$match", new BsonDocument("status", status)),
                
                // Stage 2: Convert userId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "userIdObjectId", new BsonDocument("$toObjectId", "$userId") }
                }),
                
                // Stage 3: Convert bookId string thành ObjectId
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "bookIdObjectId", new BsonDocument("$toObjectId", "$bookId") }
                }),
                
                // Stage 4: Lookup User collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "user" }
                }),
                
                // Stage 5: Lookup Book collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "books" },
                    { "localField", "bookIdObjectId" },
                    { "foreignField", "_id" },
                    { "as", "book" }
                }),
                
                // Stage 6: Unwind User array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$user" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 7: Unwind Book array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$book" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                
                // Stage 8: Remove temporary ObjectId fields
                new BsonDocument("$unset", new BsonArray { "userIdObjectId", "bookIdObjectId" }),
                
                // Stage 9: Skip và Limit
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", limit)
            };

            var result = await _borrows.Aggregate<BorrowDetailDto>(pipeline).ToListAsync();
            return result;
        }
    }
}
