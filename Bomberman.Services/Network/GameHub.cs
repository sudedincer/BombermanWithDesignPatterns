using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Shared;

namespace Bomberman.Services.Network
{
    public class GameHub : Hub
    {
        private readonly ExplosionService _explosionService;

        public GameHub(ExplosionService explosionService)
        {
            _explosionService = explosionService;
        }

        public async Task SendMovement(PlayerStateDTO state)
        {
            Console.WriteLine($"[SERVER LOG] Movement from {state.Username} ({state.X}, {state.Y})");

            await Clients.Others.SendAsync("ReceiveMovement", state);
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
    }
}