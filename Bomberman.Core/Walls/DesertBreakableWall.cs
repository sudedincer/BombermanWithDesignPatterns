using Bomberman.Core.GameLogic;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    /// <summary>
    /// Desert theme specific breakable wall (Kumtaşı)
    /// </summary>
    public class DesertBreakableWall : BreakableWall
    {
        public DesertBreakableWall(int x, int y, GameMap map) : base(x, y, map)
        {
        }

        /// <summary>
        /// Wall knows its own visual theme - Hybrid Abstract Factory!
        /// </summary>
        public override WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.DesertBreakable;

        // Future: Tema-specific davranış eklenebilir
        // Örn: Yıkılınca kum bulutu efekti
    }
}
