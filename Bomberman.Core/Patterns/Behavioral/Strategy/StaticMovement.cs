using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public class StaticMovement : IMovementStrategy
    {
        public (double DeltaX, double DeltaY) CalculateMovement(
            Enemy enemy,
            GameMap? map,
            IPlayer? targetPlayer)
        {
            double speed = enemy.Speed;
            double nextX = enemy.X;
            double nextY = enemy.Y;
            Direction dir = enemy.Direction;

            // Basit yatay ileri-geri hareketi
            if (dir == Direction.Right)
                nextX += speed;
            else if (dir == Direction.Left)
                nextX -= speed;

            // Duvara çarptıysa yön değiştir
            if (map != null && map.IsWallAt(nextX, nextY))
            {
                enemy.Direction = (dir == Direction.Right)
                    ? Direction.Left
                    : Direction.Right;

                return (0, 0); // Bu frame hareket yok
            }

            return (dir == Direction.Right ? speed : -speed, 0);
        }
    }
}