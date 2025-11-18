using Bomberman.Core.Enums;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Walls;

namespace Bomberman.Core.Patterns.Creational
{
    // Concrete Creator: Çöl Teması duvarlarını üretir
    public class DesertWallFactory : IWallFactory
    {
        public Wall CreateWall(WallType type, int x, int y, GameMap map)
        {
            // Bu switch-case, istemci kodunu duvar sınıflarından soyutlar.
            switch (type)
            {
                case WallType.Unbreakable:
                    return new UnbreakableWall();
                case WallType.Breakable:
                    return new BreakableWall(x, y,map); 
                case WallType.Hard:
                    return new HardWall(x,y);
                default:
                    // Geçersiz tip gelirse hata fırlatmak güvenlidir.
                    throw new ArgumentException($"Invalid wall type: {type}");
            }
        }
    }
}