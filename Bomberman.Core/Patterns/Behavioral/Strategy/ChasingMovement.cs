using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public class ChasingMovement : IMovementStrategy
    {
        public (double DeltaX, double DeltaY) CalculateMovement(
            Enemy enemy,
            GameMap map,
            IPlayer targetPlayer)
        {
            var (px, py) = targetPlayer.GetPosition();

            double ex = enemy.X;
            double ey = enemy.Y;

            double dx = 0, dy = 0;
            double step = enemy.Speed * 0.04;

            // X ekseni takibi
            if (px > ex) dx = step;
            else if (px < ex) dx = -step;

            // Y ekseni takibi
            if (py > ey) dy = step;
            else if (py < ey) dy = -step;

            // Çarpışma kontrolü
            double nextX = ex + dx;
            double nextY = ey + dy;

            // Eğer direkt yol kapalıysa X ve Y ayrı ayrı denenir
            // Check Collision using Center Point (Offset + 0.5)
            if (map.CheckCollision(nextX + 0.5, nextY + 0.5))
            {
                // Önce sadece X hareketi dene
                if (!map.CheckCollision(nextX + 0.5, ey + 0.5))
                    return (dx, 0);

                // Sonra sadece Y hareketi dene
                if (!map.CheckCollision(ex + 0.5, nextY + 0.5))
                    return (0, dy);

                // Hiç biri olmazsa bu kare dur
                return (0, 0);
            }

            return (dx, dy);
        }
    }
}