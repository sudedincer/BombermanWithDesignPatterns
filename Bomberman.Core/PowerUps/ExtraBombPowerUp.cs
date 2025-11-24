using Bomberman.Core.Entities;
using Bomberman.Core.PowerUps;

public class ExtraBombPowerUp : PowerUp
{
    public ExtraBombPowerUp(int x, int y) : base(x, y) {}


    protected override IPlayer OnCollect(IPlayer p)
        => new ExtraBombDecorator(p);
}