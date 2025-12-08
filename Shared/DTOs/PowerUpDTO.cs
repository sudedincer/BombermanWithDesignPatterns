using System;

namespace Shared.DTOs
{
    public class PowerUpDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } // "Speed", "Bomb", "ExtraBomb"
    }
}
