using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Shared;

using Shared.DTOs;

namespace Bomberman.Services.Network
{
    public class GameHub : Hub
    {
        private readonly ExplosionService _explosionService;
        
        // Static state to track players
        private static List<string> _connectedConnectionIds = new List<string>(); // New List
        private static readonly object _lock = new();

        public GameHub(ExplosionService explosionService)
        {
            _explosionService = explosionService;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_lock)
            {
                _connectedConnectionIds.Remove(Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinLobby(string username)
        {
            int playerCount;
            bool ready = false;

            lock (_lock)
            {
                if (!_connectedConnectionIds.Contains(Context.ConnectionId))
                {
                    _connectedConnectionIds.Add(Context.ConnectionId);
                }
                playerCount = _connectedConnectionIds.Count;
                
                if (playerCount >= 2)
                {
                    ready = true;
                }
            }

            Console.WriteLine($"[SERVER] {username} joined. ID: {Context.ConnectionId}. Total: {playerCount}");

            // Broadcast Lobby Update
            await Clients.All.SendAsync("LobbyUpdated", new LobbyStateDTO 
            { 
                PlayerCount = playerCount, 
                IsReady = ready,
                Message = $"{username} joined the lobby." 
            });

            // If 2 players, Start Game
            if (ready)
            {
                int seed = new Random().Next();
                string theme = "City"; 

                Console.WriteLine($"[SERVER] Starting Game with Seed: {seed}");

                // Send P1 Start
                if (_connectedConnectionIds.Count > 0)
                {
                    await Clients.Client(_connectedConnectionIds[0]).SendAsync("GameStarted", new GameStartDTO 
                    { 
                        Seed = seed, 
                        Theme = theme,
                        PlayerIndex = 1 
                    });
                }

                // Send P2 Start
                if (_connectedConnectionIds.Count > 1)
                {
                    await Clients.Client(_connectedConnectionIds[1]).SendAsync("GameStarted", new GameStartDTO 
                    { 
                        Seed = seed, 
                        Theme = theme,
                        PlayerIndex = 2
                    });
                }
                
                // Clear state for next game (optional, but good for demo loop)
                lock (_lock) { _connectedConnectionIds.Clear(); }
            }
        }

        public async Task ReportDeath(string username)
        {
            Console.WriteLine($"[SERVER] {username} DIED.");
            await Clients.All.SendAsync("PlayerEliminated", username);
        }

        public async Task SendMovement(PlayerStateDTO state)
        {
            Console.WriteLine($"[SERVER LOG] Movement from {state.Username} ({state.X}, {state.Y})");
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
    }
}