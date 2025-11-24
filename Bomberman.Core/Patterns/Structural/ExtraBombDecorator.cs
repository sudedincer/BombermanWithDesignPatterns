using Bomberman.Core.Entities;

public class ExtraBombDecorator : PlayerDecorator
{
    public ExtraBombDecorator(IPlayer inner) : base(inner) {}

    public override int GetMaxBombCount() => base.GetMaxBombCount() + 1;
}