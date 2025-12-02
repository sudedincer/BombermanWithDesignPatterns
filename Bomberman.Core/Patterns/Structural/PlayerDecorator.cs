using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

public abstract class PlayerDecorator : IPlayer
{
    protected IPlayer inner;
    protected float duration;

    public PlayerDecorator(IPlayer inner)
    {
        this.inner = inner;
        this.duration=duration;
    }

    public virtual double GetSpeed() => inner.GetSpeed();
    public virtual int GetBombPower() => inner.GetBombPower();
    public virtual int GetMaxBombCount() => inner.GetMaxBombCount();
    public virtual void Update(double dt)
    {
        inner.Update(dt);

        duration -= (float)dt;

        if (duration <= 0)
        {
            // Süre bitti → eski haline dön
            RevertEffect();
        }
    }
    public abstract void RevertEffect();

    public virtual (double X, double Y) GetPosition() => inner.GetPosition();
    public bool IsAlive { get => inner.IsAlive; set => inner.IsAlive = value; }

    public void Move(double dx, double dy, GameMap map)
        => inner.Move(dx, dy, map);
    public virtual void OnExplosion(int x, int y, int power)
    {
        inner.OnExplosion(x, y, power);
    }
}
