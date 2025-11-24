using Bomberman.Core.Patterns.Behavioral.Observer;
// Wall.cs, UnbreakableWall.cs, HardWall.cs tek bir dosyada gösterilmiştir:

namespace Bomberman.Core.Walls
{
    // Temel Duvar (Product)
    public abstract class Wall
    {
        public bool IsDestroyed { get; set; } = false;
        public abstract bool CanBeDestroyed();
    }

   
  

    
}