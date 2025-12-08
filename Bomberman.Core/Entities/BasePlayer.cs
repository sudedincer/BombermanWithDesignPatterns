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
        double sensitivity = 0.15; // How far to check for a gap

        // 1. Try Moving X
        if (!map.CheckCollision(X + dx + 0.5, Y + 0.5))
        {
            X += dx;
        }
        else if (dx != 0) // Blocked on X? Try to Slide Y
        {
            double slideSpeed = Math.Abs(dx);
            
            // Check Up
            if (!map.CheckCollision(X + dx + 0.5, Y + 0.5 - sensitivity))
                Y -= slideSpeed;
            // Check Down
            else if (!map.CheckCollision(X + dx + 0.5, Y + 0.5 + sensitivity))
                Y += slideSpeed;
        }

        // 2. Try Moving Y
        if (!map.CheckCollision(X + 0.5, Y + dy + 0.5))
        {
            Y += dy;
        }
        else if (dy != 0) // Blocked on Y? Try to Slide X
        {
            double slideSpeed = Math.Abs(dy);

            // Check Left
            if (!map.CheckCollision(X + 0.5 - sensitivity, Y + dy + 0.5))
                X -= slideSpeed;
            // Check Right
            else if (!map.CheckCollision(X + 0.5 + sensitivity, Y + dy + 0.5))
                X += slideSpeed;
        }
    }

    public (double X, double Y) GetPosition() => (X, Y);
    public void OnExplosion(int x, int y, int power)
    {
        if (!IsAlive)
            return;

        if ((int)Math.Floor(X + 0.5) == x &&
            (int)Math.Floor(Y + 0.5) == y)
        {
            IsAlive = false;
            Console.WriteLine("Player died from explosion!");
        }
    }
}