using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class BorrowRepository : IBorrowRepository
    {
        private readonly IMongoCollection<Borrow> _borrows;

        public BorrowRepository(MongoDbContext context)
        {
            _borrows = context.Borrows;
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

        public async Task<bool> HasActiveBorrowAsync(string userId, string bookId)
        {
            var count = await _borrows.CountDocumentsAsync(b => b.UserId == userId && b.BookId == bookId && b.Status == "Borrowed");
            return count > 0;
        }

        public async Task<List<Borrow>> GetActiveBorrowsByUserAsync(string userId)
        {
            return await _borrows.Find(b => b.UserId == userId && b.Status == "Borrowed").ToListAsync();
        }
    }
}
