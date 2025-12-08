using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.State
{
    public interface IPlayerState
    {
        void HandleMove(BasePlayer context, double dx, double dy, GameMap map);
        void HandleExplosion(BasePlayer context, int x, int y, int power);
    }
}
