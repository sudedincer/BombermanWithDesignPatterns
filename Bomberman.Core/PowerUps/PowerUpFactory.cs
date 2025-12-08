using System;
using Bomberman.Core.PowerUps;

namespace Bomberman.Core.PowerUps
{
    public class PowerUpFactory
    {
        public static PowerUp CreateRandomPowerUp(int x, int y)
        {
            int roll = Bomberman.Core.Config.GameConfig.Instance.Rng.Next(0, 3);

            return roll switch
            {
                0 => new SpeedPowerUp(x, y),
                1 => new BombPowerUp(x, y),
                _ => new ExtraBombPowerUp(x, y),
            };
        }

        public static PowerUp CreatePowerUp(int x, int y, Bomberman.Core.Enums.PowerUpType type)
        {
            return type switch
            {
                Bomberman.Core.Enums.PowerUpType.Speed => new SpeedPowerUp(x, y),
                Bomberman.Core.Enums.PowerUpType.Range => new BombPowerUp(x, y),
                Bomberman.Core.Enums.PowerUpType.ExtraBomb => new ExtraBombPowerUp(x, y),
                _ => new SpeedPowerUp(x, y)
            };
        }
    }
}