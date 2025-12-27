using Bomberman.Core.Entities;

public class SpeedBoostDecorator : TimedPlayerDecorator
{
    public SpeedBoostDecorator(IPlayer inner, float duration)
        : base(inner, duration) { }

    public override double GetSpeed()
        => base.GetSpeed() * 1.5;
}