using System;
using System.Collections.Generic;
using System.Linq;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Behavioral.Observer;

namespace Bomberman.Core.Entities
{
    /// <summary>
    /// Bomb = Subject (Konu)
    /// Observer Pattern: Bomb → Player, Enemy, GameMap, GameView
    /// Bomb sadece patlama olayını bildirir.
    /// Patlamanın yayılımını GameMap hesaplar.
    /// </summary>
    public class Bomb : IExplosionSubject
    {
        private readonly List<IExplosionObserver> _observers = new();

        public int X { get; }
        public int Y { get; }
        public int Power { get; }

        public float Lifetime { get; private set; } = 3f;    // 3 saniyede patlar
        public float TimeSincePlaced { get; private set; } = 0f;
        public float TimeRemaining => Math.Max(0, Lifetime - TimeSincePlaced);

        public bool IsExploded { get; set; } = false;
        public bool IsRemote { get; set; } = false;

        public Bomb(int x, int y, int power)
        {
            X = x;
            Y = y;
            Power = power;
        }

        // =============================================================
        // TIMER UPDATE
        // =============================================================

        public void Update(float delta)
        {
            if (IsExploded)
                return;

            TimeSincePlaced += delta;
        }

        // =============================================================
        // OBSERVER REGISTER
        // =============================================================

        public void Attach(IExplosionObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Detach(IExplosionObserver observer)
        {
            _observers.Remove(observer);
        }

       
        // =============================================================
        // NOTIFY (Patlamayı duyurur)
        // =============================================================

        public void Notify(int range)
        {
            foreach (var observer in _observers.ToList())
            {
                observer.OnExplosion(X, Y, range);
            }
        }

        // =============================================================
        // EXPLODE (Sadece olayı tetikler)
        // =============================================================

        /// <summary>
        /// Bomb simply notifies observers.
        /// Explosion propagation is handled entirely by GameMap.
        /// </summary>
        public void Explode()
        {
            if (IsExploded)
                return;

            IsExploded = true;

            // Patlama sinyalini gönder
            Notify(Power);
        }
    }
}