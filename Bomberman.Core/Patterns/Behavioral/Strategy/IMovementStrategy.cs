using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public interface IMovementStrategy
    {
        // Gereksiz object tiplerini GameMap ve Player'a çevirdik.
        (double DeltaX, double DeltaY) CalculateMovement(Enemy enemy, GameMap? map, Player? targetPlayer);
    }
}