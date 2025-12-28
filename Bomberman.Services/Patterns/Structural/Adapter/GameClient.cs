using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Shared.DTOs;

namespace Bomberman.Services.Patterns.Structural.Adapter
{
    /// <summary>
    /// ADAPTER CLASS for SignalR Integration
    /// 
    /// ADAPTER PATTERN IMPLEMENTATION:
    /// - Target Interface: IGameClient - defines methods expected by game code
    /// - Adaptee: HubConnection (SignalR's incompatible interface)
    /// - Adapter: GameClient (this class) - adapts HubConnection to IGameClient
    /// 
    /// This class wraps SignalR's HubConnection and provides a game-specific interface.
    /// It translates between the game's expected interface and SignalR's actual implementation,
    /// allowing the game code to remain decoupled from the specific network library being used.
    /// 
    /// Benefits:
    /// - Game code depends on IGameClient abstraction, not SignalR specifics
    /// - Easy to swap SignalR with another network library by creating a new adapter
    /// - Centralized network logic with clear separation of concerns
    /// </summary>
    public class GameClient : IGameClient
    {
        // ADAPTEE: The SignalR HubConnection being adapted
        private readonly HubConnection _connection;
        
        // Properties
        public bool IsConnected => _connection.State == HubConnectionState.Connected;

        // Events - Server to Client Communication
        public event Action<PlayerStateDTO>? PlayerMoved;
        public event Action<int, int, int>? ExplosionReceived;
        public event Action<LobbyStateDTO>? LobbyUpdated;
        public event Action<BombDTO> BombPlaced;
        public event Action<EnemyMovementDTO> EnemyMoved;
        public event Action<PowerUpDTO> PowerUpSpawned;
        public event Action<PowerUpDTO> PowerUpCollected;
        public event Action<GameStartDTO>? GameStarted;
        public event Action<string>? PlayerEliminated;
        public event Action<GameNavigationDTO>? GameNavigationRequested;

        public GameClient(string hubUrl)
        {
            // Create and configure the ADAPTEE (SignalR HubConnection)
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl) 
                .Build();

            // Configure event handlers - adapting SignalR's message handling to game events
            ConfigureEventHandlers();
        }

        /// <summary>
        /// Configure all SignalR message handlers
        /// This is part of the adaptation - converting SignalR messages to game events
        /// </summary>
        private void ConfigureEventHandlers()
        {
            // Player Movement Synchronization
            _connection.On<PlayerStateDTO>("ReceiveMovement", (state) =>
            {
                PlayerMoved?.Invoke(state);
            });
            
            // Bomb Placement
            _connection.On<BombDTO>("ReceiveBombPlacement", (bomb) =>
            {
                BombPlaced?.Invoke(bomb);
            });
            
            // Explosion
            _connection.On<int, int, int>("ReceiveExplosion", (x, y, power) =>
            {
                ExplosionReceived?.Invoke(x, y, power);
            });
            
            // Lobby Updates
            _connection.On<LobbyStateDTO>("LobbyUpdated", (lobby) =>
            {
                LobbyUpdated?.Invoke(lobby);
            });

            // Game Start
            _connection.On<GameStartDTO>("GameStarted", (dto) =>
            {
                GameStarted?.Invoke(dto);
            });

            // Enemy Movement
            _connection.On<EnemyMovementDTO>("EnemyMoved", (dto) =>
            {
                EnemyMoved?.Invoke(dto);
            });

            // PowerUp Spawned
            _connection.On<PowerUpDTO>("ReceivePowerUpSpawn", (dto) =>
            {
                PowerUpSpawned?.Invoke(dto);
            });

            // PowerUp Collected
            _connection.On<PowerUpDTO>("ReceivePowerUpCollection", (dto) =>
            {
                PowerUpCollected?.Invoke(dto);
            });

            // Player Elimination
            _connection.On<string>("PlayerEliminated", (username) =>
            {
                PlayerEliminated?.Invoke(username);
            });

            // Game Navigation (Restart/Lobby)
            _connection.On<GameNavigationDTO>("GameNavigationRequested", (dto) =>
            {
                GameNavigationRequested?.Invoke(dto);
            });
        }

        // Connection Management - Adapted Methods
        public async Task StartConnectionAsync()
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("Connected to GameHub");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }
        
        // Lobby & Game Flow - Adapted Methods
        public async Task JoinLobbyAsync(string username, string? theme = null)
        {
            await _connection.InvokeAsync("JoinLobby", username, theme);
        }

        public async Task RequestGameNavigationAsync(GameNavigationDTO navigation)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("RequestGameNavigation", navigation);
            }
        }

        // Player Actions - Adapted Methods
        public async Task SendMovementAsync(PlayerStateDTO state)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SendMovement", state);
            }
        }
        
        public async Task PlaceBombAsync(BombDTO bomb)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("PlaceBomb", bomb);
            }
        }

        public async Task ReportDeathAsync(string username)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("ReportDeath", username);
            }
        }

        // Enemy & PowerUp Actions - Adapted Methods
        public async Task MoveEnemyAsync(EnemyMovementDTO dto)
        {
            await _connection.InvokeAsync("MoveEnemy", dto);
        }

        public async Task SpawnPowerUpAsync(PowerUpDTO dto)
        {
            await _connection.InvokeAsync("SpawnPowerUp", dto);
        }

        public async Task CollectPowerUpAsync(PowerUpDTO dto)
        {
            await _connection.InvokeAsync("CollectPowerUp", dto);
        }

        // Authentication & Leaderboard - Adapted Methods
        public async Task<bool> LoginAsync(string username, string password)
        {
            if (_connection.State != HubConnectionState.Connected) return false;
            return await _connection.InvokeAsync<bool>("Login", username, password);
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (_connection.State != HubConnectionState.Connected) return false;
            return await _connection.InvokeAsync<bool>("Register", username, password);
        }

        public async Task<List<Bomberman.Services.Data.User>> GetLeaderboardAsync()
        {
            if (_connection.State != HubConnectionState.Connected) 
                return new List<Bomberman.Services.Data.User>();
            return await _connection.InvokeAsync<List<Bomberman.Services.Data.User>>("GetLeaderboard");
        }
    }
}
