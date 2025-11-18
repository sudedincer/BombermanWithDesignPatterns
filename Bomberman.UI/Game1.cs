using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Patterns.Creational;
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
    public class Game1 : Game
    {
        private readonly ConcurrentQueue<(int x, int y, int power)> _explosionQueue = new();
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameView _gameView;
        private GameMap _gameMap; 
        private List<Player> _players; 
        private List<Bomb> _bombs; 
        private GameClient _gameClient;
        private string _username = "Player1"; 
        private IWallFactory _wallFactory;
        private KeyboardState _previousKeyboardState; // Tekrarlanan basışı engeller
        private bool _isGameOver = false;
        private readonly int _mapWidth = 15;
        private readonly int _mapHeight = 13;

        private Texture2D _overlayPixel; // Game Over overlay için 1x1 texture

        // SignalR Hub adresi
        private const string HUB_URL = "http://localhost:5077/gamehub";

        public Game1()
        {
            int tileSize = 64;

            _graphics = new GraphicsDeviceManager(this);
            // Pencere boyutunu harita kare sayısına göre ayarla
            _graphics.PreferredBackBufferWidth = _mapWidth * tileSize;
            _graphics.PreferredBackBufferHeight = _mapHeight * tileSize;
            _graphics.ApplyChanges(); 
    
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
    
            // Factory Method başlatılıyor
            _wallFactory = new DesertWallFactory(); 

            // GameMap başlatılıyor (Factory Method deseni kullanılıyor)
            _gameMap = new GameMap(_mapWidth, _mapHeight, _wallFactory);
    
            // Temel varlıklar başlatılıyor
            _players = new List<Player> { new Player(1, 1) };
            _bombs = new List<Bomb>();
        }

        protected override void Initialize()
        {
            // SignalR Client başlatılıyor
            _gameClient = new GameClient(HUB_URL);

            // Bağlantıyı başlat
            Task.Run(() => _gameClient.StartConnectionAsync());
            
            // Patlama olayına abone ol
            _gameClient.ExplosionReceived += HandleExplosion;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameView = new GameView(_spriteBatch, GraphicsDevice);

            // Game Over overlay için 1x1 beyaz texture
            _overlayPixel = new Texture2D(GraphicsDevice, 1, 1);
            _overlayPixel.SetData(new[] { Color.White });
        }
        // Patlama geldiğinde çağrılacak metod (SignalR thread'inde çalışır)

        // Game1.cs'de HandleExplosion metodu (Sadece kuyruğa ekleme)
        private void HandleExplosion(int x, int y, int power)
        {
            // Veriyi kuyruğa ekle ve Update metodunda işlenmesini bekle.
            _explosionQueue.Enqueue((x, y, power)); 
        }
        
        protected override void Update(GameTime gameTime)
{
    KeyboardState currentKeyboardState = Keyboard.GetState();

    // ============================
    // 1) GAME OVER DURUMU
    // ============================
    if (!_players[0].IsAlive)
    {
        // R tuşu ile yeniden başlatma
        if (currentKeyboardState.IsKeyDown(Keys.R) &&
            !_previousKeyboardState.IsKeyDown(Keys.R))
        {
            RestartGame();
        }

        _previousKeyboardState = currentKeyboardState;
        _gameView.Update(gameTime);
        base.Update(gameTime);
        return;   // <-- ÇOK ÖNEMLİ: aşağıdaki oyun loop'unu durdurur.
    }


    // ============================
    // 2) NORMAL OYUN DÖNGÜSÜ
    // ============================

    // Mevcut input
    var (deltaX, deltaY, shouldPlaceBomb) = InputController.GetCurrentInput();

    // Network'ten gelen patlamaları işleme
    while (_explosionQueue.TryDequeue(out var explosionData))
    {
        ProcessExplosion(explosionData.x, explosionData.y, explosionData.power);
    }

    // Hareket
    if (deltaX != 0 || deltaY != 0)
    {
        _players[0].Move(
            deltaX * gameTime.ElapsedGameTime.TotalSeconds,
            deltaY * gameTime.ElapsedGameTime.TotalSeconds,
            _gameMap
        );

        var playerState = new PlayerStateDTO
        {
            Username = _username,
            X = _players[0].GetPosition().X,
            Y = _players[0].GetPosition().Y,
            IsAlive = true
        };
        foreach (var bomb in _bombs)
        {
            bomb.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        Task.Run(() => _gameClient.SendMovementAsync(playerState));
    }

    // Bomba koyma (tek tuş basışı)
    if (currentKeyboardState.IsKeyDown(Keys.Space) &&
        !_previousKeyboardState.IsKeyDown(Keys.Space))
    {
        int bombX = (int)Math.Round(_players[0].GetPosition().X);
        int bombY = (int)Math.Round(_players[0].GetPosition().Y);

        var bombDto = new BombDTO
        {
            PlacedByUsername = _username,
            X = bombX,
            Y = bombY,
            Power = 2
        };

        Task.Run(() => _gameClient.PlaceBombAsync(bombDto));

        _bombs.Add(new Bomb(bombX, bombY, bombDto.Power));
    }

    // View update
    _gameView.Update(gameTime);

    // Bir sonraki frame için tuş kaydı
    _previousKeyboardState = currentKeyboardState;

    base.Update(gameTime);
}
        

        private void ProcessExplosion(int x, int y, int power)
        {
            Console.WriteLine($"[CLIENT DEBUG] ProcessExplosion Başlatıldı: ({x}, {y}), power={power}");

            // 1) Patlayan bombayı listeden kaldır
            var bombToRemove = _bombs.FirstOrDefault(b =>
                (int)Math.Round((decimal)b.X) == x &&
                (int)Math.Round((decimal)b.Y) == y);

            if (bombToRemove != null)
            {
                _bombs.Remove(bombToRemove);
                Console.WriteLine($"[DEBUG] Bomba ({x}, {y}) listeden kaldırıldı.");
            }

            // 2) Merkez hücreye patlama uygula
            ApplyExplosionToCell(x, y);

            // 3) 4 yöne power kadar ilerle (classic Bomberman cross)
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int dir = 0; dir < 4; dir++)
            {
                int cx = x;
                int cy = y;

                for (int step = 1; step <= power; step++)
                {
                    cx += dx[dir];
                    cy += dy[dir];

                    // Hücre patlama ile işlendi; false dönerse o yönde propagation durur
                    if (!ApplyExplosionToCell(cx, cy))
                        break;
                }
            }
        }
        
        /// <summary>
        /// Verilen grid hücresine patlama uygular.
        /// Görsel ekler, duvarları yıkar, oyuncu öldürür.
        /// return: true = patlama bu yönde devam edebilir, false = bu hücrede durur.
        /// </summary>
        private bool ApplyExplosionToCell(int x, int y)
        {
            // Harita sınırı dışı ise patlama durur
            if (_gameMap.IsOutsideBounds(x, y))
                return false;

            Wall wall = _gameMap.GetWallAt(x, y);

            // DUVAR VARSA
            if (wall != null && !wall.IsDestroyed)
            {
                if (wall is BreakableWall bw)
                {
                    bw.OnExplosion(x, y, 1);
                    _gameMap.RemoveWall(x, y);
                }
                else if (wall is HardWall hw)
                {
                    hw.OnExplosion(x, y, 1);
                    if (hw.IsDestroyed)
                    {
                        _gameMap.RemoveWall(x, y);
                    }
                }
                // UnbreakableWall veya başka bir wall: sadece patlama etkilenir, propagation bu noktada durur.

                _gameView.AddExplosionVisual(x, y);
                KillPlayersAt(x, y);
                return false; // duvarda patlama durur
            }

            // DUVAR YOKSA → patlama görseli + oyuncu kontrolü
            _gameView.AddExplosionVisual(x, y);
            KillPlayersAt(x, y);

            return true; // bu yönde patlama devam edebilir
        }
        
        private void KillPlayersAt(int x, int y)
        {
            foreach (var player in _players)
            {
                if (!player.IsAlive)
                    continue;

                int px = (int)Math.Round(player.GetPosition().X);
                int py = (int)Math.Round(player.GetPosition().Y);

                if (px == x && py == y)
                {
                    player.IsAlive = false;
                    Console.WriteLine($"[DEBUG] Oyuncu patlamada öldü: ({x}, {y})");
                }
            }
        }
        
        private void RestartGame()
        {
            _gameMap = new GameMap(_mapWidth, _mapHeight, _wallFactory);

            _players = new List<Player>
            {
                new Player(1, 1)
            };

            _bombs.Clear();

            while (_explosionQueue.TryDequeue(out _)) {}

            _previousKeyboardState = Keyboard.GetState();
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _gameView.DrawGame(_gameMap, _players, _bombs);

            // Oyuncu öldüyse GAME OVER overlay
            if (!_players[0].IsAlive)
            {
                DrawGameOverOverlay();
            }

            base.Draw(gameTime);
        }
        private void DrawGameOverOverlay()
        {
            _spriteBatch.Begin();

            // Arka plan karartma
            var full = new Rectangle(0, 0,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight);
            _spriteBatch.Draw(_overlayPixel, full, Color.Black * 0.6f);

            // GAME OVER yazısını çiz
            DrawOutlinedText("GAME OVER",
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2 - 50,
                4, Color.White, Color.Black);

            // Restart yazısı
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
        private readonly Dictionary<char, string[]> _font = new Dictionary<char, string[]>
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

                // Outline (optional, looks better)
                Rectangle outlineRect = new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
                _spriteBatch.Draw(_overlayPixel, outlineRect, outline);

                // Fill
                _spriteBatch.Draw(_overlayPixel, rect, fill);
            }
        }
    }
}

    }
}