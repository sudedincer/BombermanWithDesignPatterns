using Bomberman.Core.Patterns.Creational;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Core.Walls;
using Bomberman.UI.Rendering;

namespace Bomberman.UI.Themes
{
    public class CityThemeFactory : IThemeFactory
    {
        public IWallFactory CreateWallFactory()
            => new CityWallFactory();

        public Texture2D CreateGroundTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateAsphaltTexture(gd, size);

        public Texture2D CreatePlayerTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateColoredSquare(gd, size, Color.SteelBlue);

        public Texture2D CreateEnemyTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateColoredSquare(gd, size, Color.DarkSlateGray);

        public Texture2D CreateBombTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateBombTexture(gd, size, Color.DimGray);

        public Texture2D CreateUnbreakableWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateStoneTexture(gd, size, Color.Gray, Color.DarkGray);

        public Texture2D CreateBreakableWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateCrackedTexture(gd, size, Color.Silver, Color.LightGray);

        public Texture2D CreateHardWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateMetalTexture(gd, size, Color.LightSlateGray, Color.SlateGray);
    }
}