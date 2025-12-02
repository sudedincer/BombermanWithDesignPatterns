using Bomberman.Core.Entities;

public abstract class TimedPlayerDecorator : PlayerDecorator
{
    private float duration;
    private float timer;

    public bool IsExpired => timer <= 0f;

    public IPlayer InnerPlayer => inner; // Geri dönüş için gerekli getter

    protected TimedPlayerDecorator(IPlayer inner, float duration)
        : base(inner)
    {
        timer = duration;
        this.duration = duration;
    }

    public void Update(float dt)
    {
        base.Update(dt);
        timer -= dt;
    }

    // Ekstra cleanup gerekiyorsa alt sınıflar override edebilir.
    public override void RevertEffect() { }
}