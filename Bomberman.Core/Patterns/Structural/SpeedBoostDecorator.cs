using Bomberman.Core.Entities;

public class SpeedBoostDecorator : PlayerDecorator
{
    public SpeedBoostDecorator(IPlayer inner) : base(inner) {}

    public override double GetSpeed() => base.GetSpeed() * 1.5;
}