namespace Shared
{
    public class GameStartDTO
    {
        public int Seed { get; set; }
        public string Theme { get; set; } = "City";
        public int PlayerIndex { get; set; } // 1 or 2
    }
}
