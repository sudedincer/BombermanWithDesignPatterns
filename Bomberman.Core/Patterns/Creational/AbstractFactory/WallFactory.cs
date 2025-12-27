using Bomberman.Core.Enums;

namespace Bomberman.Core.Patterns.Creational
{
    /// <summary>
    /// Abstract Factory için "Factory of Factories".
    /// Seçilen temaya göre doğru IWallFactory instance'ını üretir.
    /// </summary>
    public static class WallFactory
    {
        public static IWallFactory Create(ThemeType theme)
        {
            return theme switch
            {
                ThemeType.Desert => new DesertWallFactory(),
                ThemeType.Forest => new ForestWallFactory(),
                ThemeType.City   => new CityWallFactory(),
                _                => new DesertWallFactory() // default
            };
        }
    }
}