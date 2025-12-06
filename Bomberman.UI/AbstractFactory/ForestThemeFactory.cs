using Bomberman.Core.Patterns.Creational;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Core.Walls;
using Bomberman.UI.Rendering;

namespace Bomberman.UI.Themes
{
    public class ForestThemeFactory : IThemeFactory
    {
        public IWallFactory CreateWallFactory()
            => new ForestWallFactory();

        public Texture2D CreateGroundTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateGrassTexture(gd, size);

        public Texture2D CreatePlayerTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateColoredSquare(gd, size, Color.ForestGreen);

        public Texture2D CreateEnemyTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateColoredSquare(gd, size, Color.DarkOliveGreen);

        public Texture2D CreateBombTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateBombTexture(gd, size, Color.Black);

        public Texture2D CreateUnbreakableWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateStoneTexture(gd, size, Color.DarkGreen, Color.Olive);

        public Texture2D CreateBreakableWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateCrackedTexture(gd, size, Color.OliveDrab, Color.SeaGreen);

        public Texture2D CreateHardWallTexture(GraphicsDevice gd, int size)
            => TextureGenerator.GenerateMetalTexture(gd, size, Color.DarkOliveGreen, Color.LightGreen);
    }
}