using Bomberman.Core.Entities;

public class BombPowerDecorator : TimedPlayerDecorator
{
    public BombPowerDecorator(IPlayer inner, float duration)
        : base(inner, duration) { }

    public override int GetBombPower()
        => base.GetBombPower() + 1;
}