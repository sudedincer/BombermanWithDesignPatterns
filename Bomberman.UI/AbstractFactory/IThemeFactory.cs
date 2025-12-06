using Bomberman.Core.Patterns.Creational;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Core.Walls;

namespace Bomberman.UI.Themes
{
    public interface IThemeFactory
    {
        // Duvar üretimi için alt fabrika
        IWallFactory CreateWallFactory();

        // Zemin dokusu
        Texture2D CreateGroundTexture(GraphicsDevice device, int size);

        // Oyuncu - Enemy - Bomb
        Texture2D CreatePlayerTexture(GraphicsDevice device, int size);
        Texture2D CreateEnemyTexture(GraphicsDevice device, int size);
        Texture2D CreateBombTexture(GraphicsDevice device, int size);

        // Duvar dokuları
        Texture2D CreateUnbreakableWallTexture(GraphicsDevice device, int size);
        Texture2D CreateBreakableWallTexture(GraphicsDevice device, int size);
        Texture2D CreateHardWallTexture(GraphicsDevice device, int size);
    }
}