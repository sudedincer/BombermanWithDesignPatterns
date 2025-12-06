using Bomberman.Core.Patterns.Creational;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Core.Walls;
using Bomberman.UI.Rendering;

namespace Bomberman.UI.Themes
{
    public class DesertThemeFactory : IThemeFactory
    {
        public IWallFactory CreateWallFactory()
            => new DesertWallFactory();

        public Texture2D CreateGroundTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateSandTexture(gd, size);

        public Texture2D CreatePlayerTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateColoredSquare(gd, size, Color.SandyBrown);

        public Texture2D CreateEnemyTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateColoredSquare(gd, size, Color.OrangeRed);

        public Texture2D CreateBombTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateBombTexture(gd, size, Color.DarkGoldenrod);

        public Texture2D CreateUnbreakableWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateStoneTexture(gd, size, Color.Brown, Color.SaddleBrown);

        public Texture2D CreateBreakableWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateCrackedTexture(gd, size, Color.Peru, Color.Sienna);

        public Texture2D CreateHardWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateMetalTexture(gd, size, Color.Goldenrod, Color.DarkKhaki);
    }
}