using Bomberman.Core.Entities;
using Bomberman.Core.Patterns;

namespace Bomberman.Core.PowerUps
{
    public class SpeedPowerUp : PowerUp
    {
        public SpeedPowerUp(int x, int y) : base(x, y)
        {
            Duration=10f;
        }
        public override Bomberman.Core.Enums.PowerUpType Type => Bomberman.Core.Enums.PowerUpType.Speed;
        

        protected override IPlayer OnCollect(IPlayer p)
            => new SpeedBoostDecorator(p,8f);
    }
}