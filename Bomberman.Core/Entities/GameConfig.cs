using System;

namespace Bomberman.Core.Config
{
    public sealed class GameConfig
    {
        // Lazy-loaded thread-safe Singleton
        private static readonly Lazy<GameConfig> _instance =
            new Lazy<GameConfig>(() => new GameConfig());

        public static GameConfig Instance => _instance.Value;

        // Private constructor
        private GameConfig()
        {
            PlayerDefaultSpeed = 3.0;
            BombLifetime = 3.0f;
            PowerUpDropRate = 0.25; // %25
            HardWallHits = 3;
        }

        // GAME SETTINGS
        public double PlayerDefaultSpeed { get; set; }
        public float BombLifetime { get; set; }
        public double PowerUpDropRate { get; set; }
        public int HardWallHits { get; set; }

        // Global random (oyunda tek RNG)
        public Random Rng { get; private set; } = new Random();

        public void SetSeed(int seed)
        {
            Rng = new Random(seed);
        }
    }
}