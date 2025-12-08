using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Shared;
using Shared.DTOs;

namespace Bomberman.Services.Network
{
    // Bu sınıf, UI katmanının Network ile iletişim kurmasını sağlar.
    public class GameClient
    {
        private readonly HubConnection _connection;
        
        // 1. Olay Tanımlama: Sunucudan bir güncelleme geldiğinde UI/Model'i bilgilendirmek için Event kullanıyoruz.
        // Bu, Observer desenine benzer bir Publisher/Subscriber mantığı sağlar.
        public event Action<PlayerStateDTO>? PlayerMoved;
        //patlama geldiğinde tetiklenir
        public event Action<int, int, int>? ExplosionReceived;
        
        // YENİ: Lobby/GameStart Events
        public event Action<LobbyStateDTO>? LobbyUpdated;
        public event Action<Shared.BombDTO> BombPlaced;
        public event Action<Shared.DTOs.EnemyMovementDTO> EnemyMoved;
        public event Action<Shared.DTOs.PowerUpDTO> PowerUpSpawned;
        public event Action<Shared.DTOs.PowerUpDTO> PowerUpCollected;
        
        private string _username;
        public event Action<GameStartDTO>? GameStarted;
        public event Action<string>? PlayerEliminated;

        public GameClient(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl) 
                .Build();

            // Sunucudan (GameHub) gelen mesajları işleyen metotları tanımla.
            
            // HAREKET SENKRONİZASYONU: Sunucu, PlayerStateDTO'yu gönderdiğinde
            _connection.On<PlayerStateDTO>("ReceiveMovement", (state) =>
            {
                // UI'ı/Model'i güncellemesi için abone olanlara bildir
                PlayerMoved?.Invoke(state);
                Console.WriteLine($"Received move for {state.Username} at ({state.X:F2}, {state.Y:F2})");
            });
            
            // BOMBA YERLEŞTİRME: Sunucu, BombDTO'yu gönderdiğinde
            _connection.On<BombDTO>("ReceiveBombPlacement", (bomb) =>
            {
                // UI'ı/Model'i yeni bomba hakkında bilgilendir
                BombPlaced?.Invoke(bomb);
                Console.WriteLine($"Received bomb placement from {bomb.PlacedByUsername} at ({bomb.X}, {bomb.Y})");
            });
            // Sunucudan gelen patlama mesajını yakala
            _connection.On<int, int, int>("ReceiveExplosion", (x, y, power) =>
            {
                // Olayı tetikle
                ExplosionReceived?.Invoke(x, y, power);
                Console.WriteLine($"Explosion at ({x}, {y}) with power {power} received.");
            });
            
            // LOBBY UPDATE
            _connection.On<LobbyStateDTO>("LobbyUpdated", (lobby) =>
            {
                LobbyUpdated?.Invoke(lobby);
            });

            // GAME START
            _connection.On<GameStartDTO>("GameStarted", (dto) =>
            {
                GameStarted?.Invoke(dto);
            });

            // ENEMY MOVED
            _connection.On<Shared.DTOs.EnemyMovementDTO>("EnemyMoved", (dto) =>
            {
                EnemyMoved?.Invoke(dto);
            });

            _connection.On<Shared.DTOs.PowerUpDTO>("ReceivePowerUpSpawn", (dto) =>
            {
                PowerUpSpawned?.Invoke(dto);
            });

            _connection.On<Shared.DTOs.PowerUpDTO>("ReceivePowerUpCollection", (dto) =>
            {
                PowerUpCollected?.Invoke(dto);
            });


            // PLAYER ELIMINATED
            _connection.On<string>("PlayerEliminated", (username) =>
            {
                PlayerEliminated?.Invoke(username);
            });
        }

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
        
        public async Task JoinLobbyAsync(string username)
        {
            await _connection.InvokeAsync("JoinLobby", username);
        }

        public async Task MoveEnemyAsync(Shared.DTOs.EnemyMovementDTO dto)
        {
            await _connection.InvokeAsync("MoveEnemy", dto);
        }

        public async Task SpawnPowerUpAsync(Shared.DTOs.PowerUpDTO dto)
        {
            await _connection.InvokeAsync("SpawnPowerUp", dto);
        }

        public async Task CollectPowerUpAsync(Shared.DTOs.PowerUpDTO dto)
        {
            await _connection.InvokeAsync("CollectPowerUp", dto);
        }
        
        // --- CLIENT'TAN SUNUCUYA ÇAĞRILAR (OUTGOING) ---
        
        // UI/Controller tarafından çağrılır (Hareket)
        public async Task SendMovementAsync(PlayerStateDTO state)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                // Hub'daki SendMovement metodunu çağır
                await _connection.InvokeAsync("SendMovement", state);
            }
        }
        
        // UI/Controller tarafından çağrılır (Bomba Koyma)
        public async Task PlaceBombAsync(BombDTO bomb)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                // Hub'daki PlaceBomb metodunu çağır
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
    }
}