using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

public abstract class PlayerDecorator : IPlayer
{
    protected IPlayer inner;

    public PlayerDecorator(IPlayer inner)
    {
        this.inner = inner;
    }

    public virtual double GetSpeed() => inner.GetSpeed();
    public virtual int GetBombPower() => inner.GetBombPower();
    public virtual int GetMaxBombCount() => inner.GetMaxBombCount();
    public virtual (double X, double Y) GetPosition() => inner.GetPosition();
    public bool IsAlive { get => inner.IsAlive; set => inner.IsAlive = value; }

    public void Move(double dx, double dy, GameMap map)
        => inner.Move(dx, dy, map);
}