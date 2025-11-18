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
            // Merkezi bildir (Patlama başladığı yer)
            Notify(Power);

            // 4 ana yönde patlamayı yay (Observer)
            int[] dx = { 1, -1, 0, 0 }; // Sağ, Sol
            int[] dy = { 0, 0, 1, -1 }; // Aşağı, Yukarı

            for (int i = 0; i < 4; i++)
            {
                for (int r = 1; r <= Power; r++) // Patlama menzili (Power) kadar yay
                {
                    int currentX = X + dx[i] * r;
                    int currentY = Y + dy[i] * r;

                    // Harita sınırlarını ve duvar tipini kontrol et
                    if (map.IsOutsideBounds(currentX, currentY)) break;

                    Walls.Wall wall = map.GetWallAt(currentX, currentY);

                    if (wall is Walls.UnbreakableWall)
                    {
                        // Kırılamaz duvara çarptı, yayılım durur.
                        break;
                    }
                    else if (wall is Walls.BreakableWall || wall is Walls.HardWall)
                    {
                        // Kırılabilir/Sert duvara çarptı. Sadece o kareyi etkile ve yayılımı durdur.
                        NotifySingle(currentX, currentY, Power); // Tek bir kareyi bildir
                        break;
                    }

                    // Boş kare ise (yol) sadece kareyi bildir
                    NotifySingle(currentX, currentY, Power);
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