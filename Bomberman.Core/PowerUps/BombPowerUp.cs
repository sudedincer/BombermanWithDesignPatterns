using Bomberman.Core.Entities;
using Bomberman.Core.Patterns;

namespace Bomberman.Core.PowerUps
{
    public class BombPowerUp : PowerUp
    {
        public BombPowerUp(int x, int y) : base(x, y) {}

        protected override IPlayer OnCollect(IPlayer p)
            => new BombPowerDecorator(p);
    }
}