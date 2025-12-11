namespace Shared
{
    public class LobbyStateDTO
    {
        public int PlayerCount { get; set; }
        public bool IsReady { get; set; }
        public string Message { get; set; } = "";
        public bool IsFirstPlayer { get; set; }
        public string? SelectedTheme { get; set; }
    }
}
