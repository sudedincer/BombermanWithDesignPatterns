namespace Shared
{
    // Oyuncu konum ve durumunu senkronize eden DTO
    public class PlayerStateDTO
    {
        public required string Username { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsAlive { get; set; }
        // Ek olarak: Hız, bomba sayısı gibi anlık durumlar eklenebilir.
    }
}