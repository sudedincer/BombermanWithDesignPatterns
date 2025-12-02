using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public class StaticMovement : IMovementStrategy
    {
        public (double DeltaX, double DeltaY) CalculateMovement(
            Enemy enemy,
            GameMap map,
            IPlayer targetPlayer)
        {
            double step = enemy.Speed * 0.05; 

            double dx = (enemy.Direction == Direction.Right) ? step : -step;
            double nextX = enemy.X + dx;

            // Duvar varsa yön değiş
            if (map.IsWallAt(nextX, enemy.Y))
            {
                enemy.Direction = enemy.Direction == Direction.Right
                    ? Direction.Left
                    : Direction.Right;

                return (0, 0);
            }

            return (dx, 0);
        }
    }
}