using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Shared.DTOs;

namespace Bomberman.Services.Patterns.Structural.Adapter
{
    /// <summary>
    /// TARGET INTERFACE for the Adapter Pattern.
    /// 
    /// ADAPTER PATTERN - SignalR Integration:
    /// - Target Interface: IGameClient (this interface)
    /// - Adaptee: HubConnection (Microsoft.AspNetCore.SignalR.Client)
    /// - Adapter: GameClient class
    /// 
    /// This interface defines the methods that the game code expects to use for network communication.
    /// The GameClient class adapts SignalR's HubConnection to match this interface, allowing the game
    /// to work with SignalR without being tightly coupled to its specific implementation.
    /// </summary>
    public interface IGameClient
    {
        // Connection Management
        bool IsConnected { get; }
        Task StartConnectionAsync();

        // Lobby & Game Flow
        Task JoinLobbyAsync(string username, string? theme = null);
        Task RequestGameNavigationAsync(GameNavigationDTO navigation);

        // Player Actions
        Task SendMovementAsync(PlayerStateDTO state);
        Task PlaceBombAsync(BombDTO bomb);
        Task ReportDeathAsync(string username);

        // Enemy & PowerUp Actions
        Task MoveEnemyAsync(EnemyMovementDTO dto);
        Task SpawnPowerUpAsync(PowerUpDTO dto);
        Task CollectPowerUpAsync(PowerUpDTO dto);

        // Authentication & Leaderboard
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password);
        Task<List<Bomberman.Services.Data.User>> GetLeaderboardAsync();

        // Events - Server to Client Communication
        event Action<PlayerStateDTO>? PlayerMoved;
        event Action<int, int, int>? ExplosionReceived;
        event Action<LobbyStateDTO>? LobbyUpdated;
        event Action<BombDTO> BombPlaced;
        event Action<EnemyMovementDTO> EnemyMoved;
        event Action<PowerUpDTO> PowerUpSpawned;
        event Action<PowerUpDTO> PowerUpCollected;
        event Action<GameStartDTO>? GameStarted;
        event Action<string>? PlayerEliminated;
        event Action<GameNavigationDTO>? GameNavigationRequested;
    }
}
