namespace Bomberman.Services.Data;

// Yüksek skorlar için Repository Arayüzü
public interface IScoreRepository
{
    Task<List<Score>> GetLeaderboardAsync(int topCount);
}