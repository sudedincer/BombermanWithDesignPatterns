using Bomberman.Core.Entities;

public class ExtraBombDecorator : TimedPlayerDecorator
{
    public ExtraBombDecorator(IPlayer inner, float duration)
        : base(inner, duration) { }

    public override int GetMaxBombCount()
        => base.GetMaxBombCount() + 1;
}