using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Core.Entities;
using Bomberman.Core.Walls;
using Bomberman.Core.GameLogic;
using Bomberman.Core.PowerUps;


namespace Bomberman.UI.View
{
    public class GameView
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _pixel;
        private readonly int _tileSize;
        private Texture2D _powerUpTexture;
        private readonly Dictionary<string, Texture2D> _textureCache = new();
        private Rectangle _starRect;
        private Rectangle _flameRect;
        private Rectangle _boxRect;
        private int _iconSize;
        private readonly List<(int X, int Y, float Timer)> _explosions = new();


        public GameView(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, int tileSize = 64)
        {
            _spriteBatch = spriteBatch;
            _tileSize = tileSize;

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            CacheTexture("Unbreakable", CreateSolid(graphicsDevice, Color.Gray));
            CacheTexture("Breakable", CreateSolid(graphicsDevice, Color.Red));
            CacheTexture("HardFull", CreateSolid(graphicsDevice, Color.LimeGreen));
            CacheTexture("HardDamaged", CreateSolid(graphicsDevice, Color.GreenYellow));
            CacheTexture("Player", CreateSolid(graphicsDevice, Color.Blue));
            CacheTexture("Bomb", CreateSolid(graphicsDevice, Color.Black));
            CacheTexture("Explosion", CreateSolid(graphicsDevice, Color.Orange));
        }

        private void CacheTexture(string key, Texture2D tex)
        {
            if (!_textureCache.ContainsKey(key))
                _textureCache[key] = tex;
        }

        private Texture2D CreateSolid(GraphicsDevice device, Color color)
        {
            Texture2D texture = new Texture2D(device, _tileSize, _tileSize);
            Color[] data = new Color[_tileSize * _tileSize];
            for (int i = 0; i < data.Length; i++) data[i] = color;
            texture.SetData(data);
            return texture;
        }

        public void DrawGame(GameMap map, IPlayer player, List<Bomb> bombs)
        {
            _spriteBatch.Begin();

            DrawWalls(map.Walls);
            DrawPowerUps(map.PowerUps);
            DrawGridLines(map.Width, map.Height);
            DrawBombs(bombs);
            DrawExplosions();
            
            DrawPlayer(player);

            _spriteBatch.End();
        }

        private void DrawGridLines(int width, int height)
        {
            Color gridColor = Color.Black;

            for (int y = 0; y <= height; y++)
                DrawLine(new Vector2(0, y * _tileSize), new Vector2(width * _tileSize, y * _tileSize), gridColor);

            for (int x = 0; x <= width; x++)
                DrawLine(new Vector2(x * _tileSize, 0), new Vector2(x * _tileSize, height * _tileSize), gridColor);
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness = 1)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            _spriteBatch.Draw(_pixel,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(edge.Length(), thickness),
                SpriteEffects.None,
                0);
        }

        private void DrawWalls(Wall[,] walls)
        {
            int rows = walls.GetLength(0);
            int cols = walls.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Wall wall = walls[r, c];
                    if (wall == null || wall.IsDestroyed) continue;

                    Texture2D tex = GetWallTexture(wall);
                    Vector2 pos = new Vector2(c * _tileSize, r * _tileSize);

                    _spriteBatch.Draw(tex, pos, Color.White);
                }
            }
        }

        private Texture2D GetWallTexture(Wall wall)
        {
            if (wall is UnbreakableWall)
                return _textureCache["Unbreakable"];

            if (wall is BreakableWall)
                return _textureCache["Breakable"];

            if (wall is HardWall hw)
            {
                // Hard wall damage feedback
                if (hw.HitsRemaining >= 2)
                    return _textureCache["HardFull"];

                if (hw.HitsRemaining == 1)
                    return _textureCache["HardDamaged"];
            }

            return _textureCache["Unbreakable"];
        }

        private void DrawPlayer(IPlayer player)
        {
            var pos = player.GetPosition();

            Vector2 screenPos = new Vector2(
                (float)pos.X * _tileSize,
                (float)pos.Y * _tileSize
            );

            float offset = _tileSize * 0.15f;
            float size = _tileSize * 0.70f;

            Rectangle dest = new Rectangle(
                (int)(screenPos.X + offset),
                (int)(screenPos.Y + offset),
                (int)size,
                (int)size
            );

            _spriteBatch.Draw(_textureCache["Player"], dest, Color.White);
        }

        private void DrawPowerUps(List<PowerUp> powerUps)
        {
            foreach (var pu in powerUps)
            {
                if (pu.Collected)
                    continue;

                Vector2 pos = new Vector2(pu.X * _tileSize, pu.Y * _tileSize);

                Rectangle dest = new Rectangle(
                    (int)pos.X + 4,
                    (int)pos.Y + 4,
                    _tileSize - 8,
                    _tileSize - 8);

                Rectangle sourceRect = pu switch
                {
                    SpeedPowerUp => _starRect,
                    BombPowerUp => _flameRect,
                    ExtraBombPowerUp => _boxRect,
                    _ => _starRect
                };

                _spriteBatch.Draw(_powerUpTexture, dest, sourceRect, Color.White);
            }
        }        private void DrawExplosions()
        {
            foreach (var exp in _explosions)
            {
                Vector2 pos = new Vector2(exp.X * _tileSize, exp.Y * _tileSize);

                Rectangle dest = new Rectangle(
                    (int)pos.X, (int)pos.Y,
                    _tileSize, _tileSize
                );

                _spriteBatch.Draw(_textureCache["Explosion"], dest, Color.White);
            }
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                var e = _explosions[i];
                e.Timer -= delta;

                if (e.Timer <= 0)
                    _explosions.RemoveAt(i);
                else
                    _explosions[i] = e;
            }
        }

        public void AddExplosionVisual(int x, int y)
        {
            _explosions.Add((x, y, 0.25f)); // patlama 250ms gözüksün
        }

        private void DrawBombs(List<Bomb> bombs)
        {
            foreach (var bomb in bombs)
            {
                float x = bomb.X * _tileSize + _tileSize / 2f;
                float y = bomb.Y * _tileSize + _tileSize / 2f;

                float radius = _tileSize * 0.25f;

                float pulse = (float)(Math.Sin(bomb.TimeSincePlaced * 6) * 3);
                radius += pulse;

                Color bodyColor = bomb.TimeRemaining < 0.5f && ((int)(bomb.TimeRemaining * 20) % 2 == 0)
                    ? Color.Red
                    : Color.Black;

                DrawCircle(x, y, radius + 3, Color.DarkGray);
                DrawCircle(x, y, radius, bodyColor);
                DrawCircle(x + radius * 0.8f, y - radius * 0.8f, radius * 0.25f, Color.Yellow);
            }
        }

        private void DrawCircle(float cx, float cy, float radius, Color color)
        {
            int segments = 24;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathF.Tau * i / segments;
                float px = cx + MathF.Cos(angle1) * radius;
                float py = cy + MathF.Sin(angle1) * radius;

                var rect = new Rectangle((int)px, (int)py, 4, 4);
                _spriteBatch.Draw(_pixel, rect, color);
            }
        }
        
        public void SetPowerUpTexture(Texture2D texture)
        {
            _powerUpTexture = texture;

            int iconWidth = _powerUpTexture.Width / 3;   // 1536 / 3 = 512
            int iconHeight = _powerUpTexture.Height;     // 1024

            _starRect  = new Rectangle(0 * iconWidth, 0, iconWidth, iconHeight);
            _flameRect = new Rectangle(1 * iconWidth, 0, iconWidth, iconHeight);
            _boxRect   = new Rectangle(2 * iconWidth, 0, iconWidth, iconHeight);
        }
    }
}