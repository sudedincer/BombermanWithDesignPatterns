using Bomberman.Core.Enums;
using Bomberman.Core.Walls;
using System;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Creational
{
    public class ForestWallFactory : IWallFactory
    {
        public Wall CreateWall(WallType type, int x, int y, GameMap map)
        {
            // ✅ GERÇEK ABSTRACT FACTORY - Forest tema-specific sınıflar
            switch (type)
            {
                case WallType.Unbreakable:
                    return new UnbreakableWall(); // Ağaç gövdesi (tema-independent)
                    
                case WallType.Breakable:
                    // ✅ Forest-specific breakable wall (Ahşap Sandık)
                    return new ForestBreakableWall(x, y, map);
                    
                case WallType.Hard:
                    // ✅ Forest-specific hard wall (Kalın Kütük)
                    return new ForestHardWall(x, y);
                    
                default:
                    throw new ArgumentException($"Invalid wall type for Forest Factory.");
            }
        }
    }
}