
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Bomberman.Services.Data
{
    // Concrete Repository: SQLite Veritabanı ile çalışır.
    public class SqliteUserRepository : IUserRepository
    {
        private readonly string _connectionString;
        
        public SqliteUserRepository(string databasePath)
        {
            // Veritabanı dosyasının yolunu alır (Örn: "Data Source=bomberman.db")
            _connectionString = $"Data Source={databasePath}";
            EnsureDatabaseCreated();
        }
        
        // Uygulama ilk çalıştığında veritabanı tablosunu oluşturur (zorunlu)
        private void EnsureDatabaseCreated()
        {
            // Bu metot, USER ve SCORE tablolarını oluşturacak SQL komutlarını çalıştırır.
            // (Kod kalabalığı yapmamak için SQL'i buraya tam yazmıyorum)
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            // ... CREATE TABLE SQL sorguları burada çalışır ...
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            // SELECT * FROM User WHERE Username = @username; sorgusu burada çalışır.
            // ...
            return Task.FromResult(new User { Username = username, PasswordHash = "dummyhash" });
        }

        public Task<bool> AddUserAsync(User user)
        {
            // INSERT INTO User ... sorgusu burada çalışır.
            // ...
            return Task.FromResult(true);
        }

        public Task<bool> UpdateStatsAsync(string username, int wins, int losses)
        {
            // UPDATE Score SET Wins = @wins, Losses = @losses WHERE Username = @username; sorgusu burada çalışır.
            // ...
            return Task.FromResult(true);
        }
    }
}