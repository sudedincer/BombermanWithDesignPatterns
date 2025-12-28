using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Shared;

using Shared.DTOs;

namespace Bomberman.Services.Network
{
    public class GameHub : Hub
    {
        private readonly ExplosionService _explosionService;
        private readonly Bomberman.Services.Data.IUserRepository _userRepository; // Inject Repository
        
        // Static state to track players
        private static List<string> _connectedConnectionIds = new List<string>(); 
        private static Dictionary<string, string> _connectionUsernames = new Dictionary<string, string>(); // Map ID -> Username
        private static string? _selectedTheme = null; 
        private static readonly object _lock = new();

        public GameHub(ExplosionService explosionService, Bomberman.Services.Data.IUserRepository userRepository)
        {
            _explosionService = explosionService;
            _userRepository = userRepository;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_lock)
            {
                _connectedConnectionIds.Remove(Context.ConnectionId);
                _connectionUsernames.Remove(Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinLobby(string username, string? theme = null)
        {
            // Database Registration / Check
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser == null)
            {
                var newUser = new Bomberman.Services.Data.User 
                { 
                    Username = username,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserAsync(newUser);
                Console.WriteLine($"[DB] New User Registered: {username}");
            }
            else
            {
               Console.WriteLine($"[DB] Existing User Logged In: {username} (Wins: {existingUser.Wins})");
            }

            int playerCount;
            bool ready = false;
            bool isFirstPlayer = false;

            lock (_lock)
            {
                if (!_connectedConnectionIds.Contains(Context.ConnectionId))
                {
                    _connectedConnectionIds.Add(Context.ConnectionId);
                    _connectionUsernames[Context.ConnectionId] = username;
                }
                playerCount = _connectedConnectionIds.Count;
                
                // Determine if this user is the host (first in the list)
                if (_connectedConnectionIds.Count > 0 && _connectedConnectionIds[0] == Context.ConnectionId)
                {
                    isFirstPlayer = true;
                    
                    // Allow host to update theme regardless of whether they are new or existing
                    if (!string.IsNullOrEmpty(theme))
                    {
                        _selectedTheme = theme;
                        Console.WriteLine($"[SERVER] Host selected/updated theme: {theme}");
                    }
                }
                
                if (playerCount >= 2)
                {
                    ready = true;
                }
            }

            Console.WriteLine($"[SERVER] {username} joined. ID: {Context.ConnectionId}. Total: {playerCount}");

            // Send personalized lobby update to the joining player
            await Clients.Caller.SendAsync("LobbyUpdated", new LobbyStateDTO 
            { 
                PlayerCount = playerCount, 
                IsReady = ready,
                IsFirstPlayer = isFirstPlayer,
                SelectedTheme = _selectedTheme,
                Message = isFirstPlayer ? "You are the first player. Select a theme!" : $"Waiting for game to start. Theme: {_selectedTheme ?? "Not selected"}" 
            });

            // Broadcast to others
            await Clients.Others.SendAsync("LobbyUpdated", new LobbyStateDTO 
            { 
                PlayerCount = playerCount, 
                IsReady = ready,
                IsFirstPlayer = false,
                SelectedTheme = _selectedTheme,
                Message = $"{username} joined the lobby." 
            });

            // If 2 players, Start Game
            if (ready)
            {
                int seed = new Random().Next();
                string gameTheme = _selectedTheme ?? "Desert"; // Use selected theme or default to Desert 

                Console.WriteLine($"[SERVER] Starting Game with Seed: {seed}");

                // Send P1 Start
                if (_connectedConnectionIds.Count > 0)
                {
                    await Clients.Client(_connectedConnectionIds[0]).SendAsync("GameStarted", new GameStartDTO 
                    { 
                        Seed = seed, 
                        Theme = gameTheme,
                        PlayerIndex = 1 
                    });
                }

                // Send P2 Start
                if (_connectedConnectionIds.Count > 1)
                {
                    await Clients.Client(_connectedConnectionIds[1]).SendAsync("GameStarted", new GameStartDTO 
                    { 
                        Seed = seed, 
                        Theme = gameTheme,
                        PlayerIndex = 2
                    });
                }
            }
        }

        public async Task ReportDeath(string username)
        {
            Console.WriteLine($"[SERVER] {username} DIED.");
            
            // 1. Update Loser Stats
            await _userRepository.UpdateStatsAsync(username, isWin: false);
            
            // 2. Find Winner and Update Stats
            string? winnerName = null;
            lock (_lock)
            {
                // Iterate over connected players, find the one who is NOT the loser
                foreach (var kvp in _connectionUsernames)
                {
                    if (kvp.Value != username)
                    {
                        winnerName = kvp.Value;
                        break;
                    }
                }
            }
            
            if (winnerName != null)
            {
                Console.WriteLine($"[SERVER] Winner identified: {winnerName}");
                await _userRepository.UpdateStatsAsync(winnerName, isWin: true);
            }

            await Clients.All.SendAsync("PlayerEliminated", username);
        }

        public async Task SendMovement(PlayerStateDTO state)
        {
            // Console.WriteLine($"[SERVER LOG] Movement from {state.Username} ({state.X}, {state.Y})");
            await Clients.Others.SendAsync("ReceiveMovement", state);
        }

        public async Task MoveEnemy(EnemyMovementDTO movement)
        {
            await Clients.Others.SendAsync("EnemyMoved", movement);
        }

        public async Task PlaceBomb(BombDTO bomb)
        {
            Console.WriteLine($"[SERVER LOG] Bomb placed by {bomb.PlacedByUsername} at ({bomb.X}, {bomb.Y})");

            // Bomba görseli için direkt bilgilendir
            await Clients.Others.SendAsync("ReceiveBombPlacement", bomb);

            // Patlamayı arkada çalıştır
            _ = Task.Run(async () =>
            {
                Console.WriteLine("[SERVER] Bomb countdown started...");
                await Task.Delay(3000); // 3 saniye bekle

                Console.WriteLine("[SERVER] Triggering explosion...");
                await _explosionService.TriggerExplosion(bomb.X, bomb.Y, bomb.Power);
            });
        }
        
        public async Task SpawnPowerUp(PowerUpDTO powerUp)
        {
            await Clients.Others.SendAsync("ReceivePowerUpSpawn", powerUp);
        }

        public async Task CollectPowerUp(PowerUpDTO powerUp)
        {
            await Clients.All.SendAsync("ReceivePowerUpCollection", powerUp);
        }

        public async Task<bool> Login(string username, string password)
        {
            Console.WriteLine($"[AUTH] Login Attempt: {username}");
            var user = await _userRepository.GetByUsernameAsync(username);
            
            if (user == null) 
            {
                Console.WriteLine($"[AUTH] Login Failed: User {username} not found.");
                return false;
            }
            
            // Simple check
            bool success = user.PasswordHash == password;
            Console.WriteLine($"[AUTH] Login {(success ? "Success" : "Failed (Bad Pass)")}: {username}");
            return success;
        }

        public async Task<bool> Register(string username, string password)
        {
             Console.WriteLine($"[AUTH] Register Attempt: {username}");
             // Check if exists
             var existing = await _userRepository.GetByUsernameAsync(username);
             if (existing != null) 
             {
                 Console.WriteLine($"[AUTH] Register Failed: {username} already exists.");
                 return false;
             }

             var newUser = new Bomberman.Services.Data.User 
             { 
                 Username = username, 
                 PasswordHash = password, // Ideally hash this!
                 CreatedAt = DateTime.UtcNow 
             };
             
             bool result = await _userRepository.AddUserAsync(newUser);
             Console.WriteLine($"[AUTH] Register Result for {username}: {result}");
             return result;
        }

        public async Task<List<Bomberman.Services.Data.User>> GetLeaderboard()
        {
            var users = await _userRepository.GetTopPlayersAsync(10);
            return users.ToList();
        }

        public async Task RequestGameNavigation(GameNavigationDTO navigation)
        {
            Console.WriteLine($"[SERVER] Game Navigation Request: {navigation.Action} by {navigation.RequestedBy}");
            
            // If Restart, Server determines the new Seed for map synchronization
            if (navigation.Action == "Restart")
            {
                navigation.Seed = new Random().Next();
                Console.WriteLine($"[SERVER] Generated Seed for Restart: {navigation.Seed}");
            }
            
            // Broadcast to ALL players (including sender) so everyone navigates together
            await Clients.All.SendAsync("GameNavigationRequested", navigation);
        }
    }
}