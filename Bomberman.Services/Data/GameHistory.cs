namespace Bomberman.Services.Data;

// Oyun geçmişi kayıtları
public class GameHistory
{
    public int Id { get; set; }
    public string Player1 { get; set; } = string.Empty;
    public string Player2 { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public int Player1Kills { get; set; }
    public int Player2Kills { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    public int DurationSeconds { get; set; }
}
