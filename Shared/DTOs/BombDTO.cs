namespace Shared
{
    // Bomba yerleştirme bilgisini senkronize eden DTO
    public class BombDTO
    {
        public string PlacedByUsername { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Power { get; set; }
    }
}