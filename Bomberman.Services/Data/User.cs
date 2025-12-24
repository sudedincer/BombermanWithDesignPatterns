namespace Bomberman.Services.Data;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Stats
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames { get; set; }
    public int Kills { get; set; }  // Enemy kills
    
    // Preferences
    public string PreferredTheme { get; set; } = "Forest";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}