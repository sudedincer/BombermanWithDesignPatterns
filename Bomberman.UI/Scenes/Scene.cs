using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.UI.Scenes
{
    public abstract class Scene
    {
        protected Game1 Game { get; }

        protected GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
        protected SpriteBatch SpriteBatch => Game.SpriteBatch;

        protected Scene(Game1 game)
        {
            Game = game;
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }

  
}