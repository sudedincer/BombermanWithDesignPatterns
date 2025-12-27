using Bomberman.Core.Enums;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Walls;

namespace Bomberman.Core.Patterns.Creational
{
    public class DesertWallFactory : IWallFactory
    {
        public Wall CreateWall(WallType type, int x, int y, GameMap map)
        {
            // ✅ GERÇEK ABSTRACT FACTORY - Tema-specific sınıflar döndürüyor!
            switch (type)
            {
                case WallType.Unbreakable:
                    // Unbreakable wall tema-independent (hepsi aynı)
                    return new UnbreakableWall();
                
                case WallType.Breakable:
                    // ✅ Desert-specific breakable wall (Kumtaşı)
                    return new DesertBreakableWall(x, y, map); 
                
                case WallType.Hard:
                    // ✅ Desert-specific hard wall (Sertleştirilmiş Kil)
                    return new DesertHardWall(x, y);
                
                default:
                    throw new ArgumentException($"Invalid wall type: {type}");
            }
        }
    }
}