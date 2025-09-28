using MongoDB.Driver;
using MongoDB_Libweb.Data;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Repositories
{
    public class FileUploadRepository : IFileUploadRepository
    {
        private readonly IMongoCollection<FileUpload> _fileUploads;

        public FileUploadRepository(MongoDbContext context)
        {
            _fileUploads = context.FileUploads;
        }

        public async Task<List<FileUpload>> GetAllAsync(int page = 1, int limit = 10)
        {
            var skip = (page - 1) * limit;
            return await _fileUploads.Find(_ => true)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<FileUpload?> GetByIdAsync(string id)
        {
            return await _fileUploads.Find(f => f.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<FileUpload>> GetByBookIdAsync(string bookId)
        {
            return await _fileUploads.Find(f => f.BookId == bookId).ToListAsync();
        }

        public async Task<FileUpload> CreateAsync(FileUpload fileUpload)
        {
            fileUpload.UploadDate = DateTime.UtcNow;
            await _fileUploads.InsertOneAsync(fileUpload);
            return fileUpload;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _fileUploads.DeleteOneAsync(f => f.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _fileUploads.CountDocumentsAsync(_ => true);
        }

        public async Task<bool> ExistsByBookIdAsync(string bookId)
        {
            var count = await _fileUploads.CountDocumentsAsync(f => f.BookId == bookId);
            return count > 0;
        }
    }
}
