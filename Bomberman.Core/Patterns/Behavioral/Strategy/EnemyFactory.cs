using Bomberman.Core.Entities;
using Bomberman.Core.Enums;
using Bomberman.Core.Patterns.Behavioral.Strategy;

namespace Bomberman.Core.Factories
{
    public static class EnemyFactory
    {
        public static Enemy CreateEnemy(double x, double y, EnemyType type)
        {
            IMovementStrategy strategy = type switch
            {
                EnemyType.Static => new StaticMovement(),
                EnemyType.Chaser => new ChasingMovement(),
                _ => new RandomWalkMovement()
            };

            var enemy = new Enemy(x, y, strategy);
            enemy.Type = type;
            return enemy;
        }
    }
}