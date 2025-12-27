using Bomberman.Core.GameLogic;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    /// <summary>
    /// Forest theme specific breakable wall (Ahşap Sandık)
    /// </summary>
    public class ForestBreakableWall : BreakableWall
    {
        public ForestBreakableWall(int x, int y, GameMap map) : base(x, y, map)
        {
        }

        public override WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.ForestBreakable;

        // Future: Tema-specific davranış
        // Örn: Yıkılınca yapraklar uçuşur
    }
}
