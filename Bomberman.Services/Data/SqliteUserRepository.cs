
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
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT,
                    Wins INTEGER DEFAULT 0,
                    Losses INTEGER DEFAULT 0,
                    TotalGames INTEGER DEFAULT 0,
                    PreferredTheme TEXT DEFAULT 'Forest',
                    CreatedAt TEXT
                );
            ";
            command.ExecuteNonQuery();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users WHERE Username = @username";
            command.Parameters.AddWithValue("@username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Wins = reader.GetInt32(3),
                    Losses = reader.GetInt32(4),
                    TotalGames = reader.GetInt32(5),
                    PreferredTheme = reader.IsDBNull(6) ? "Forest" : reader.GetString(6),
                    CreatedAt = DateTime.Parse(reader.GetString(7))
                };
            }
            return null;
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, Wins, Losses, TotalGames, PreferredTheme, CreatedAt)
                    VALUES (@username, @passwordHash, @wins, @losses, @totalGames, @preferredTheme, @createdAt)
                ";
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@wins", user.Wins);
                command.Parameters.AddWithValue("@losses", user.Losses);
                command.Parameters.AddWithValue("@totalGames", user.TotalGames);
                command.Parameters.AddWithValue("@preferredTheme", user.PreferredTheme);
                command.Parameters.AddWithValue("@createdAt", user.CreatedAt.ToString("o"));

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (SqliteException)
            {
                // Likely unique constraint violation on Username
                return false;
            }
        }

        public async Task<bool> UpdateStatsAsync(string username, bool isWin)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            // TotalGames artar. Win ise Wins artar, Lose ise Losses artar.
            string updateSql = isWin 
                ? "UPDATE Users SET Wins = Wins + 1, TotalGames = TotalGames + 1 WHERE Username = @username" 
                : "UPDATE Users SET Losses = Losses + 1, TotalGames = TotalGames + 1 WHERE Username = @username";

            command.CommandText = updateSql;
            command.Parameters.AddWithValue("@username", username);
            
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<User>> GetTopPlayersAsync(int count)
        {
            var users = new List<User>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users ORDER BY Wins DESC LIMIT @count";
            command.Parameters.AddWithValue("@count", count);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Wins = reader.GetInt32(3),
                    Losses = reader.GetInt32(4),
                    TotalGames = reader.GetInt32(5),
                    PreferredTheme = reader.IsDBNull(6) ? "Forest" : reader.GetString(6),
                    CreatedAt = DateTime.Parse(reader.GetString(7))
                });
            }
            return users;
        }

        public async Task<bool> UpdatePreferencesAsync(string username, string theme)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET PreferredTheme = @theme WHERE Username = @username";
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@theme", theme);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}