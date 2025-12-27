using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    /// <summary>
    /// City theme specific hard wall (Çelik Duvar)
    /// </summary>
    public class CityHardWall : HardWall
    {
        public CityHardWall(int x, int y) : base(x, y)
        {
        }

        public override WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.CityHard;

        // Future: Tema-specific davranış
        // Örn: Metal çarpma sesi eklenebilir
    }
}
