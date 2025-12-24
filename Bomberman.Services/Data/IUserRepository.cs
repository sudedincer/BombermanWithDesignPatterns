namespace Bomberman.Services.Data;

// Kullanıcı işlemleri için Repository Arayüzü
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> AddUserAsync(User user);
    Task<bool> UpdateStatsAsync(string username, bool isWin);
    Task<bool> UpdateKillsAsync(string username, int killCount);
    Task<IEnumerable<User>> GetTopPlayersAsync(int count);
    Task<bool> UpdatePreferencesAsync(string username, string theme);
}