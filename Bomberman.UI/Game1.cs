using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Behavioral.Observer;
using Bomberman.Core.Patterns.Creational;
using Bomberman.Core.PowerUps;
using Bomberman.Core.Walls;
using Bomberman.Services.Network;
using Bomberman.UI.Controller;
using Bomberman.UI.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared;

namespace Bomberman.UI
{
    public class Game1 : Game, IExplosionObserver
    {
        private readonly ConcurrentQueue<(int x, int y, int power)> _explosionQueue = new();

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private GameView _gameView;
        private GameMap _gameMap;

        private IPlayer _player;                // Decorator kullanılan oyuncu (BasePlayer + power-ups)
        private bool _isPlayerAlive;

        private readonly List<Bomb> _bombs = new();
        private GameClient _gameClient;

        private string _username = "Player1";
        private IWallFactory _wallFactory;
        private KeyboardState _previousKeyboardState;

        private readonly int _mapWidth = 15;
        private readonly int _mapHeight = 13;

        private Texture2D _overlayPixel; // Game Over overlay için 1x1 texture

        private const string HUB_URL = "http://localhost:5077/gamehub";

        private Texture2D _powerUpTexture;
        private Texture2D _enemyTexture;
        int tileSize = 64;
        private Texture2D _playerTexture;
        private Texture2D _bombTexture;
        public Game1()
        {
            

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = _mapWidth * tileSize;
            _graphics.PreferredBackBufferHeight = _mapHeight * tileSize;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _wallFactory = new DesertWallFactory();
            _gameMap = new GameMap(_mapWidth, _mapHeight, _wallFactory);

            // Oyuncu: BasePlayer ile başlıyoruz, power-up aldıkça decorator ile sarılacak
            _player = new BasePlayer(1, 1);
            _isPlayerAlive = true;
        }

        protected override void Initialize()
        {
            _gameClient = new GameClient(HUB_URL);

            Task.Run(() => _gameClient.StartConnectionAsync());

            _gameClient.ExplosionReceived += HandleExplosion;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameView = new GameView(_spriteBatch, GraphicsDevice);
            _overlayPixel = new Texture2D(GraphicsDevice, 1, 1);
            _overlayPixel.SetData(new[] { Color.White });
            _powerUpTexture = Content.Load<Texture2D>("Textures/powerups");
            _gameView.SetPowerUpTexture(_powerUpTexture);
            _enemyTexture = Content.Load<Texture2D>("enemy");
            _gameView.SetEnemyTexture(_enemyTexture);
            _playerTexture = Content.Load<Texture2D>("player");
            _gameView.SetPlayerTexture(_playerTexture);
            _bombTexture = Content.Load<Texture2D>("bomb");
            _gameView.SetBombTexture(_bombTexture);
        }

        private void HandleExplosion(int x, int y, int power)
        {
            _explosionQueue.Enqueue((x, y, power));
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // ============================
            // 1) GAME OVER DURUMU
            // ============================
            if (!_isPlayerAlive)
            {
                if (currentKeyboardState.IsKeyDown(Keys.R) &&
                    !_previousKeyboardState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                }

                _previousKeyboardState = currentKeyboardState;
                _gameView.Update(gameTime);
                base.Update(gameTime);
                return;
            }

            // ============================
            // 2) NORMAL OYUN DÖNGÜSÜ
            // ============================

            var (deltaX, deltaY, _) = InputController.GetCurrentInput();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Network'ten gelen patlamaları işle
            while (_explosionQueue.TryDequeue(out var explosionData))
            {
                ProcessExplosion(explosionData.x, explosionData.y, explosionData.power);
            }

            // Oyuncu hareketi (Decorator'dan gelen speed ile)
            if (deltaX != 0 || deltaY != 0)
            {
                double speed = _player.GetSpeed();
                var pos = _player.GetPosition();

                double moveX = deltaX * speed * dt;
                double moveY = deltaY * speed * dt;

                double targetX = pos.X + moveX;
                double targetY = pos.Y + moveY;

                if (!_gameMap.IsWallAt(targetX, targetY))
                {
                    _player.Move(moveX, moveY, _gameMap);
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

                Task.Run(() => _gameClient.SendMovementAsync(playerState));
            }

            // Bombaları zamanla güncelle
            foreach (var bomb in _bombs)
            {
                bomb.Update(dt);
            }
            // Enemy movement update
            foreach (var enemy in _gameMap.Enemies)
            {
                enemy.Update(_gameMap, _player);
            }

            // Bomba koyma (Space, tek tuş basışı)
            if (currentKeyboardState.IsKeyDown(Keys.Space) &&
                !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                
                var pos = _player.GetPosition();

                int bombX = (int)Math.Round(pos.X);
                int bombY = (int)Math.Round(pos.Y);

                int power = _player.GetBombPower(); // Decorator'dan gelen bomba gücü

                var bombDto = new BombDTO
                {
                    PlacedByUsername = _username,
                    X = bombX,
                    Y = bombY,
                    Power = power
                };
                var bomb = new Bomb(bombX, bombY, power);

                bomb.Attach(_player);
                bomb.Attach(_gameMap);
                bomb.Attach(_gameView);

                foreach (var enemy in _gameMap.Enemies)
                    bomb.Attach(enemy);

                _bombs.Add(bomb);
               

                Task.Run(() => _gameClient.PlaceBombAsync(bombDto));

                
            }

            // Power-up toplama
            CheckPowerUpPickup();

            _gameView.Update(gameTime);
            _previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }
        
        

        private void CheckPowerUpPickup()
        {
            var pos = _player.GetPosition();

            int px = (int)Math.Round(pos.X);
            int py = (int)Math.Round(pos.Y);

            foreach (var p in _gameMap.PowerUps)
            {
                if (!p.Collected && p.X == px && p.Y == py)
                {
                    _player = p.Apply(_player); // decorator devreye giriyor
                    p.Collected = true;
                }
            }
        }

      /*  private void ProcessExplosion(int x, int y, int power)
        {
            // Bombayı kaldır
            var bombToRemove = _bombs.FirstOrDefault(b =>
                (int)Math.Round((decimal)b.X) == x &&
                (int)Math.Round((decimal)b.Y) == y);

            if (bombToRemove != null)
                _bombs.Remove(bombToRemove);

            // Merkez hücreye patlama uygula
            ApplyExplosionToCell(x, y);

            // Dört yöne yayılım
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            // Önce merkezde enemy öldür
            KillEnemiesAt(x, y);

            for (int dir = 0; dir < 4; dir++)
            {
                int cx = x;
                int cy = y;

                for (int step = 1; step <= power; step++)
                {
                    cx += dx[dir];
                    cy += dy[dir];

                    // Bu tile'a patlama uygula
                    if (!ApplyExplosionToCell(cx, cy))
                        break;

                    // ❗ DOĞRU: enemy öldürme TAM BURADA yapılmalı
                    KillEnemiesAt(cx, cy);
                }
            }
        }*/
      
      private void ProcessExplosion(int x, int y, int power)
      {
          var bomb = _bombs.FirstOrDefault(b =>
              (int)Math.Round((decimal)b.X) == x &&
              (int)Math.Round((decimal)b.Y) == y);

          if (bomb == null)
              return;

          bomb.IsExploded = true;
          bomb.Explode(_gameMap);
          _bombs.Remove(bomb);

          // Merkez hücreye etki
          HandleExplosionOnCell(x, y);

          // Patlamanın yayılacağı 4 yön
          int[] dx = { 1, -1, 0, 0 };
          int[] dy = { 0, 0, 1, -1 };

          for (int dir = 0; dir < 4; dir++)
          {
              int cx = x;
              int cy = y;

              for (int i = 1; i <= power; i++)
              {
                  cx += dx[dir];
                  cy += dy[dir];

                  // Hücreyi işliyoruz
                  if (!HandleExplosionOnCell(cx, cy))
                      break; // duvara veya limite çarptı
              }
          }
      }
      private bool HandleExplosionOnCell(int x, int y)
      {
          // sınır dışıysa devam etmeyi bırak
          if (_gameMap.IsOutsideBounds(x, y))
              return false;

          // explosion görseli
          _gameView.AddExplosionVisual(x, y);

          // player öldür
          KillPlayerAt(x, y);

          // enemy öldür
          KillEnemiesAt(x, y);

          // duvar kontrol et
          var wall = _gameMap.GetWallAt(x, y);
          if (wall != null && !wall.IsDestroyed)
          {
              // kırılabilir veya hard wall ise yık
              if (wall is BreakableWall || wall is HardWall)
              {
                  _gameMap.RemoveWall(x, y);
              }

              // unbreakable ise → patlama durur
              return false;
          }

          // boşsa → patlama devam eder
          return true;
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
                Console.WriteLine($"[DEBUG] Oyuncu patlamada öldü: ({x}, {y})");
            }
        }

        private void RestartGame()
        {
            _gameMap = new GameMap(_mapWidth, _mapHeight, _wallFactory);

            _player = new BasePlayer(1, 1);
            _isPlayerAlive = true;

            _bombs.Clear();

            while (_explosionQueue.TryDequeue(out _)) { }

            _previousKeyboardState = Keyboard.GetState();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _gameView.DrawGame(_gameMap, _player, _bombs);

            if (!_isPlayerAlive)
            {
                DrawGameOverOverlay();
            }

            base.Draw(gameTime);
        }

        private void DrawGameOverOverlay()
        {
            _spriteBatch.Begin();

            var full = new Rectangle(
                0, 0,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight);

            _spriteBatch.Draw(_overlayPixel, full, Color.Black * 0.6f);

            DrawOutlinedText("GAME OVER",
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2 - 50,
                4, Color.White, Color.Black);

            DrawOutlinedText("PRESS R TO RESTART",
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2 + 40,
                2, Color.Yellow, Color.Black);

            _spriteBatch.End();
        }

        private void DrawOutlinedText(string text, int centerX, int centerY, int scale, Color fill, Color outline)
        {
            int charWidth = 6 * scale;
            int charHeight = 8 * scale;
            int spacing = 2 * scale;

            int totalWidth = text.Length * (charWidth + spacing);
            int x = centerX - totalWidth / 2;
            int y = centerY - charHeight / 2;

            foreach (char c in text)
            {
                DrawChar(c, x, y, scale, fill, outline);
                x += charWidth + spacing;
            }
        }

        private readonly Dictionary<char, string[]> _font = new()
        {
            { 'A', new[]{ " XXX ",
                          "X   X",
                          "X   X",
                          "XXXXX",
                          "X   X",
                          "X   X",
                          "X   X" }},

            { 'E', new[]{ "XXXXX",
                          "X    ",
                          "X    ",
                          "XXX  ",
                          "X    ",
                          "X    ",
                          "XXXXX" }},

            { 'G', new[]{ " XXX ",
                          "X   X",
                          "X    ",
                          "X XXX",
                          "X   X",
                          "X   X",
                          " XXX " }},

            { 'M', new[]{ "X   X",
                          "XX XX",
                          "X X X",
                          "X   X",
                          "X   X",
                          "X   X",
                          "X   X" }},

            { 'O', new[]{ " XXX ",
                          "X   X",
                          "X   X",
                          "X   X",
                          "X   X",
                          "X   X",
                          " XXX " }},

            { 'V', new[]{ "X   X",
                          "X   X",
                          "X   X",
                          "X   X",
                          " X X ",
                          " X X ",
                          "  X  " }},

            { 'R', new[]{ "XXXX ",
                          "X   X",
                          "X   X",
                          "XXXX ",
                          "X X  ",
                          "X  X ",
                          "X   X" }},

            { 'P', new[]{ "XXXX ",
                          "X   X",
                          "X   X",
                          "XXXX ",
                          "X    ",
                          "X    ",
                          "X    " }},

            { 'S', new[]{ " XXXX",
                          "X    ",
                          "X    ",
                          " XXX ",
                          "    X",
                          "    X",
                          "XXXX " }},

            { 'T', new[]{ "XXXXX",
                          "  X  ",
                          "  X  ",
                          "  X  ",
                          "  X  ",
                          "  X  ",
                          "  X  " }},

            { ' ', new[]{ "     ",
                          "     ",
                          "     ",
                          "     ",
                          "     ",
                          "     ",
                          "     " }},
        };

        private void DrawChar(char c, int x, int y, int scale, Color fill, Color outline)
        {
            c = char.ToUpper(c);
            if (!_font.ContainsKey(c)) return;

            var rows = _font[c];

            for (int row = 0; row < rows.Length; row++)
            {
                for (int col = 0; col < rows[row].Length; col++)
                {
                    if (rows[row][col] == 'X')
                    {
                        var rect = new Rectangle(
                            x + col * scale,
                            y + row * scale,
                            scale,
                            scale
                        );

                        Rectangle outlineRect = new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
                        _spriteBatch.Draw(_overlayPixel, outlineRect, outline);
                        _spriteBatch.Draw(_overlayPixel, rect, fill);
                    }
                }
            }
        }
        
        private void CheckPlayerEnemyCollision()
        {
            foreach (var enemy in _gameMap.Enemies)
            {
                // Tile bazlı çarpışma
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
            var dead = _gameMap.Enemies
                .Where(e => (int)Math.Round(e.X) == x &&
                            (int)Math.Round(e.Y) == y)
                .ToList();

            foreach (var e in dead)
                _gameMap.Enemies.Remove(e);
        }
        
        public void SetPlayerTexture(Texture2D tex)
        {
            _playerTexture = tex;
        }


        public void OnExplosion(int x, int y, int power)
        {
            // Enemy öldür
            KillEnemiesAt(x, y);

            // Oyuncu öldür
            KillPlayerAt(x, y);

            // Duvar kır
            _gameMap.RemoveWall(x, y);

            // Görsel efekt -> zaten GameView içinde bu metot var
            _gameView.AddExplosionVisual(x, y);
        }
    }
    
}