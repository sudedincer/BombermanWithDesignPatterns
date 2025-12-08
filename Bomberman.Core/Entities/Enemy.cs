using Bomberman.Core.Enums;
using Bomberman.Core.Patterns.Behavioral.Strategy;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Behavioral.Observer;

namespace Bomberman.Core.Entities
{
    public class Enemy :IExplosionObserver
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        private static int _globalIdCounter = 0;
        public int VisualId { get; } = System.Threading.Interlocked.Increment(ref _globalIdCounter);
        public bool IsAlive { get; set; } = true;

        public double Speed { get; set; } = 0.2; // Slower speed (was 0.5)
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

        public void OnExplosion(int x, int y, int power)
        {
            if (!IsAlive)
                return;

            int ex = (int)Math.Round(X);
            int ey = (int)Math.Round(Y);

            if (ex == x && ey == y)
            {
                IsAlive = false;
                Console.WriteLine("Enemy Killed by explosion");
            }
        }
    }
}