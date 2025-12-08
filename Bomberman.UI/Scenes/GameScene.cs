using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bomberman.Core.Entities;
using Bomberman.Core.Enums;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Creational;
using Bomberman.Core.Patterns.Creational.Builder;
using Bomberman.Core.Patterns.Structural.Adapter;
using Bomberman.UI.Patterns.Structural.Adapter;
using Bomberman.Core.PowerUps;
using Bomberman.Core.Walls;
using Bomberman.Services.Network;
using Bomberman.UI.Controller;
using Bomberman.UI.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared;

namespace Bomberman.UI.Scenes
{
    public class GameScene : Scene
    {
        private readonly ThemeType _theme;

        private GameMap _map;
        private IPlayer _player;
        private bool _isPlayerAlive = true;

        private KeyboardState _previousKeyboardState;
        private readonly string _username = "Player1";

        private SpriteFont _font;
        private MouseState _previousMouseState;

        // UI Constants
        private Rectangle _restartBtnRect;
        private Rectangle _lobbyBtnRect;

        public GameScene(Game1 game, ThemeType selectedTheme) : base(game)
        {
            _theme = selectedTheme;

            // 1) Font
            _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");

            // 2) Tema'ya göre wall factory
            IWallFactory factory = WallFactory.Create(_theme);

            // 3) Map + Player (Using Builder Pattern)
            ClassicMapBuilder mapBuilder = new ClassicMapBuilder();
            mapBuilder.SetDimensions(Game.MapWidth, Game.MapHeight);
            mapBuilder.SetTheme(factory);
            
            mapBuilder.BuildWalls();
            mapBuilder.ClearSafeZone();
            mapBuilder.SpawnEnemies(3); // count not strictly used by ClassicBuilder yet but good practice

            _map = mapBuilder.GetMap();
            
            _player = new BasePlayer(1, 1);
            _isPlayerAlive = true;

            // 4) Patlama event → görsel + kill
            _map.ExplosionCell += OnExplosionCell;

            // 5) Tema'ya göre duvar atlası
            if (_theme == ThemeType.Desert)
            {
                var t1 = Game.Content.Load<Texture2D>("Textures/desert_unbreakable");
                var t2 = Game.Content.Load<Texture2D>("Textures/desert_hard");
                var t3 = Game.Content.Load<Texture2D>("Textures/desert_hard_damaged");
                var t4 = Game.Content.Load<Texture2D>("Textures/desert_breakable");
                Game.GameView.SetWallTextures(t1, t2, t3, t4);
            }
            else if (_theme == ThemeType.Forest)
            {
                var t1 = Game.Content.Load<Texture2D>("Textures/forest_unbreakable");
                var t2 = Game.Content.Load<Texture2D>("Textures/forest_hard");
                var t3 = Game.Content.Load<Texture2D>("Textures/forest_hard_damaged");
                var t4 = Game.Content.Load<Texture2D>("Textures/forest_breakable");
                Game.GameView.SetWallTextures(t1, t2, t3, t4);
            }
            else
            {
                var t1 = Game.Content.Load<Texture2D>("Textures/city_unbreakable");
                var t2 = Game.Content.Load<Texture2D>("Textures/city_hard");
                var t3 = Game.Content.Load<Texture2D>("Textures/city_hard_damaged");
                var t4 = Game.Content.Load<Texture2D>("Textures/city_breakable");
                Game.GameView.SetWallTextures(t1, t2, t3, t4);
            }
        }

        private void OnExplosionCell(int x, int y)
        {
            // Patlama efekti + öldürme
            Game.GameView.AddExplosionVisual(x, y);
            KillPlayerAt(x, y);
            KillEnemiesAt(x, y);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // ============================
            // GAME OVER LOGIC
            // ============================
            if (!_isPlayerAlive)
            {
                // Calculate button positions dynamically (in case of resize)
                SetupGameOverLayout();

                // Mouse Interaction
                bool leftClick = mouseState.LeftButton == ButtonState.Released &&
                                 _previousMouseState.LeftButton == ButtonState.Pressed;

                if (leftClick)
                {
                    if (_restartBtnRect.Contains(mouseState.Position))
                    {
                        Restart();
                    }
                    if (_lobbyBtnRect.Contains(mouseState.Position))
                    {
                        SceneManager.ChangeScene(new LobbyScene(Game));
                    }
                }

                // Keyboard 'R' to Restart
                if (currentKeyboardState.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
                {
                    Restart();
                }

                _previousMouseState = mouseState;
                _previousKeyboardState = currentKeyboardState;

                Game.GameView.Update(gameTime);
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ============================
            // OYUNCU HAREKETİ (ADAPTER PATTERN)
            // ============================
            // Dependency Injection ideali, ama şimdilik burada oluşturuyoruz.
            IInputService inputService = new KeyboardInputAdapter();
            var (deltaX, deltaY, _) = inputService.GetInput();

            if (deltaX != 0 || deltaY != 0)
            {
                double speed = _player.GetSpeed();
                var pos = _player.GetPosition();

                double moveX = deltaX * speed * dt;
                double moveY = deltaY * speed * dt;

                double targetX = pos.X + moveX;
                double targetY = pos.Y + moveY;

                if (!_map.IsWallAt(targetX, targetY))
                {
                    _player.Move(moveX, moveY, _map);
                    CheckPowerUpPickup();
                    CheckPlayerEnemyCollision();
                }

                var newPos = _player.GetPosition();

                var playerState = new PlayerStateDTO
                {
                    Username = _username,
                    X = newPos.X,
                    Y = newPos.Y,
                    IsAlive = _isPlayerAlive
                };

                // network tarafı
                Task.Run(() => Game.GameClient.SendMovementAsync(playerState));
            }

            // ============================
            // BOMBALAR
            // ============================
            // ============================
            // BOMBALAR
            // ============================
            // Iterate backwards or separate list to avoid modification errors if needed, 
            // but Update doesn't remove bombs usually.
            // GameMap.Bombs is a List.
            foreach (var bomb in _map.Bombs)
            {
                bomb.Update(dt);

                if (!bomb.IsExploded && bomb.TimeRemaining <= 0)
                {
                    bomb.Explode(); 
                }
            }

            // ============================
            // ENEMY UPDATE
            // ============================
            foreach (var enemy in _map.Enemies)
            {
                enemy.Update(_map, _player);
            }

            // ============================
            // PLAYER UPDATE + DECORATOR SÜRELERİ
            // ============================
            _player.Update(dt);

            if (_player is TimedPlayerDecorator timed && timed.IsExpired)
            {
                timed.RevertEffect();
                _player = timed.InnerPlayer;
            }

            // ============================
            // BOMBA KOYMA (Space)
            // ============================
            if (currentKeyboardState.IsKeyDown(Keys.Space) &&
                !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                int activeBombs = _map.Bombs.Count(b => !b.IsExploded);
                int maxBombs = _player.GetMaxBombCount();

                if (activeBombs < maxBombs)
                {
                    PlaceBomb();
                }
            }

            CheckPowerUpPickup();

            Game.GameView.Update(gameTime);
            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = mouseState;
        }

        private void SetupGameOverLayout()
        {
            var vp = GraphicsDevice.Viewport;
            int btnWidth = 220;
            int btnHeight = 50;
            int spacing = 20;

            int centerX = vp.Width / 2;
            int startY = vp.Height / 2 + 30;

            _restartBtnRect = new Rectangle(centerX - btnWidth / 2, startY, btnWidth, btnHeight);
            _lobbyBtnRect = new Rectangle(centerX - btnWidth / 2, startY + btnHeight + spacing, btnWidth, btnHeight);
        }

        private void PlaceBomb()
        {
            var pos = _player.GetPosition();

            // Center-based tile mapping
            int bombX = (int)Math.Floor(pos.X + 0.5);
            int bombY = (int)Math.Floor(pos.Y + 0.5);
            int power = _player.GetBombPower();

            System.Console.WriteLine($"[DEBUG] Placing Bomb at ({bombX}, {bombY})");

            var bombDto = new BombDTO
            {
                PlacedByUsername = _username,
                X = bombX,
                Y = bombY,
                Power = power
            };

            var bomb = new Bomb(bombX, bombY, power);
            
            // Observer pattern
            bomb.Attach(_map);        // → IExplosionObserver: OnExplosion
            bomb.Attach(Game.GameView);
            foreach (var enemy in _map.Enemies)
                bomb.Attach(enemy);

            _map.Bombs.Add(bomb); // Add to map instead of local list

            Task.Run(() => Game.GameClient.PlaceBombAsync(bombDto));
        }

        private void CheckPowerUpPickup()
        {
            var pos = _player.GetPosition();

            int px = (int)Math.Floor(pos.X + 0.5);
            int py = (int)Math.Floor(pos.Y + 0.5);

            foreach (var p in _map.PowerUps)
            {
                if (!p.Collected && p.X == px && p.Y == py)
                {
                    _player = p.Apply(_player);
                    p.Collected = true;
                }
            }
        }

        private void KillPlayerAt(int x, int y)
        {
            if (!_isPlayerAlive)
                return;

            int px = (int)Math.Round(_player.GetPosition().X);
            int py = (int)Math.Round(_player.GetPosition().Y);

            if (px == x && py == y)
            {
                _isPlayerAlive = false;
                Console.WriteLine($"[DEBUG] Player died in explosion at ({x},{y})");
            }
        }

        private void CheckPlayerEnemyCollision()
        {
            foreach (var enemy in _map.Enemies)
            {
                int ex = (int)Math.Round(enemy.X);
                int ey = (int)Math.Round(enemy.Y);

                var pos = _player.GetPosition();
                int px = (int)Math.Round(pos.X);
                int py = (int)Math.Round(pos.Y);

                if (px == ex && py == ey)
                {
                    _isPlayerAlive = false;
                    return;
                }
            }
        }

        private void KillEnemiesAt(int x, int y)
        {
            var dead = _map.Enemies
                .Where(e => (int)Math.Floor(e.X + 0.5) == x &&
                            (int)Math.Floor(e.Y + 0.5) == y)
                .ToList();

            foreach (var e in dead)
                _map.Enemies.Remove(e);
        }

        private void Restart()
        {
            // Eski map event'ini bırak
            if (_map != null)
                _map.ExplosionCell -= OnExplosionCell;

            IWallFactory factory = WallFactory.Create(_theme);
            ClassicMapBuilder mapBuilder = new ClassicMapBuilder();
            mapBuilder.SetDimensions(Game.MapWidth, Game.MapHeight);
            mapBuilder.SetTheme(factory);

            mapBuilder.BuildWalls();
            mapBuilder.ClearSafeZone();
            mapBuilder.SpawnEnemies(3);

            _map = mapBuilder.GetMap();
            _map.ExplosionCell += OnExplosionCell;

            _player = new BasePlayer(1, 1);
            _isPlayerAlive = true;

            _map.Bombs.Clear();
            Game.GameView.ClearExplosions();
        }

        public override void Draw(GameTime gameTime)
        {
            Color bgColor = _theme switch
            {
                ThemeType.Desert => Color.NavajoWhite, // Sand color
                ThemeType.Forest => Color.DarkSeaGreen, // Softer green
                ThemeType.City   => Color.Gray,
                _ => Color.CornflowerBlue
            };

            Game.GraphicsDevice.Clear(bgColor);

            Game.GameView.DrawGame(_map, _player, _map.Bombs);

            if (!_isPlayerAlive)
            {
                DrawGameOverOverlay();
            }
        }

        private void DrawGameOverOverlay()
        {
            SpriteBatch.Begin();

            var vp = GraphicsDevice.Viewport;
            var full = new Rectangle(0, 0, vp.Width, vp.Height);

            // Darker Overlay for focus
            SpriteBatch.Draw(Game.Pixel, full, Color.Black * 0.75f);

            // === TITLE ===
            string title = "GAME OVER";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (vp.Width - titleSize.X * 3f) / 2, // 3x scale calculation
                vp.Height / 2 - 120);

            // Shadow text
            SpriteBatch.DrawString(_font, title, titlePos + new Vector2(4, 4), Color.Black * 0.5f, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
            // Main text
            SpriteBatch.DrawString(_font, title, titlePos, Color.Crimson, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

            // === BUTTONS ===
            Point mousePos = Mouse.GetState().Position;
            bool restartHover = _restartBtnRect.Contains(mousePos);
            bool lobbyHover = _lobbyBtnRect.Contains(mousePos);

            DrawStyledButton(_restartBtnRect, "RESTART", restartHover);
            DrawStyledButton(_lobbyBtnRect, "LOBBY", lobbyHover);

            SpriteBatch.End();
        }

        private void DrawStyledButton(Rectangle r, string text, bool isHovered)
        {
            Color baseColor = new Color(50, 50, 60);
            Color hoverColor = new Color(220, 180, 50); // Gold/Orange for accent
            
            Color bg = isHovered ? hoverColor : baseColor;
            Color textColor = isHovered ? Color.Black : Color.White;

            // Shadow
            Rectangle shadow = r;
            shadow.Offset(4, 4);
            SpriteBatch.Draw(Game.Pixel, shadow, Color.Black * 0.5f);

            // Button Body
            SpriteBatch.Draw(Game.Pixel, r, bg);

            // Border
            DrawBorder(r, 2, Color.White * 0.2f);

            // Text
            Vector2 size = _font.MeasureString(text);
            Vector2 pos = new Vector2(
                r.Center.X - size.X / 2,
                r.Center.Y - size.Y / 2);
            SpriteBatch.DrawString(_font, text, pos, textColor);
        }

        private void DrawBorder(Rectangle r, int thickness, Color color)
        {
            SpriteBatch.Draw(Game.Pixel, new Rectangle(r.X, r.Y, r.Width, thickness), color);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), color);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(r.X, r.Y, thickness, r.Height), color);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), color);
        }
    }
}