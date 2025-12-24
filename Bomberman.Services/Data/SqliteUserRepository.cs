
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
                    Kills INTEGER DEFAULT 0,
                    PreferredTheme TEXT DEFAULT 'Forest',
                    CreatedAt TEXT
                );
            ";
            command.ExecuteNonQuery();

            // Create GameHistory table
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS GameHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Player1 TEXT NOT NULL,
                    Player2 TEXT NOT NULL,
                    Winner TEXT NOT NULL,
                    Theme TEXT NOT NULL,
                    Player1Kills INTEGER DEFAULT 0,
                    Player2Kills INTEGER DEFAULT 0,
                    PlayedAt TEXT NOT NULL,
                    DurationSeconds INTEGER DEFAULT 0
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
                    Kills = reader.GetInt32(6),
                    PreferredTheme = reader.IsDBNull(7) ? "Forest" : reader.GetString(7),
                    CreatedAt = DateTime.Parse(reader.GetString(8))
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
                    INSERT INTO Users (Username, PasswordHash, Wins, Losses, TotalGames, Kills, PreferredTheme, CreatedAt)
                    VALUES (@username, @passwordHash, @wins, @losses, @totalGames, @kills, @preferredTheme, @createdAt)
                ";
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@wins", user.Wins);
                command.Parameters.AddWithValue("@losses", user.Losses);
                command.Parameters.AddWithValue("@totalGames", user.TotalGames);
                command.Parameters.AddWithValue("@kills", user.Kills);
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
                    Kills = reader.GetInt32(6),
                    PreferredTheme = reader.IsDBNull(7) ? "Forest" : reader.GetString(7),
                    CreatedAt = DateTime.Parse(reader.GetString(8))
                });
            }
            return users;
        }

        public async Task<bool> UpdateKillsAsync(string username, int killCount)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET Kills = Kills + @killCount WHERE Username = @username";
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@killCount", killCount);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
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

        public async Task<bool> SaveGameAsync(GameHistory game)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GameHistory (Player1, Player2, Winner, Theme, Player1Kills, Player2Kills, PlayedAt, DurationSeconds)
                VALUES (@player1, @player2, @winner, @theme, @p1kills, @p2kills, @playedAt, @duration)
            ";
            command.Parameters.AddWithValue("@player1", game.Player1);
            command.Parameters.AddWithValue("@player2", game.Player2);
            command.Parameters.AddWithValue("@winner", game.Winner);
            command.Parameters.AddWithValue("@theme", game.Theme);
            command.Parameters.AddWithValue("@p1kills", game.Player1Kills);
            command.Parameters.AddWithValue("@p2kills", game.Player2Kills);
            command.Parameters.AddWithValue("@playedAt", game.PlayedAt.ToString("o"));
            command.Parameters.AddWithValue("@duration", game.DurationSeconds);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<GameHistory>> GetRecentGamesAsync(int count)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM GameHistory ORDER BY PlayedAt DESC LIMIT @count";
            command.Parameters.AddWithValue("@count", count);

            var games = new List<GameHistory>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new GameHistory
                {
                    Id = reader.GetInt32(0),
                    Player1 = reader.GetString(1),
                    Player2 = reader.GetString(2),
                    Winner = reader.GetString(3),
                    Theme = reader.GetString(4),
                    Player1Kills = reader.GetInt32(5),
                    Player2Kills = reader.GetInt32(6),
                    PlayedAt = DateTime.Parse(reader.GetString(7)),
                    DurationSeconds = reader.GetInt32(8)
                });
            }
            return games;
        }
    }
}