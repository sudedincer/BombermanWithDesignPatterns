using System.Collections.Generic;
using Bomberman.Core.Patterns.Behavioral.Observer;
using System.Linq;
using Bomberman.Core.GameLogic; // Kütüphane ekleme
using Bomberman.Core.Walls;

namespace Bomberman.Core.Entities
{
    // Bomb sınıfı, IExplosionSubject arayüzünü uygulayarak Konu (Subject) rolünü üstlenir.
    public class Bomb : IExplosionSubject
    {
        private List<IExplosionObserver> _observers = new List<IExplosionObserver>();
        public int X { get; }
        public int Y { get; }
        public int Power { get; }
        public float TimeSincePlaced { get; private set; }
        public float TimeRemaining => Lifetime - TimeSincePlaced;
        public float Lifetime { get; private set; } = 3f; // 3 saniyede patlar
        public bool IsExploded { get; set; } = false;

        public void Update(float delta)
        {
            TimeSincePlaced += delta;
        }

        public Bomb(int x, int y, int power)
        {
            X = x;
            Y = y;
            Power = power; // Bu, patlama menzili olarak kullanılır
        }

        public void Attach(IExplosionObserver observer)
        {
            // Abone listesine ekle
            _observers.Add(observer);
        }


       
        public void Detach(IExplosionObserver observer)
        {
            // Abonelikten çıkar
            _observers.Remove(observer);
        }

        public void Notify(int range)
        {
            // Tüm gözlemcilere patlama olayını bildir
            foreach (var observer in _observers.ToList()) // ToList() ile güvenli döngü
            {
                observer.OnExplosion(X, Y, range);
            }
        }

        // Bu metot, oyun döngüsünde zamanlayıcı bittiğinde çağrılır.
        public void Explode(GameMap map)
        {
            IsExploded = true;

            // merkezi bildir
            Notify(Power);

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int i = 0; i < 4; i++)
            {
                for (int r = 1; r <= Power; r++)
                {
                    int currentX = X + dx[i] * r;
                    int currentY = Y + dy[i] * r;

                    if (map.IsOutsideBounds(currentX, currentY))
                        break;

                    var wall = map.GetWallAt(currentX, currentY);

                    if (wall is UnbreakableWall)
                        break;

                    NotifySingle(currentX, currentY, Power);

                    if (wall is BreakableWall || wall is HardWall)
                        break;
                }
            }
        }

        // Tek bir kareyi bildiren yardımcı metot
        private void NotifySingle(int x, int y, int range)
        {
            foreach (var observer in _observers.ToList())
            {
                observer.OnExplosion(x, y, range);
            }

        }
    }
}