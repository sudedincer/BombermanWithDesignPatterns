using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Behavioral.Observer;

namespace Bomberman.Core.Walls
{
    public class BreakableWall : Wall, IExplosionObserver
    {
        public int X { get; } // Konum
        public int Y { get; } // Konum
        private GameMap _map;
        public override bool CanBeDestroyed() => true;
        
        
        public BreakableWall(int x, int y, GameMap map)
        {
            X = x;
            Y = y;
            _map = map;
        }

        public void OnExplosion(int explosionX, int explosionY, int range)
        {
            if (!IsDestroyed && X == explosionX && Y == explosionY)
                IsDestroyed = true;
        }
    }
}