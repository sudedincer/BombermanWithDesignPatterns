using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    /// <summary>
    /// Forest theme specific hard wall (Kalın Kütük)
    /// </summary>
    public class ForestHardWall : HardWall
    {
        public ForestHardWall(int x, int y) : base(x, y)
        {
        }

        public override WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.ForestHard;

        // Future: Tema-specific davranış
        // Örn: Ağaç kırılma sesi eklenebilir
    }
}
