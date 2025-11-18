using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Bomberman.Services.Network
{
    public class ExplosionService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public ExplosionService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task TriggerExplosion(int x, int y, int power)
        {
            Console.WriteLine($"[SERVER] ExplosionService → Sending explosion ({x}, {y})");

            await _hubContext.Clients.All.SendAsync("ReceiveExplosion", x, y, power);
        }
    }
}