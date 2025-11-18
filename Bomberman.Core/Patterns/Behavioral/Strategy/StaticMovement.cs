using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public class StaticMovement : IMovementStrategy
    {
        public (double DeltaX, double DeltaY) CalculateMovement(Enemy enemy, GameMap? map, Player? targetPlayer)
        {
            // Basitlik için sadece sağ-sol hareketi ve duvara çarpınca yön değiştirme.

            double speed = enemy.Speed;
            double nextX = enemy.X;
            double nextY = enemy.Y;
            Direction currentDirection = enemy.Direction;

            // Hareket hesaplaması
            if (currentDirection == Direction.Right) nextX += speed;
            else if (currentDirection == Direction.Left) nextX -= speed;

            // Çarpışma kontrolü (Null kontrolü burada önemlidir)
            if (map != null && map.IsWallAt(nextX, nextY)) 
            {
                // Duvara çarptı, yönü tersine çevir
                if (currentDirection == Direction.Right) enemy.Direction = Direction.Left;
                else if (currentDirection == Direction.Left) enemy.Direction = Direction.Right;
                
                // Hareketi sıfırla, yeni yönde bir sonraki update'te hareket edecek
                return (0, 0); 
            }
            
            // Gerçek hareketi döndür
            return (currentDirection == Direction.Right ? speed : -speed, 0);
        }
    }
}