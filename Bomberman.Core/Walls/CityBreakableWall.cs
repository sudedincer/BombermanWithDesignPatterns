using Bomberman.Core.GameLogic;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    /// <summary>
    /// City theme specific breakable wall (Tuğla Duvar)
    /// </summary>
    public class CityBreakableWall : BreakableWall
    {
        public CityBreakableWall(int x, int y, GameMap map) : base(x, y, map)
        {
        }

        public override WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.CityBreakable;

        // Future: Tema-specific davranış
        // Örn: Yıkılınca tuğla parçaları saçılır
    }
}
