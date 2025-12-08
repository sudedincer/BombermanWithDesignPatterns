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
        private bool _isVictory = false; // New Victory State
        
        // Bomb & Input Logic
        private float _bombCooldownTimer = 0f;
        private KeyboardState _previousKeyboardState;
        private readonly string _username = "Player1";

        private SpriteFont _font;
        private MouseState _previousMouseState;

        // UI Constants
        private Rectangle _restartBtnRect;
        private Rectangle _lobbyBtnRect;
        private int _playerIndex; // Store locally for Drawing logic

        public GameScene(Game1 game, ThemeType selectedTheme, int? seed = null, int playerIndex = 1) : base(game)
        {
            _theme = selectedTheme;
            _playerIndex = playerIndex;
            
            // 1) Font
            _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");

            // 2) Wall Factory
            IWallFactory factory = WallFactory.Create(_theme);

            // 3) Map + Player (Using Builder Pattern)
            ClassicMapBuilder mapBuilder = new ClassicMapBuilder();
            // If Seed provided (Multiplayer), set it
            if (seed.HasValue)
                mapBuilder = new ClassicMapBuilder(seed.Value);

            mapBuilder.SetDimensions(Game.MapWidth, Game.MapHeight);
            mapBuilder.SetTheme(factory);
            
            mapBuilder.BuildWalls();
            mapBuilder.ClearSafeZone(); // Clears both corners 
            mapBuilder.SpawnEnemies(3); 
            
            _map = mapBuilder.GetMap();
            _map.ExplosionCell += OnExplosionCell;

            // 4) Initialize Players (P1: Top-Left, P2: Bottom-Right)
            int p1X = 1; 
            int p1Y = 1;
            int p2X = Game.MapWidth - 2; 
            int p2Y = Game.MapHeight - 2;

            if (playerIndex == 1)
            {
                _player = new BasePlayer(p1X, p1Y);
            }
            else
            {
                _player = new BasePlayer(p2X, p2Y);
            }
            
            // 5) Register Multiplayer Listeners if Online
            if (seed.HasValue) // Only valid if multiplayer
            {
                 RegisterMultiplayerEvents();
                 // Initialize Remote Player at opposite position
                 int remoteX = (playerIndex == 1) ? p2X : p1X;
                 int remoteY = (playerIndex == 1) ? p2Y : p1Y;
                 _remotePlayer = new BasePlayer(remoteX, remoteY);
            }

            // 6) Load Wall Textures based on Theme
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
            Game.GameView.SetExplosionTexture(Game.Content.Load<Texture2D>("Textures/explosion"));
            
            Game.GameView.SetPlayerTexture(Game.Content.Load<Texture2D>("Textures/player"));
            Game.GameView.SetPlayer2Texture(Game.Content.Load<Texture2D>("Textures/player2"));

            Game.GameView.SetBombTexture(Game.Content.Load<Texture2D>("Textures/bomb"));
            Game.GameView.SetPowerUpTexture(Game.Content.Load<Texture2D>("Textures/powerups"));

            var enemies = new List<Texture2D>
            {
                Game.Content.Load<Texture2D>("Textures/enemy"),
                Game.Content.Load<Texture2D>("Textures/enemy2"),
                Game.Content.Load<Texture2D>("Textures/enemy3")
            };
            Game.GameView.SetEnemyTextures(enemies);
        }

        private IPlayer _remotePlayer; // Simple visualization
        
        private void RegisterMultiplayerEvents()
        {
            Game.GameClient.PlayerMoved += OnRemotePlayerMoved;
            Game.GameClient.ExplosionReceived += OnRemoteExplosion; // Still keep for explosion effects
            Game.GameClient.PlayerEliminated += OnPlayerEliminated;
            
            Game.GameClient.BombPlaced += OnRemoteBombPlaced;
            Game.GameClient.EnemyMoved += OnRemoteEnemyMoved;
            
            Game.GameClient.PowerUpSpawned += OnRemotePowerUpSpawned;
            Game.GameClient.PowerUpCollected += OnRemotePowerUpCollected;

            // Map Logic
            if (_playerIndex == 1) // Host Logic
            {
                _map.ShouldSpawnPowerUps = true;
                _map.OnPowerUpSpawned += (pu) =>
                {
                    Game.GameClient.SpawnPowerUpAsync(new Shared.DTOs.PowerUpDTO
                    {
                        X = pu.X,
                        Y = pu.Y,
                        Type = pu.Type.ToString()
                    });
                };
            }
        }

        private void OnRemotePowerUpSpawned(Shared.DTOs.PowerUpDTO dto)
        {
            // Only non-hosts need to add it, but for safety (and ensuring identical ID/State), 
            // maybe validation is needed. For now, trust the server.
            // If Host receives loopback, it might duplicate?
            // "Clients.Others" usually prevents loopback.
            // So P2 receives it. P2 adds it.
            // P1 added it locally in RemoveWall.
            
            // Check if exists?
            if (!_map.PowerUps.Any(p => p.X == dto.X && p.Y == dto.Y))
            {
                // Parse Type
                Bomberman.Core.Enums.PowerUpType type = Enum.Parse<Bomberman.Core.Enums.PowerUpType>(dto.Type);
                var pu = Bomberman.Core.PowerUps.PowerUpFactory.CreatePowerUp(dto.X, dto.Y, type);
                _map.PowerUps.Add(pu);
            }
        }

        private void OnRemotePowerUpCollected(Shared.DTOs.PowerUpDTO dto)
        {
            var pu = _map.PowerUps.FirstOrDefault(p => p.X == dto.X && p.Y == dto.Y);
            if (pu != null)
            {
                pu.Collected = true; // or remove from list
                // _map.PowerUps.Remove(pu); // CheckPowerUpPickup usually does collection logic
            }
        }

        private void OnPlayerEliminated(string username)
        {
            // If someone else died, I win! (Assuming 2 players)
            // Ideally check if username != MyUsername, but we don't store MyUsername locally well yet.
            // But ReportDeath is sent by the dying client.
            // So if I receive this, and I am alive, it's Victory.
            
            if (_isPlayerAlive) 
            {
                _isVictory = true;
                _isPlayerAlive = false; // Stop game logic (Technical "End State", but visually Victory)
                // Actually, keeping _isPlayerAlive=false stops updates, so it's a good "Game Over" flag.
                // But we need to distinguish Victory vs Defeat in Draw.
            }
        }

        private void OnRemotePlayerMoved(PlayerStateDTO state)
        {
            // If username is mine, ignore (loopback protection)
            // But we don't have unique IDs yet, so rely on different connection or "Remote" flag
            // For now, let's assume valid remote update.
            // Initialize remote player if null
            if (_remotePlayer == null)
            {
                 var rp = new BasePlayer(state.X, state.Y);
                 _remotePlayer = rp;
            }
            
            // Cast to BasePlayer/Concrete to set position directly if interface doesn't support it
            // BasePlayer has SetPosition? We need to check or add it.
            // Refactored BasePlayer has SetPosition(bx, by)
            // Cast to BasePlayer/Concrete to set position directly if interface doesn't support it
            // BasePlayer has SetPosition? We need to check or add it.
            // Refactored BasePlayer has SetPosition(bx, by)
            if (_remotePlayer is BasePlayer bp)
            {
                bp.SetPosition(state.X, state.Y);
            }
            else if (_remotePlayer is PlayerDecorator dec && dec is IPlayer inner)
            {
                 // Complex decoding if we really use decorators on remote. 
                 // For now assumes simple BasePlayer.
            }
        }

        private void OnRemoteExplosion(int x, int y, int power)
        {
             // 1. Find the bomb at these coordinates (visual removal)
             var bomb = _map.Bombs.FirstOrDefault(b => b.X == x && b.Y == y && !b.IsExploded);
             if (bomb != null)
             {
                 bomb.IsExploded = true;
                 // Ideally trigger its local OnExplode observers if needed, 
                 // but _map.OnExplosion handles the logic below.
             }

             // 2. Trigger Logic (Damage walls, kill players)
             _map.OnExplosion(x, y, power);
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
                    // Apply movement locally
                    _player.Move(moveX, moveY, _map);
                }
            }

            if (_isPlayerAlive && !_isVictory)
            {
                // HOST (Player 1) controls Enemies
                if (_playerIndex == 1)
                {
                    _map.UpdateEnemies(_player, _remotePlayer);
                    
                    // Broadcast Enemy Movements
                    foreach (var enemy in _map.Enemies)
                    {
                        if (enemy.IsAlive) 
                        {
                            Game.GameClient.MoveEnemyAsync(new Shared.DTOs.EnemyMovementDTO
                            {
                                EnemyId = enemy.VisualId,
                                X = enemy.X,
                                Y = enemy.Y
                            });
                        }
                    }
                }
                
                // EVERYONE updates local state (Movements, etc)
                double playerDelta = gameTime.ElapsedGameTime.TotalSeconds;
                _player.Update(playerDelta);
                
                CheckPowerUpPickup();
                
                // Check collisions 
                CheckPlayerEnemyCollision();
            // End of Input Block
            
            // Network Update of Player Position
            var newPos = _player.GetPosition();
            var playerState = new PlayerStateDTO
            {
                Username = _username,
                X = newPos.X,
                Y = newPos.Y,
                IsAlive = _isPlayerAlive
            };

            Task.Run(() => Game.GameClient.SendMovementAsync(playerState));

            // ============================
            // BOMBALAR
            // ============================
            foreach (var bomb in _map.Bombs)
            {
                bomb.Update(dt);

                // Only explode LOCAL bombs automatically (timer).
                // Remote bombs wait for server signal "OnExplosion".
                // FIX: To prevent double-damage on HardWalls (Local + Server),
                // we now wait for Server signal for ALL bombs (Local & Remote).
                /*
                if (!bomb.IsRemote && !bomb.IsExploded && bomb.TimeRemaining <= 0)
                {
                    bomb.Explode(); 
                }
                */
            }

            // ============================
            // PLAYER DECORATOR SÜRELERİ (Expiry Check)
            // ============================
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

                    // Notify Server
                    Game.GameClient.CollectPowerUpAsync(new Shared.DTOs.PowerUpDTO
                    {
                        X = p.X,
                        Y = p.Y,
                        Type = p.Type.ToString()
                    });
                }
            }
        }

        private async void KillPlayerAt(int x, int y)
        {
            // Simple center check.
            int px = (int)Math.Floor(_player.GetPosition().X + 0.5);
            int py = (int)Math.Floor(_player.GetPosition().Y + 0.5);

            if (_isPlayerAlive && px == x && py == y)
            {
                _isPlayerAlive = false;
                _isVictory = false; // Explicit defeat
                // Report to Server
                 await Game.GameClient.ReportDeathAsync("Me"); 
                 // We send "Me" or generated username if we had it. 
                 // Server broadcasts "Me" (or whatever). 
                 // Since we don't have unique IDs, this is a bit loose but works for 1v1.
                 // Ideally Game1 should store Username.
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
            Game.GraphicsDevice.Clear(new Color(30, 30, 35));

            Game.GameView.DrawGame(_map, _player, _remotePlayer, _playerIndex, _map.Bombs);

            if (!_isPlayerAlive || _isVictory)
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
            // === TITLE ===
            string title = _isVictory ? "VICTORY!" : "DEFEAT";
            Color titleColor = _isVictory ? Color.Gold : Color.Crimson;

            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (vp.Width - titleSize.X * 3f) / 2, // 3x scale calculation
                vp.Height / 2 - 120);

            // Shadow text
            SpriteBatch.DrawString(_font, title, titlePos + new Vector2(4, 4), Color.Black * 0.5f, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
            // Main text
            SpriteBatch.DrawString(_font, title, titlePos, titleColor, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

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

        private void OnRemoteBombPlaced(Shared.BombDTO dto)
        {
             // Create visual bomb entity
             var bomb = new Bomb(dto.X, dto.Y, dto.Power);
             bomb.Attach(Game.GameView); 
             bomb.IsRemote = true; // Mark as remote so it doesn't self-destruct locally
             
             // Note: We don't attach Map/Enemies here because explosion logic is also synchronized via "ExplosionReceived"
             // or we can let the server drive explosions.
             // Currently ExplosionReceived handles the blast.
             // But we need to add to _map.Bombs so it gets drawn.
             _map.Bombs.Add(bomb);
        }

        private void OnRemoteEnemyMoved(Shared.DTOs.EnemyMovementDTO dto)
        {
            // Find enemy by VisualId
            var enemy = _map.Enemies.FirstOrDefault(e => e.VisualId == dto.EnemyId);
            if (enemy != null)
            {
                // Snap position (Simple sync)
                // Since setter is private/or we need a method.
                // Enemy properties X/Y have private setters.
                // We might need to add a method 'SetPosition' to Enemy class or use Reflection.
                // Let's check Enemy class. It likely has private setters.
                // Assuming we need to add SetPosition to Enemy.
                enemy.SetPosition(dto.X, dto.Y); 
            }
        }
    }
}