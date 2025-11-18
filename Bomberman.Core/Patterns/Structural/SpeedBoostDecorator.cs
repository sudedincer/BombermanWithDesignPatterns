namespace Bomberman.Core
{
    // Hız Artışı Power-up'ı (Concrete Decorator)
    public class SpeedBoostDecorator : PlayerDecorator
    {
        public SpeedBoostDecorator(IPlayer player) : base(player) { }

        public override double GetSpeed()
        {
            // Temel hızı alır ve %20 artırır (Dekorasyon)
            return base.GetSpeed() * 1.2;
        }
    }
}