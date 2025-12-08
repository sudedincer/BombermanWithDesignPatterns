using System;
using Bomberman.Core.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bomberman.UI.Scenes
{
    public class LobbyScene : Scene
    {
        private SpriteFont _font;
        private ThemeType _selectedTheme = ThemeType.Desert;
        private bool _waitingForGame = false;

        // UI Layout
        private Rectangle _desertCard;
        private Rectangle _forestCard;
        private Rectangle _cityCard;
        private Rectangle _startBtn;
        private Rectangle _centerPanel;

        // Textures
        private Texture2D _desertPreview;
        private Texture2D _forestPreview;
        private Texture2D _cityPreview;
        
        // Colors - Modern Minimalist Palette
        private Color _bgColor = new Color(20, 24, 30);        // Soft Dark Blue/Gray
        private Color _panelBgColor = new Color(30, 34, 40, 240); // Slightly lighter panel
        private Color _cardBgColor = new Color(40, 44, 50);    // Clean card bg
        private Color _cardBgHover = new Color(50, 54, 60);    // Light hover
        private Color _selectedColor = new Color(255, 190, 80); // Soft Gold
        private Color _whiteText = new Color(230, 230, 230);   // Off-white for text
        
        private MouseState _prevMouse;
        private float _hoverTimer; 

        public LobbyScene(Game1 game) : base(game)
        {
            _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");

            _desertPreview = Game.Content.Load<Texture2D>("Textures/lobby_desert");
            _forestPreview = Game.Content.Load<Texture2D>("Textures/lobby_forest");
            _cityPreview   = Game.Content.Load<Texture2D>("Textures/lobby_city");

            SetupLayout();
        }

        private void SetupLayout()
        {
            int screenW = Game.GraphicsDevice.Viewport.Width;
            int screenH = Game.GraphicsDevice.Viewport.Height;

            // Smaller, more focused panel
            int panelW = 750;
            int panelH = 400;
            _centerPanel = new Rectangle(
                (screenW - panelW) / 2,
                (screenH - panelH) / 2,
                panelW,
                panelH);

            int cardW = 200;
            int cardH = 220; // Shorter cards
            int gap = 30;
            int startY = _centerPanel.Y + 80;

            int totalW = (cardW * 3) + (gap * 2);
            int startX = _centerPanel.X + (_centerPanel.Width - totalW) / 2;

            _desertCard = new Rectangle(startX, startY, cardW, cardH);
            _forestCard = new Rectangle(startX + cardW + gap, startY, cardW, cardH);
            _cityCard   = new Rectangle(startX + (cardW + gap) * 2, startY, cardW, cardH);

            int btnW = 200;
            int btnH = 50;
            _startBtn = new Rectangle(
                (screenW - btnW) / 2,
                _centerPanel.Bottom - btnH - 30,
                btnW,
                btnH);
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            Point p = mouse.Position;

            bool leftClicked = 
                mouse.LeftButton == ButtonState.Pressed &&
                _prevMouse.LeftButton == ButtonState.Released;

            if (leftClicked)
            {
                if (_desertCard.Contains(p)) _selectedTheme = ThemeType.Desert;
                if (_forestCard.Contains(p)) _selectedTheme = ThemeType.Forest;
                if (_cityCard.Contains(p))   _selectedTheme = ThemeType.City;

                if (_startBtn.Contains(p))
                {
                    // Game.StartGame(_selectedTheme); // Old Logic
                    Game.JoinLobby(); // New Multiplayer Logic
                    _waitingForGame = true;
                }
            }

            var kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Enter))
            {
                Game.JoinLobby();
                _waitingForGame = true;
            }
            if (kb.IsKeyDown(Keys.Escape))
            {
                Game.Exit();
            }

            _hoverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _prevMouse = mouse;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(_bgColor);

            SpriteBatch.Begin();

            // Background Pattern: None. Clean.

            // Center Panel (Soft Shadow + Clean Rect)
            Rectangle shadowRect = _centerPanel;
            shadowRect.Offset(8, 8);
            SpriteBatch.Draw(Game.Pixel, shadowRect, Color.Black * 0.3f);
            SpriteBatch.Draw(Game.Pixel, _centerPanel, _panelBgColor);
            
            // Title: Clean, Modern, White
            string title = "Select Arena"; // Simplifed text
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                _centerPanel.Center.X - titleSize.X / 2,
                _centerPanel.Y + 30);
            
            SpriteBatch.DrawString(_font, title, titlePos, _whiteText);

            Point mousePos = Mouse.GetState().Position;
            bool desertHover = _desertCard.Contains(mousePos);
            bool forestHover = _forestCard.Contains(mousePos);
            bool cityHover   = _cityCard.Contains(mousePos);
            bool startHover  = _startBtn.Contains(mousePos);

            // Cards
            DrawThemeCard(_desertCard, "DESERT", _desertPreview, _selectedTheme == ThemeType.Desert, desertHover);
            DrawThemeCard(_forestCard, "FOREST", _forestPreview, _selectedTheme == ThemeType.Forest, forestHover);
            DrawThemeCard(_cityCard,   "CITY",   _cityPreview,   _selectedTheme == ThemeType.City,   cityHover);

            // Start Button
            if (_waitingForGame)
            {
                string waitMsg = "Waiting for players... (1/2)";
                Vector2 textSize = _font.MeasureString(waitMsg);
                Vector2 pos = new Vector2(
                    _centerPanel.Center.X - textSize.X / 2,
                    _startBtn.Top + 10);
                SpriteBatch.DrawString(_font, waitMsg, pos, Color.White);
            }
            else
            {
                DrawButton(_startBtn, "JOIN GAME", startHover);
            }

            SpriteBatch.End();
        }

        private void DrawThemeCard(Rectangle r, string title, Texture2D preview, bool isSelected, bool isHovered)
        {
            // Hover effect: slight lift (visualized by offset/growth is tricky with fixed rects, so just color/border)
            Color bg = isSelected ? _cardBgHover : _cardBgColor;
            if (isHovered) bg = Color.Lerp(bg, Color.White, 0.05f);

            SpriteBatch.Draw(Game.Pixel, r, bg);

            // Selection Border - Clean
            if (isSelected)
            {
                DrawBorder(r, 2, _selectedColor);
            }
            else if (isHovered)
            {
                DrawBorder(r, 1, Color.White * 0.3f);
            }

            // Preview Image - Clean, centered in top half
            int imgSize = 80;
            int imgY = r.Y + 40;
            int imgX = r.X + (r.Width - imgSize) / 2;
            
            // Draw image with slight scale if selected
            Rectangle destRect = new Rectangle(imgX, imgY, imgSize, imgSize);
            SpriteBatch.Draw(preview, destRect, Color.White);

            // Title - Bottom
            Vector2 textSize = _font.MeasureString(title);
            Vector2 textPos = new Vector2(
                r.X + (r.Width - textSize.X) / 2,
                r.Y + r.Height - 40);
            
            Color textColor = isSelected ? _selectedColor : Color.Gray;
            if (isHovered && !isSelected) textColor = Color.White;

            SpriteBatch.DrawString(_font, title, textPos, textColor);
        }

        private void DrawButton(Rectangle r, string text, bool isHovered)
        {
            Color bg = isHovered ? _selectedColor : new Color(60, 60, 70);
            Color textColor = isHovered ? new Color(20, 24, 30) : Color.White;

            SpriteBatch.Draw(Game.Pixel, r, bg);

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