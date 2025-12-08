using Bomberman.Core.Enums;
using Bomberman.Services.Network;
using Bomberman.UI.Scenes;
using Bomberman.UI.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.UI
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameView _gameView;
        private GameClient _gameClient;

        private readonly int _mapWidth = 15;
        private readonly int _mapHeight = 13;
        private readonly int _tileSize = 64;

        public SpriteBatch SpriteBatch => _spriteBatch;
        public Texture2D Pixel { get; private set; }
        public GameView GameView => _gameView;
        public GameClient GameClient => _gameClient;
        public int MapWidth => _mapWidth;
        public int MapHeight => _mapHeight;

        private const string HUB_URL = "http://localhost:5077/gamehub";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = _mapWidth * _tileSize;
            _graphics.PreferredBackBufferHeight = _mapHeight * _tileSize;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _gameClient = new GameClient(HUB_URL);
            _ = _gameClient.StartConnectionAsync();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 1x1 pixel
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });

            // GameView + texture'lar
            _gameView = new GameView(_spriteBatch, GraphicsDevice);
            var powerUpsTex = Content.Load<Texture2D>("Textures/powerups");
            _gameView.SetPowerUpTexture(powerUpsTex);

            var enemyTex1 = Content.Load<Texture2D>("Textures/enemy");
            var enemyTex2 = Content.Load<Texture2D>("Textures/enemy2");
            var enemyTex3 = Content.Load<Texture2D>("Textures/enemy3");
            _gameView.SetEnemyTextures(new System.Collections.Generic.List<Texture2D> { enemyTex1, enemyTex2, enemyTex3 });

            var playerTex = Content.Load<Texture2D>("Textures/player");
            _gameView.SetPlayerTexture(playerTex);

            var bombTex = Content.Load<Texture2D>("Textures/bomb");
            _gameView.SetBombTexture(bombTex);

            try
            {
                var explosionTex = Content.Load<Texture2D>("Textures/explosion");
                _gameView.SetExplosionTexture(explosionTex);
            }
            catch { /* If texture missing, keep default red box */ }

            // Default olarak desert atlas yerine split texture yüklüyoruz (preview amaçlı veya null kalmasın diye)
            var t1 = Content.Load<Texture2D>("Textures/desert_unbreakable");
            var t2 = Content.Load<Texture2D>("Textures/desert_hard");
            var t3 = Content.Load<Texture2D>("Textures/desert_hard_damaged");
            var t4 = Content.Load<Texture2D>("Textures/desert_breakable");
            _gameView.SetWallTextures(t1, t2, t3, t4);

            // Başlangıç sahnesi: Lobby
            SceneManager.ChangeScene(new LobbyScene(this));
        }

        protected override void Update(GameTime gameTime)
        {
            SceneManager.CurrentScene?.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SceneManager.CurrentScene?.Draw(gameTime);
            base.Draw(gameTime);
        }

        public void StartGame(ThemeType theme)
        {
            SceneManager.ChangeScene(new GameScene(this, theme));
        }
    }
}