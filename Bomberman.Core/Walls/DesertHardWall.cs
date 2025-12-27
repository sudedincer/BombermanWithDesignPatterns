using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    /// <summary>
    /// Desert theme specific hard wall (Sertleştirilmiş Kil)
    /// </summary>
    public class DesertHardWall : HardWall
    {
        public DesertHardWall(int x, int y) : base(x, y)
        {
        }

        public override WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.DesertHard;

        // Future: Tema-specific davranış
        // Örn: Hasar aldığında çatlak sesi eklenebilir
    }
}
