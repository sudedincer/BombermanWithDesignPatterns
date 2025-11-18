namespace Bomberman.Core.Walls
{
    public class HardWall : Wall
    {
        public int X { get; }
        public int Y { get; }

        private int _hitsRemaining = 2;   // 2 patlamada yıkılıyor
        public int HitsRemaining => _hitsRemaining; // GameView buradan okur

        public HardWall(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool CanBeDestroyed() => _hitsRemaining > 0;

        public void TakeHit()
        {
            if (_hitsRemaining <= 0)
                return;

            _hitsRemaining--;

            if (_hitsRemaining == 0)
                IsDestroyed = true;
        }

        public void OnExplosion(int explosionX, int explosionY, int range)
        {
            if (!IsDestroyed && X == explosionX && Y == explosionY)
                TakeHit();
        }
    }
}