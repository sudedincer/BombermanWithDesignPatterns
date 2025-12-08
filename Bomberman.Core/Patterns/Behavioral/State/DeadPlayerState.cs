using System;
using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.State
{
    public class DeadPlayerState : IPlayerState
    {
        public void HandleMove(BasePlayer context, double dx, double dy, GameMap map)
        {
            // Dead players cannot move
        }

        public void HandleExplosion(BasePlayer context, int x, int y, int power)
        {
            // Dead players cannot die again
        }
    }
}
