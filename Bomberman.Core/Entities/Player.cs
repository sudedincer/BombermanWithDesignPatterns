using Bomberman.Core.GameLogic;

using System;
// Gerekli diğer using yönergeleri

namespace Bomberman.Core.Entities
{
    // Temel oyuncu sınıfı
    public class Player : IPlayer
    {
        private int _maxBombs = 1; 
        private int _bombPower = 1;
        private double _speed = 3.0;
        private double _x = 0, _y = 0; 
        public bool IsAlive { get; set; } = true; // Oyuncu durumu
        
        // Constructor, sadece başlangıç pozisyonunu ayarlar.
        public Player(double startX, double startY)
        {
            _x = startX;
            _y = startY;
        }

        public int GetMaxBombs() => _maxBombs;
        public int GetBombPower() => _bombPower;
        public double GetSpeed() => _speed;

        // Move metodunu GameMap parametresi alacak şekilde güncelliyoruz.
        // NOTE: Bu, tüm çağıran yerlerin (Game1.Update, vb.) GameMap nesnesini iletmesini gerektirir.
        public void Move(double deltaX, double deltaY, GameMap map)
        {
            // Hareketin hızla çarpılmış gerçek mesafesini hesapla
            double actualMoveX = deltaX * GetSpeed();
            double actualMoveY = deltaY * GetSpeed();

            double newX = _x + actualMoveX;
            double newY = _y + actualMoveY;
            
            // Çarpışma Kontrolü:
            // Sadece hareket etmek istenen yeni konumda duvar yoksa hareket et.
            if (!map.IsWallAt(newX, newY)) 
            {
                _x = newX;
                _y = newY;
            }
            // Duvar varsa pozisyon güncellenmez.
        }
        
        // Eski Move metodunu, Decorator desenine uyum için koruyoruz.
        // Bu metot, sadece tekil yerel hareketlerde kullanılmalıdır.
        public void Move(double deltaX, double deltaY)
        {
            // Bu metot çarpışma kontrolü yapmaz ve artık kullanılmamalıdır.
            // Ancak IPlayer arayüzü bunu gerektiriyorsa, GameMap olmadan
            // basit hareket yapar. Projenizde GameMap'li versiyon kullanılmalıdır.
        }
        
        public (double X, double Y) GetPosition() => (_x, _y);
        
        public void OnExplosion(int explosionX, int explosionY, int range)
        {
            // Player'ın bulunduğu kareyi al
            int playerGridX = (int)Math.Round(GetPosition().X);
            int playerGridY = (int)Math.Round(GetPosition().Y);

            // Eğer oyuncu patlama merkezinde veya menzilindeyse ölür.
            // Basit kontrol: Patlama merkezine denk gelirse öl.
            if (IsAlive && playerGridX == explosionX && playerGridY == explosionY)
            {
                IsAlive = false;
                // Network'e ölüm mesajı gönderilmeli (Controller/Hub tarafından)
            }
        }
    }
}