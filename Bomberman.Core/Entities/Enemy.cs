using Bomberman.Core.Enums;
using Bomberman.Core.Patterns.Behavioral.Strategy;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Entities
{
    public class Enemy
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Speed { get; } = 0.5;
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

        // Gereksiz null atamalarını engellemek için somut tipleri kullanıyoruz.
        public void Update(GameMap? map, Player?  targetPlayer) 
        {
            // Null uyarılarını kesmek için safe-call kontrolü ekleyebilirsiniz:
            if (_movementStrategy == null) return;
            
            var (deltaX, deltaY) = _movementStrategy.CalculateMovement(this, map, targetPlayer);

            X += deltaX;
            Y += deltaY;
        }
    }
}