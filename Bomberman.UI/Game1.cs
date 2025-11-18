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

        // SignalR Hub adresi
        private const string HUB_URL = "http://localhost:5077/gamehub";

        public Game1()
        {
            int tileSize = 64;
            int mapWidth = 15;
            int mapHeight = 13;
            _graphics = new GraphicsDeviceManager(this);
            // Pencere boyutunu harita kare sayısına göre ayarla
            _graphics.PreferredBackBufferWidth = mapWidth * tileSize;
            _graphics.PreferredBackBufferHeight = mapHeight * tileSize;
            _graphics.ApplyChanges(); 
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // Factory Method başlatılıyor
            _wallFactory = new DesertWallFactory(); 

            // GameMap başlatılıyor (Factory Method deseni kullanılıyor)
            _gameMap = new GameMap(mapWidth, mapHeight, _wallFactory);
            
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
            // Oyun zaten bittiyse, sadece Draw metodu çalışmalı (Update'i atla)
            if (_isGameOver)
            {
                return; 
            }
            // 1. OYUN BİTTİ KONTROLÜ
            if (_players.Any() && !_players[0].IsAlive) 
            {
                _isGameOver = true;
                // Oyuncu ölünce yapılacak diğer temizlik işleri (Network bağlantısını kesme vb.)
            }
            // Mevcut klavye durumunu yakala
             KeyboardState currentKeyboardState = Keyboard.GetState();
    
            // 1. Girdiyi yakala
            var (deltaX, deltaY, shouldPlaceBomb) = InputController.GetCurrentInput();
            
            // 2. KRİTİK: Kuyruktaki patlama olaylarını güvenli bir şekilde işle
            // Bu, Network thread'inden gelen verinin ana MonoGame thread'inde işlenmesini sağlar.
            while (_explosionQueue.TryDequeue(out var explosionData))
            {
                // Model güncellemesi ve görsel efektler için ana işleyiciyi çağır
                ProcessExplosion(explosionData.x, explosionData.y, explosionData.power);
            }
            
            // 3. Yerel Oyuncuyu Güncelle ve Network'e Gönder
            if (deltaX != 0 || deltaY != 0)
            {
                // Çarpışma kontrolü ile hareket
                _players[0].Move(deltaX * gameTime.ElapsedGameTime.TotalSeconds,
                    deltaY * gameTime.ElapsedGameTime.TotalSeconds,
                    _gameMap); 

                // Network'e gönder
                var playerState = new PlayerStateDTO
                {
                    Username = _username,
                    X = _players[0].GetPosition().X,
                    Y = _players[0].GetPosition().Y,
                    IsAlive = true
                };
                // Network çağrısı
                Task.Run((Func<Task>)(() => _gameClient.SendMovementAsync(playerState)));
            }

    // 4. Bomba Koyma İşlemi (TEK TUŞ BASIŞI Kontrolü)
    if (currentKeyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
    {
        int bombX = (int)Math.Round(_players[0].GetPosition().X);
        int bombY = (int)Math.Round(_players[0].GetPosition().Y);
        
        var bombDto = new BombDTO
        {
            PlacedByUsername = _username,
            X = bombX,
            Y = bombY,
            Power = 1 
        };
    
        // Network'e gönder
        Task.Run((Func<Task>)(() => _gameClient.PlaceBombAsync(bombDto)));
        
        // Yerel olarak Bombayı hemen Model'e ekle (Görselin anında görünmesi için)
        _bombs.Add(new Bomb(bombX, bombY, bombDto.Power));
    }

    // 5. Bir sonraki kare için klavye durumunu kaydet ve View'ı güncelle
    _previousKeyboardState = currentKeyboardState;
    _gameView.Update(gameTime); // Patlama görsel zamanlayıcısı burada işler

    base.Update(gameTime);
}   
        

        private void ProcessExplosion(int x, int y, int power)
        {
            Console.WriteLine($"[CLIENT DEBUG] ProcessExplosion Başlatıldı: ({x}, {y})");

            // 1) Bombayı kaldır
            var bombToRemove = _bombs.FirstOrDefault(b =>
                (int)Math.Round((decimal)b.X) == x &&
                (int)Math.Round((decimal)b.Y) == y);

            if (bombToRemove != null)
                _bombs.Remove(bombToRemove);

            // 2) Merkez hücreye patlama uygula
            ApplyExplosionToCell(x, y);

            // 3) 4 yöne power kadar ilerle
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int d = 0; d < 4; d++)
            {
                int cx = x;
                int cy = y;

                for (int step = 1; step <= power; step++)
                {
                    cx += dx[d];
                    cy += dy[d];

                    if (!ApplyExplosionToCell(cx, cy))
                        break; // o yönde patlama durur
                }
            }
        }
        
        private bool ApplyExplosionToCell(int x, int y)
        {
            // Harita sınırı kontrolü
            if (x < 0 || y < 0 || x >= _gameMap.Width || y >= _gameMap.Height)
                return false;

            Wall wall = _gameMap.GetWallAt(x, y);

            // DUVAR VARSA
            if (wall != null && !wall.IsDestroyed)
            {
                // BreakableWall
                if (wall is BreakableWall bw)
                {
                    bw.OnExplosion(x, y, 1);
                    _gameMap.RemoveWall(x, y);
                }
                // HardWall
                else if (wall is HardWall hw)
                {
                    hw.OnExplosion(x, y, 1);
                    if (hw.IsDestroyed)
                        _gameMap.RemoveWall(x, y);
                }

                // NOT: Bomberman kuralı → duvar varsa patlama o hücrede DURUR
                _gameView.AddExplosionVisual(x, y);
                return false;
            }

            // OYUNCU VAR MI?
            foreach (var p in _players)
            {
                if (!p.IsAlive) continue;

                int px = (int)Math.Round(p.GetPosition().X);
                int py = (int)Math.Round(p.GetPosition().Y);

                if (px == x && py == y)
                {
                    p.IsAlive = false;
                    Console.WriteLine("[DEBUG] Oyuncu öldü!");
                }
            }

            // GÖRSEL EKLE
            _gameView.AddExplosionVisual(x, y);

            // boş kare → patlama devam edebilir
            return true;
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Oyun bittiyse (IsAlive = false) pop-up'ı çiz
            if (_isGameOver)
            {
                _spriteBatch.Begin();
        
                string message = "OYUN BİTTİ!";
        
                // Basit bir pop-up paneli (siyah arkaplan)
                Texture2D rect = new Texture2D(GraphicsDevice, 300, 100);
                Color[] data = new Color[300 * 100];
                for(int i=0; i<data.Length; ++i) data[i] = Color.Black;
                rect.SetData(data);
        
                Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Bounds.Center.X, GraphicsDevice.Viewport.Bounds.Center.Y);
                Vector2 rectPosition = screenCenter - new Vector2(150, 50); // Paneli merkezle

                _spriteBatch.Draw(rect, rectPosition, Color.White); // Paneli çiz
        
                _spriteBatch.End();
                return; // Oyun bitince normal çizimi durdur
            }
    
            // Normal oyun çizimi
            _gameView.DrawGame(_gameMap, _players, _bombs);

            base.Draw(gameTime);
        }
    }
}