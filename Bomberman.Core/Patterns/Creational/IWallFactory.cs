using Bomberman.Core.Enums;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Walls;

namespace Bomberman.Core.Patterns.Creational
{
    // Creator Arayüzü (Fabrika)
    public partial interface IWallFactory
    {
        // Duvar tipi ve konumu alarak Wall nesnesi döndürür.
        Wall CreateWall(WallType type, int x, int y,GameMap map);
    }
}