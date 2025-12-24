using System;
using Bomberman.Services.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.UI.Scenes
{
    public class LoginScene : Scene
    {
        private SpriteFont _font;
        private string _username = "";
        private string _password = "";
        
        private bool _isFocusOnUsername = true;
        private string _statusMessage = "Enter credentials";
        private Color _statusColor = Color.LightGray;

        private Rectangle _panelRect;
        private Rectangle _userBox;
        private Rectangle _passBox;
        private Rectangle _loginBtn;
        private Rectangle _registerBtn;

        private float _cursorTimer;
        private bool _isBusy = false; // Prevent double clicks
        
        // Colors
        private Color _bgColor = new Color(20, 24, 30);
        private Color _panelColor = new Color(35, 40, 48);
        private Color _inputColor = new Color(25, 30, 35);
        private Color _accentColor = new Color(255, 190, 80); // Gold

        public LoginScene(Game1 game) : base(game)
        {
            _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");
            Game.Window.TextInput += OnTextInput;
            SetupLayout();
            
            // Check connection immediately
            if (!Game.GameClient.IsConnected)
            {
                 _statusMessage = "Connecting to Server...";
                 _statusColor = Color.Yellow;
            }
        }

        private void SetupLayout()
        {
            int w = Game.GraphicsDevice.Viewport.Width;
            int h = Game.GraphicsDevice.Viewport.Height;
            
            int panelW = 400;
            int panelH = 320; // Slightly shorter panel
            _panelRect = new Rectangle((w - panelW)/2, (h - panelH)/2, panelW, panelH);

            // Centering helper
            int centerX = w / 2;
            
            int boxW = 320;
            int boxH = 45;
            int startY = _panelRect.Y + 80;

            _userBox = new Rectangle(centerX - boxW/2, startY, boxW, boxH);
            _passBox = new Rectangle(centerX - boxW/2, startY + 70, boxW, boxH);
            
            int btnW = 150;  // Increased from 140
            int gap = 20;
            int btnY = startY + 140;
            
            // Calculate Side by Side buttons from center
            _loginBtn = new Rectangle(centerX - btnW - gap/2, btnY, btnW, 50);
            _registerBtn = new Rectangle(centerX + gap/2, btnY, btnW, 50);
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (e.Key == Keys.Tab)
            {
                _isFocusOnUsername = !_isFocusOnUsername;
                return;
            }
            if (e.Key == Keys.Enter)
            {
                AttemptLogin();
                return;
            }
            if (e.Key == Keys.Back)
            {
                if (_isFocusOnUsername && _username.Length > 0)
                    _username = _username.Substring(0, _username.Length - 1);
                else if (!_isFocusOnUsername && _password.Length > 0)
                    _password = _password.Substring(0, _password.Length - 1);
                return;
            }

            if (_font.Characters.Contains(e.Character) && e.Key != Keys.Enter && e.Key != Keys.Tab)
            {
                if (_isFocusOnUsername && _username.Length < 15) _username += e.Character;
                if (!_isFocusOnUsername && _password.Length < 15) _password += e.Character;
            }
        }

        private async void AttemptLogin()
        {
             if (_isBusy) return;
             
             if (!Game.GameClient.IsConnected)
            {
                _statusMessage = "Server Offline! Check Connection.";
                _statusColor = Color.Red;
                return;
            }

            _isBusy = true;
            _statusMessage = "Logging in...";
            _statusColor = Color.Yellow;

            try
            {
                bool success = await Game.GameClient.LoginAsync(_username, _password);
                _isBusy = false;
                
                if (success)
                {
                    _statusMessage = "Success!";
                    _statusColor = Color.Green;
                    Game.Window.TextInput -= OnTextInput; 
                    Game.JoinLobbyWithAuth(_username); 
                }
                else
                {
                    _statusMessage = "Login Failed! User/Pass incorrect.";
                    _statusColor = Color.Red;
                }
            }
             catch (Exception ex)
            {
                Console.WriteLine("Login Error: " + ex.Message);
                _isBusy = false;
                _statusMessage = "Network Error!";
                _statusColor = Color.Red;
            }
        }

        private async void AttemptRegister()
        {
             if (_isBusy) return;

             if (!Game.GameClient.IsConnected)
            {
                _statusMessage = "Server Offline! Check Connection.";
                _statusColor = Color.Red;
                return;
            }

            _isBusy = true;
            _statusMessage = "Registering...";
            _statusColor = Color.Yellow;

            try
            {
                bool success = await Game.GameClient.RegisterAsync(_username, _password);
                _isBusy = false;

                if (success)
                {
                    _statusMessage = "Registered! Pls Login.";
                    _statusColor = Color.Green;
                }
                else
                {
                    _statusMessage = "Username taken / Exists!";
                    _statusColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine("Register Error: " + ex.Message);
                _isBusy = false;
                _statusMessage = "Network Error!";
                _statusColor = Color.Red;
            }
        }


        public override void Update(GameTime gameTime)
        {
            _cursorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState ms = Mouse.GetState();
            
            // Only trigger on click DOWN (pressed this frame, but not last frame)
            if (ms.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
            {
                Point p = ms.Position;
                if (_userBox.Contains(p)) _isFocusOnUsername = true;
                if (_passBox.Contains(p)) _isFocusOnUsername = false;
                
                if (_loginBtn.Contains(p) && !_isBusy) AttemptLogin();
                if (_registerBtn.Contains(p) && !_isBusy) AttemptRegister();
            }
            
            _prevMouseState = ms;
        }

        private MouseState _prevMouseState;

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(_bgColor);
            SpriteBatch.Begin();

            // Background Panel
            SpriteBatch.Draw(Game.Pixel, _panelRect, _panelColor);
            DrawBorder(_panelRect, 1, Color.Gray * 0.5f);

            // Title
            DrawCenteredString("BOMBERMAN LOGIN", _panelRect.Y + 30, Color.White, 1.2f);

            // Inputs
            DrawInput(_userBox, _username, "Username", _isFocusOnUsername);
            string maskedPass = new string('•', _password.Length);
            DrawInput(_passBox, maskedPass, "Password", !_isFocusOnUsername);

            // Buttons
            Point mousePos = Mouse.GetState().Position;
            DrawButton(_loginBtn, "LOGIN", _loginBtn.Contains(mousePos) ? _accentColor : _inputColor);
            DrawButton(_registerBtn, "REGISTER", _registerBtn.Contains(mousePos) ? _accentColor : _inputColor);

            // Status
            DrawCenteredString(_statusMessage, _registerBtn.Bottom + 25, _statusColor, 0.9f);

            SpriteBatch.End();
        }

        private void DrawInput(Rectangle r, string text, string placeholder, bool hasFocus)
        {
            SpriteBatch.Draw(Game.Pixel, r, _inputColor);
            DrawBorder(r, 1, hasFocus ? _accentColor : Color.Gray * 0.5f);
            
            string display = string.IsNullOrEmpty(text) ? placeholder : text;
            Color textColor = string.IsNullOrEmpty(text) ? Color.Gray : Color.White;
            
            if (hasFocus && (int)(_cursorTimer * 2) % 2 == 0) 
            {
                if (!string.IsNullOrEmpty(text)) display += "|";
                else display = "|"; // Show cursor even if empty
            }
            
            Vector2 size = _font.MeasureString(display);
            // Center vertically, left align with padding
            Vector2 pos = new Vector2(r.X + 15, r.Center.Y - size.Y/2);
            
            SpriteBatch.DrawString(_font, display, pos, textColor);
        }

        private void DrawButton(Rectangle r, string text, Color bg)
        {
            SpriteBatch.Draw(Game.Pixel, r, bg);
            // Hover border check? Usually confusing with current method, skipping for cleanliness
            // But we can add a subtle border
            DrawBorder(r, 1, Color.White * 0.1f);

            Color textColor = (bg == _accentColor) ? Color.Black : Color.White;
            Vector2 size = _font.MeasureString(text);
            SpriteBatch.DrawString(_font, text, new Vector2(r.Center.X - size.X/2, r.Center.Y - size.Y/2), textColor);
        }

        private void DrawCenteredString(string text, int y, Color color, float scale = 1f)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 pos = new Vector2((Game.GraphicsDevice.Viewport.Width - size.X)/2, y);
            SpriteBatch.DrawString(_font, text, pos, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
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
