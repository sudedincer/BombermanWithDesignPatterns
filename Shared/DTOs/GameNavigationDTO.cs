namespace Shared.DTOs
{
    public class GameNavigationDTO
    {
        public string Action { get; set; } = ""; // "Restart" or "Lobby"
        public string RequestedBy { get; set; } = ""; // Username who clicked
        public int? Seed { get; set; } // For map synchronization
    }
}
