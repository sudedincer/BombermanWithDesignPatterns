using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Behavioral.Observer;

public class BasePlayer : IPlayer,IExplosionObserver
{
    public double X { get; private set; }
    public double Y { get; private set; }
    private int _maxBombCount = 1;
    private int _bombPower = 1;
    public bool IsAlive { get; set; } = true;

    public BasePlayer(double x, double y)
    {
        X = x;
        Y = y;
    }

    public virtual double GetSpeed() => 3.5;
    public virtual int GetBombPower() => _bombPower;
    public virtual int GetMaxBombCount() => _maxBombCount;
    public virtual void Update(double dt)
    {

    }

    public void Move(double dx, double dy, GameMap map)
    {
        X += dx;
        Y += dy;
    }

    public (double X, double Y) GetPosition() => (X, Y);
    public void OnExplosion(int x, int y, int power)
    {
        if (!IsAlive)
            return;

        if ((int)Math.Round(X) == x &&
            (int)Math.Round(Y) == y)
        {
            IsAlive = false;
            Console.WriteLine("Player died from explosion!");
        }
    }
}