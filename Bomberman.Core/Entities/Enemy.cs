using Bomberman.Core.Enums;
using Bomberman.Core.Patterns.Behavioral.Strategy;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Entities
{
    public class Enemy
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public bool IsAlive { get; set; } = true;

        public double Speed { get; set; } = 0.5;
        public Direction Direction { get; set; } = Direction.Right;

        private IMovementStrategy? _movementStrategy;

        public Enemy(double x, double y, IMovementStrategy strategy)
        {
            X = x;
            Y = y;

            SetMovementStrategy(strategy);

            
        }

        public void SetMovementStrategy(IMovementStrategy strategy)
        {
            _movementStrategy = strategy;
        }

        public void Update(GameMap map, IPlayer targetPlayer)
        {
            if (_movementStrategy == null)
                return;

            var (dx, dy) = _movementStrategy.CalculateMovement(this, map, targetPlayer);

            X += dx;
            Y += dy;
        }
    }
}