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
            // Şehir teması (Beton/Tuğla)
            switch (type)
            {
                case WallType.Unbreakable:
                    return new UnbreakableWall(); // Beton blok
                case WallType.Breakable:
                    return new BreakableWall(x, y,map); // Tuğla duvar
                case WallType.Hard:
                    return new HardWall(x, y); // Çelik/Güçlendirilmiş beton
                default:
                    throw new ArgumentException($"Invalid wall type for City Factory.");
            }
        }
    }
}