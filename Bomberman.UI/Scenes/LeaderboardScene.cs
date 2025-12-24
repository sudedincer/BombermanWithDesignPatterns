using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Bomberman.Services.Data;

namespace Bomberman.UI.Scenes
{
    public class LeaderboardScene : Scene
    {
        private SpriteFont _font;
        private List<User> _topPlayers = new List<User>();
        private Rectangle _backBtn;
        private bool _isLoading = true;

        public LeaderboardScene(Game1 game) : base(game)
        {
            _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");
            SetupLayout();
            LoadData();
        }

        private void SetupLayout()
        {
            int w = Game.GraphicsDevice.Viewport.Width;
            int h = Game.GraphicsDevice.Viewport.Height;
            _backBtn = new Rectangle(20, h - 70, 200, 50);  // Increased from 150
        }

        private async void LoadData()
        {
            _isLoading = true;
            try
            {
                _topPlayers = await Game.GameClient.GetLeaderboardAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load leaderboard: " + ex.Message);
            }
            _isLoading = false;
        }

        public override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed)
            {
                if (_backBtn.Contains(ms.Position))
                {
                    // Return to Lobby (assuming user is already logged in, we kept the session)
                    // Note: We need to pass the username back if we want to keep state perfect, 
                    // but for now let's just use the Game._username if accessible or just go back.
                    // Actually Game1 stores _username, so JoinLobbyWithAuth works perfectly.
                    // BUT Game1._username is private... let's check.
                    // Ah, Game1.JoinLobbyWithAuth sets _username. But Game1._username is private. 
                    // However, we are inside Game1, so we can access public methods.
                    // Wait, we don't have public access to username.
                    // Solution: Just switch scene to LobbyScene directly. 
                    // LobbyScene doesn't require username in constructor, it uses Game1 methods.
                    SceneManager.ChangeScene(new LobbyScene(Game)); 
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Modern gradient background
            Game.GraphicsDevice.Clear(new Color(15, 20, 28));
            SpriteBatch.Begin();

            int centerX = Game.GraphicsDevice.Viewport.Width / 2;

            // === Title with medals ===
            string title = "HALL OF FAME";
            Vector2 titleSize = _font.MeasureString(title) * 1.6f;
            Vector2 titlePos = new Vector2(centerX - titleSize.X / 2, 40);
            
            // Title shadow
            SpriteBatch.DrawString(_font, title, titlePos + new Vector2(3, 3), Color.Black * 0.5f, 0, Vector2.Zero, 1.6f, SpriteEffects.None, 0);
            // Title
            SpriteBatch.DrawString(_font, title, titlePos, new Color(255, 215, 0), 0, Vector2.Zero, 1.6f, SpriteEffects.None, 0);

            if (_isLoading)
            {
                string loading = "Loading leaderboard...";
                Vector2 loadingSize = _font.MeasureString(loading) * 1.2f;
                SpriteBatch.DrawString(_font, loading, 
                    new Vector2(centerX - loadingSize.X / 2, 200), 
                    Color.Yellow, 0, Vector2.Zero, 1.2f, SpriteEffects.None, 0);
            }
            else
            {
                // === Leaderboard Card ===
                Rectangle leaderboardCard = new Rectangle(centerX - 400, 130, 800, 500);
                DrawCard(leaderboardCard, new Color(25, 30, 38));

                // Table header
                int headerY = leaderboardCard.Y + 20;
                string header = "  #   Player         W    L    K  ";
                SpriteBatch.DrawString(_font, header, 
                    new Vector2(leaderboardCard.X + 40, headerY), 
                    new Color(255, 200, 100), 0, Vector2.Zero, 0.95f, SpriteEffects.None, 0);

                // Header underline
                SpriteBatch.Draw(Game.Pixel, 
                    new Rectangle(leaderboardCard.X + 20, headerY + 30, leaderboardCard.Width - 40, 2), 
                    new Color(255, 200, 100) * 0.5f);

                // Player rows
                int rowY = headerY + 50;
                int rank = 1;
                
                foreach (var player in _topPlayers)
                {
                    // Simple rank numbers instead of emojis
                    string rankStr = $"{rank}.";

                    // Row highlight for top 3
                    if (rank <= 3)
                    {
                        Color highlight = rank == 1 ? new Color(255, 215, 0) * 0.15f :
                                        rank == 2 ? new Color(192, 192, 192) * 0.15f :
                                        new Color(205, 127, 50) * 0.15f;
                        SpriteBatch.Draw(Game.Pixel, 
                            new Rectangle(leaderboardCard.X + 15, rowY - 5, leaderboardCard.Width - 30, 35), 
                            highlight);
                    }

                    // Stats display - fixed width columns
                    string playerName = player.Username.Length > 11 ? player.Username.Substring(0, 11) : player.Username;
                    string stats = $"  {rankStr,-3} {playerName,-11}  {player.Wins,2}   {player.Losses,2}   {player.Kills,2}  ";
                    
                    Color textColor = rank switch
                    {
                        1 => new Color(255, 215, 0),    // Gold
                        2 => new Color(192, 192, 192),  // Silver
                        3 => new Color(205, 127, 50),   // Bronze
                        _ => Color.LightGray
                    };
                    
                    SpriteBatch.DrawString(_font, stats, 
                        new Vector2(leaderboardCard.X + 40, rowY), 
                        textColor, 0, Vector2.Zero, 0.9f, SpriteEffects.None, 0);
                    
                    rowY += 40;
                    rank++;

                    // Limit display to fit in card
                    if (rank > 10) break;
                }

                if (_topPlayers.Count == 0)
                {
                    string noData = "No records found. Play some games!";
                    Vector2 noDataSize = _font.MeasureString(noData);
                    SpriteBatch.DrawString(_font, noData, 
                        new Vector2(centerX - noDataSize.X / 2, rowY), 
                        Color.Gray, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
                }
            }

            // === Back Button ===
            Point mousePos = Mouse.GetState().Position;
            bool backHover = _backBtn.Contains(mousePos);
            DrawStyledButton(_backBtn, "BACK", new Color(70, 100, 180), backHover);

            // Footer
            string footer = "Statistics are updated in real-time";
            Vector2 footerSize = _font.MeasureString(footer) * 0.7f;
            SpriteBatch.DrawString(_font, footer, 
                new Vector2(centerX - footerSize.X / 2, Game.GraphicsDevice.Viewport.Height - 40), 
                Color.Gray * 0.6f, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);

            SpriteBatch.End();
        }

        private void DrawCard(Rectangle rect, Color bgColor)
        {
            // Main card
            SpriteBatch.Draw(Game.Pixel, rect, bgColor);
            
            // Border
            int thickness = 2;
            Color borderColor = new Color(255, 200, 100) * 0.4f;
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), borderColor);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), borderColor);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), borderColor);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), borderColor);
        }

        private void DrawStyledButton(Rectangle rect, string text, Color bgColor, bool hover)
        {
            // Shadow
            Rectangle shadow = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
            SpriteBatch.Draw(Game.Pixel, shadow, Color.Black * 0.3f);

            // Button background
            Color finalBg = hover ? bgColor * 1.3f : bgColor;
            SpriteBatch.Draw(Game.Pixel, rect, finalBg);

            // Border
            int borderThickness = hover ? 2 : 1;
            Color borderColor = hover ? Color.White * 0.9f : Color.White * 0.4f;
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
            SpriteBatch.Draw(Game.Pixel, new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);

            // Text
            Vector2 textSize = _font.MeasureString(text);
            float scale = hover ? 1.05f : 1f;
            Vector2 scaledSize = textSize * scale;
            SpriteBatch.DrawString(_font, text, 
                new Vector2(rect.Center.X - scaledSize.X / 2, rect.Center.Y - scaledSize.Y / 2), 
                Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
