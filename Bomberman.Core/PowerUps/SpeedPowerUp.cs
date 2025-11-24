using Bomberman.Core.Entities;
using Bomberman.Core.Patterns;

namespace Bomberman.Core.PowerUps
{
    public class SpeedPowerUp : PowerUp
    {
        public SpeedPowerUp(int x, int y) : base(x, y) {}

        protected override IPlayer OnCollect(IPlayer p)
            => new SpeedBoostDecorator(p);
    }
}