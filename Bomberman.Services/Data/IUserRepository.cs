namespace Bomberman.Services.Data;

// Kullanıcı işlemleri için Repository Arayüzü
public interface IUserRepository
{
    Task<User> GetByUsernameAsync(string username);
    Task<bool> AddUserAsync(User user);
    Task<bool> UpdateStatsAsync(string username, int wins, int losses);
}