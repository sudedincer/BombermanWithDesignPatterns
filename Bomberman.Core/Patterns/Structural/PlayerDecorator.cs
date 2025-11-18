namespace Bomberman.Core
{
    // Tüm Power-up'ların miras alacağı soyut sınıf (Abstract Decorator)
    public abstract class PlayerDecorator : IPlayer
    {
        protected IPlayer decoratedPlayer;

        public PlayerDecorator(IPlayer player)
        {
            this.decoratedPlayer = player;
        }
        
        // Metotların büyük çoğunluğu sarmalanan nesneye devredilir (Delegation)
        public virtual int GetMaxBombs() => decoratedPlayer.GetMaxBombs();
        public virtual int GetBombPower() => decoratedPlayer.GetBombPower();
        public virtual double GetSpeed() => decoratedPlayer.GetSpeed();
        public virtual void Move(double x, double y) => decoratedPlayer.Move(x, y);
        public virtual (double X, double Y) GetPosition() => decoratedPlayer.GetPosition();
    }
}