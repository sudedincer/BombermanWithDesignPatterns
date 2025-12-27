using Bomberman.Core.Patterns.Behavioral.Observer;
using Bomberman.Core.Enums;

namespace Bomberman.Core.Walls
{
    // Temel Duvar (Product)
    public abstract class Wall
    {
        public bool IsDestroyed { get; set; } = false;
        public abstract bool CanBeDestroyed();
        
        /// <summary>
        /// Returns visual theme identifier for this wall.
        /// UI layer uses this to select appropriate texture.
        /// Core layer stays UI-independent by using enum instead of paths.
        /// </summary>
        public virtual WallVisualTheme GetVisualTheme() 
            => WallVisualTheme.Generic;
    }

   
  

    
}