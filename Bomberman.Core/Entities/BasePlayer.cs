using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Behavioral.Observer;
using Bomberman.Core.Patterns.Behavioral.State;

public class BasePlayer : IPlayer,IExplosionObserver
{
    public double X { get; private set; }
    public double Y { get; private set; }
    private int _maxBombCount = 1;
    private int _bombPower = 1;
    // State Pattern
    public IPlayerState CurrentState { get; private set; }

    // Backward compatibility property
    // eski kodun çalışmaya devam etmesi için ekstra kontrol
    public bool IsAlive 
    { 
        get => CurrentState is AlivePlayerState; 
        set 
        { 
            if (value && !(CurrentState is AlivePlayerState))
                TransitionTo(new AlivePlayerState());
            else if (!value && !(CurrentState is DeadPlayerState))
                TransitionTo(new DeadPlayerState());
        }
    }

    public BasePlayer(double x, double y)
    {
        X = x;
        Y = y;
        TransitionTo(new AlivePlayerState());
    }

    public void TransitionTo(IPlayerState state)
    {
        CurrentState = state;
    }

    public void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
    }

    public virtual double GetSpeed() => 3.5;
    public virtual int GetBombPower() => _bombPower;
    public virtual int GetMaxBombCount() => _maxBombCount;
    public virtual void Update(double dt)
    {
        // State update if needed
    }

    public void Move(double dx, double dy, GameMap map)
    {
        CurrentState.HandleMove(this, dx, dy, map);
    }

    public (double X, double Y) GetPosition() => (X, Y);
    
    public void OnExplosion(int x, int y, int power)
    {
        CurrentState.HandleExplosion(this, x, y, power);
    }
}