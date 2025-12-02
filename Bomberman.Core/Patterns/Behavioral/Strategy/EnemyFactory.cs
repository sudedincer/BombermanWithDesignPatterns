using Bomberman.Core.Entities;
using Bomberman.Core.Enums;
using Bomberman.Core.Patterns.Behavioral.Strategy;

namespace Bomberman.Core.Factories
{
    public static class EnemyFactory
    {
        public static Enemy CreateEnemy(double x, double y, EnemyType type)
        {
            return type switch
            {
                EnemyType.RandomWalker =>
                    new Enemy(x, y, new RandomWalkMovement()),

                EnemyType.Static =>
                    new Enemy(x, y, new StaticMovement()),

                EnemyType.Chaser =>
                    new Enemy(x, y, new ChasingMovement()),

                _ =>
                    new Enemy(x, y, new RandomWalkMovement())
            };
        }
    }
}