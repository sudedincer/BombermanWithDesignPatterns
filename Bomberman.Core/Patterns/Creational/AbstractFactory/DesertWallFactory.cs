using Bomberman.Core.Enums;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Walls;

namespace Bomberman.Core.Patterns.Creational
{
    public class DesertWallFactory : IWallFactory
    {
        // Gelen x ve y parametrelerini kullanıyoruz
        public Wall CreateWall(WallType type, int x, int y, GameMap map)
        {
            switch (type)
            {
                case WallType.Unbreakable:
                    // UnbreakableWall'un constructor'ı parametre almadığı için bu doğru.
                    return new UnbreakableWall();
                
                case WallType.Breakable:
                    // BreakableWall'a konum parametrelerini iletiyoruz
                    return new BreakableWall(x, y,map); 
                
                case WallType.Hard:
                    // HardWall'a konum parametrelerini iletiyoruz
                    return new HardWall(x, y);
                
                default:
                    throw new ArgumentException($"Invalid wall type: {type}");
            }
        }
    }
}