using System;
using Bomberman.Core.PowerUps;

namespace Bomberman.Core.PowerUps
{
    public class PowerUpFactory
    {
        private static readonly Random _rng = new Random();

        public static PowerUp CreateRandomPowerUp(int x, int y)
        {
            int roll = _rng.Next(0, 3);

            return roll switch
            {
                0 => new SpeedPowerUp(x, y),
                1 => new BombPowerUp(x, y),
                _ => new ExtraBombPowerUp(x, y),
            };
        }
    }
}