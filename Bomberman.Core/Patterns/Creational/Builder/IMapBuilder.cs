using Bomberman.Core.GameLogic;
using Bomberman.Core.Factories;

namespace Bomberman.Core.Patterns.Creational.Builder
{
    public interface IMapBuilder
    {
        void SetDimensions(int width, int height);
        void SetTheme(IWallFactory factory);
        
        // Steps to build the map
        void BuildWalls();
        void ClearSafeZone();
        void SpawnEnemies(int count);
        
        GameMap GetMap();
    }
}
