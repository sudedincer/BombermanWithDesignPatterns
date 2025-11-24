using Bomberman.Core.Entities;

public class BombPowerDecorator : PlayerDecorator
{
    public BombPowerDecorator(IPlayer inner) : base(inner) {}

    public override int GetBombPower() => base.GetBombPower() + 1;
}