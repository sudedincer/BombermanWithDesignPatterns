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
        private bool _isFirstPlayer = false;
        private bool _canSelectTheme = true; // Start as true, disable for second player
        private string _lobbyMessage = "";
        
        // Input
        private string _usernameInput = "";
        private Rectangle _inputRect;
        private bool _isTyping = true; // Focus on start

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
            Game.Window.TextInput += OnTextInput; // Subscribe to text input

            _desertPreview = Game.Content.Load<Texture2D>("Textures/lobby_desert");
            _forestPreview = Game.Content.Load<Texture2D>("Textures/lobby_forest");
            _cityPreview   = Game.Content.Load<Texture2D>("Textures/lobby_city");

            // Subscribe to lobby updates
            Game.GameClient.LobbyUpdated += OnLobbyUpdated;

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
            
            // Input Box above Start Button
            _inputRect = new Rectangle(
                (screenW - 300) / 2,
                _centerPanel.Bottom - btnH - 100,
                300,
                40);

            _startBtn = new Rectangle(
                (screenW - btnW) / 2,
                _centerPanel.Bottom - btnH - 30,
                btnW,
                btnH);
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (!_isTyping || _waitingForGame) return;

            if (e.Key == Keys.Back)
            {
                if (_usernameInput.Length > 0)
                    _usernameInput = _usernameInput.Substring(0, _usernameInput.Length - 1);
            }
            else if (e.Key == Keys.Enter)
            {
               // Enter handled in Update
            }
            else
            {
                // Simple filter (letters, numbers, space)
                if (_font.Characters.Contains(e.Character))
                {
                    if (_usernameInput.Length < 12) // Max length
                        _usernameInput += e.Character;
                }
            }
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
                // Only allow theme selection if this is the first player
                if (_canSelectTheme)
                {
                    if (_desertCard.Contains(p)) _selectedTheme = ThemeType.Desert;
                    if (_forestCard.Contains(p)) _selectedTheme = ThemeType.Forest;
                    if (_cityCard.Contains(p))   _selectedTheme = ThemeType.City;
                }

                if (_startBtn.Contains(p))
                {
                    if (string.IsNullOrWhiteSpace(_usernameInput)) return; // Prevent empty join

                    // Send theme if first player, otherwise send null
                    string? themeToSend = _canSelectTheme ? _selectedTheme.ToString() : null;
                    Game.JoinLobby(_usernameInput, themeToSend);
                    _waitingForGame = true;
                    Game.Window.TextInput -= OnTextInput; // Unsubscribe
                }
            }

            var kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Enter) && !string.IsNullOrWhiteSpace(_usernameInput))
            {
                string? themeToSend = _canSelectTheme ? _selectedTheme.ToString() : null;
                Game.JoinLobby(_usernameInput, themeToSend);
                _waitingForGame = true;
                Game.Window.TextInput -= OnTextInput;
            }
            if (kb.IsKeyDown(Keys.Escape))
            {
                Game.Exit();
            }

            _hoverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _prevMouse = mouse;
        }

        private void OnLobbyUpdated(Shared.LobbyStateDTO lobbyState)
        {
            _isFirstPlayer = lobbyState.IsFirstPlayer;
            _lobbyMessage = lobbyState.Message;
            
            // If this is the second player (not first), disable theme selection
            if (!lobbyState.IsFirstPlayer)
            {
                _canSelectTheme = false;
                
                // Update our local theme to match the first player's selection
                if (!string.IsNullOrEmpty(lobbyState.SelectedTheme))
                {
                    if (Enum.TryParse<ThemeType>(lobbyState.SelectedTheme, out var theme))
                    {
                        _selectedTheme = theme;
                    }
                }
            }
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

            // Cards - only show if can select theme
            if (_canSelectTheme)
            {
                DrawThemeCard(_desertCard, "DESERT", _desertPreview, _selectedTheme == ThemeType.Desert, desertHover);
                DrawThemeCard(_forestCard, "FOREST", _forestPreview, _selectedTheme == ThemeType.Forest, forestHover);
                DrawThemeCard(_cityCard,   "CITY",   _cityPreview,   _selectedTheme == ThemeType.City,   cityHover);
            }
            else if (!_isFirstPlayer && !string.IsNullOrEmpty(_lobbyMessage))
            {
                // Show message for second player
                string msg = $"Selected Theme: {_selectedTheme}";
                Vector2 msgSize = _font.MeasureString(msg);
                Vector2 msgPos = new Vector2(
                    _centerPanel.Center.X - msgSize.X / 2,
                    _centerPanel.Center.Y - msgSize.Y / 2);
                SpriteBatch.DrawString(_font, msg, msgPos, _whiteText);
            }

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
                DrawInputBox();
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
        private void DrawInputBox()
        {
            SpriteBatch.Draw(Game.Pixel, _inputRect, new Color(50, 50, 50));
            DrawBorder(_inputRect, 1, Color.Gray);

            string text = _usernameInput;
            if (_isTyping && (int)(_hoverTimer * 2) % 2 == 0) text += "|"; // Cursor blink

            if (string.IsNullOrEmpty(_usernameInput) && !_isTyping)
            {
                text = "Enter Username..."; 
            }

            Vector2 size = _font.MeasureString(text);
            Vector2 pos = new Vector2(
                _inputRect.X + 10,
                _inputRect.Center.Y - size.Y / 2);
            
            SpriteBatch.DrawString(_font, text, pos, Color.White);
        }
    }
}