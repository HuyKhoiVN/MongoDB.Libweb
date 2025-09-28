using MongoDB.Driver;
using MongoDB_Libweb.Models;

namespace MongoDB_Libweb.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        // Các collections (thay bằng tên collection số ít như đã thiết kế)
        public IMongoCollection<User> Users => _database.GetCollection<User>("User");
        public IMongoCollection<Book> Books => _database.GetCollection<Book>("Book");
        public IMongoCollection<Borrow> Borrows => _database.GetCollection<Borrow>("Borrow");
        public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Category");
        public IMongoCollection<Author> Authors => _database.GetCollection<Author>("Author");
        public IMongoCollection<FileUpload> FileUploads => _database.GetCollection<FileUpload>("FileUpload");
    }
}
