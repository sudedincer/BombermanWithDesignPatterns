using Bomberman.Core.Entities;
using Bomberman.Core.Patterns;

namespace Bomberman.Core.PowerUps
{
    public abstract class PowerUp
    {
        public int X { get; }
        public int Y { get; }
        public bool Collected { get; set; }
        public float Duration { get; protected set; } = 10f; // 10 saniye
        public abstract Bomberman.Core.Enums.PowerUpType Type { get; }


        protected PowerUp(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IPlayer Apply(IPlayer player)
        {
            if (Collected)
                return player;

            Collected = true;
            return OnCollect(player);
        }

        protected abstract IPlayer OnCollect(IPlayer player);
    }
}