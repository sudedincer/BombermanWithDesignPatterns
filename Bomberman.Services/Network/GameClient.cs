using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Shared;

namespace Bomberman.Services.Network
{
    // Bu sınıf, UI katmanının Network ile iletişim kurmasını sağlar.
    public class GameClient
    {
        private readonly HubConnection _connection;
        
        // 1. Olay Tanımlama: Sunucudan bir güncelleme geldiğinde UI/Model'i bilgilendirmek için Event kullanıyoruz.
        // Bu, Observer desenine benzer bir Publisher/Subscriber mantığı sağlar.
        public event Action<PlayerStateDTO>? PlayerMoved;
        public event Action<BombDTO>? BombPlaced;
        //patlama geldiğinde tetiklenir
        public event Action<int, int, int>? ExplosionReceived;

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
        }

        public async Task StartConnectionAsync()
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("SignalR connection established.");
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
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
    }
}