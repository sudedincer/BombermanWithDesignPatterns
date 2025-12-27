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
            // Orman teması için farklı duvar tipleri (görsel olarak) döndürebilirsiniz.
            switch (type)
            {
                case WallType.Unbreakable:
                    return new UnbreakableWall(); // Ağaç gövdesi gibi
                case WallType.Breakable:
                    return new BreakableWall(x, y, map); // Yaprak/Çalı gibi
                case WallType.Hard:
                    return new HardWall(x, y); // Kalın kütük
                default:
                    throw new ArgumentException($"Invalid wall type for Forest Factory.");
            }
        }
    }
}