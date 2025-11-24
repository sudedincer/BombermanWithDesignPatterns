using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public interface IMovementStrategy
    {
        (double DeltaX, double DeltaY) CalculateMovement(
            Enemy enemy,
            GameMap? map,
            IPlayer? targetPlayer
        );
    }
}