namespace Bomberman.Services.Data;

// Oyun istatistikleri (wins, losses, total games)
public class Score
{
    public int Id { get; set; }
    public string Username { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames => Wins + Losses;
}