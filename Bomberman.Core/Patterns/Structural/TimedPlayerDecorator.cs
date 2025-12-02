using Bomberman.Core.Entities;

public abstract class TimedPlayerDecorator : PlayerDecorator
{
    private float timer;

    public bool IsExpired => timer <= 0f;

    // Power-up bittiğinde geri dönebilmek için
    public IPlayer InnerPlayer => inner;

    protected TimedPlayerDecorator(IPlayer inner, float durationSeconds)
        : base(inner)
    {
        timer = durationSeconds;
    }

    // 🔹 IPlayer.Update(double dt) imzasını override ediyoruz.
    public override void Update(double dt)
    {
        // Süreyi azalt
        timer -= (float)dt;

        // Normal player davranışını yine çalıştır
        base.Update(dt);
    }

    // Ekstra cleanup gerekiyorsa alt sınıflar override edebilir.
    public override void RevertEffect()
    {
        // Varsayılan olarak ekstra bir şey yapma.
    }
}