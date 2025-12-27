using Bomberman.Core.Enums;
using Bomberman.Core.Walls;
using System;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Creational
{
    public class CityWallFactory : IWallFactory
    {
        public Wall CreateWall(WallType type, int x, int y, GameMap map)
        {
            // ✅ GERÇEK ABSTRACT FACTORY - City tema-specific sınıflar
            switch (type)
            {
                case WallType.Unbreakable:
                    return new UnbreakableWall(); // Beton blok (tema-independent)
                    
                case WallType.Breakable:
                    // ✅ City-specific breakable wall (Tuğla Duvar)
                    return new CityBreakableWall(x, y, map);
                    
                case WallType.Hard:
                    // ✅ City-specific hard wall (Çelik Duvar)
                    return new CityHardWall(x, y);
                    
                default:
                    throw new ArgumentException($"Invalid wall type for City Factory.");
            }
        }
    }
}