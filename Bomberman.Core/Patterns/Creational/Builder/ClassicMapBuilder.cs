using System;
using Bomberman.Core.GameLogic;
using Bomberman.Core.Factories;
using Bomberman.Core.Enums;
using Bomberman.Core.Walls;

namespace Bomberman.Core.Patterns.Creational.Builder
{
    public class ClassicMapBuilder : IMapBuilder
    {
        private GameMap _map;
        private int _width;
        private int _height;
        private IWallFactory _factory;
        private readonly Random _rng = new();

        public void SetDimensions(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void SetTheme(IWallFactory factory)
        {
            _factory = factory;
        }

        public void BuildWalls()
        {
            // Initialize the map object
            _map = new GameMap(_width, _height);

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    // Border Walls
                    if (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                    {
                        var wall = _factory.CreateWall(WallType.Unbreakable, x, y, _map);
                        _map.SetWall(x, y, wall);
                        continue;
                    }

                    // Grid Pattern Walls
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        var wall = _factory.CreateWall(WallType.Unbreakable, x, y, _map);
                        _map.SetWall(x, y, wall);
                        continue;
                    }

                    // Random Hard Walls (10%)
                    if (_rng.Next(100) < 10)
                    {
                        var wall = _factory.CreateWall(WallType.Hard, x, y, _map);
                        _map.SetWall(x, y, wall);
                        continue;
                    }

                    // Random Breakable Walls (40%)
                    if (_rng.Next(100) < 40)
                    {
                        var wall = _factory.CreateWall(WallType.Breakable, x, y, _map);
                        _map.SetWall(x, y, wall);
                        continue;
                    }
                }
            }
        }

        public void ClearSafeZone()
        {
            // Clear standard top-left start area (3x3 grid from 1,1)
            for (int y = 1; y <= 3; y++)
            {
                for (int x = 1; x <= 3; x++)
                {
                    Wall wall = _map.GetWallAt(x, y);
                    // Don't remove Unbreakable (Grid/Border) walls
                    if (!(wall is UnbreakableWall))
                    {
                        // Set to null (remove wall)
                        _map.SetWall(x, y, null);
                    }
                }
            }
        }

        public void SpawnEnemies(int count)
        {
            // For now, adhere to classic logic which spawns specific types rather than just 'count'
            // We can adapt 'count' later if needed or ignore it for strict classic rules
            
            SpawnEnemy(EnemyType.RandomWalker);
            SpawnEnemy(EnemyType.Static);
            SpawnEnemy(EnemyType.Chaser);
        }
        
        private void SpawnEnemy(EnemyType type)
        {
            while (true)
            {
                int x = _rng.Next(1, _width - 1);
                int y = _rng.Next(1, _height - 1);

                // Use the map's collision check (which now checks center) or simple IsWallAt
                // Since spawning happens on Integer coordinates, IsWallAt works fine if using Floor logic we fixed
                if (!_map.IsWallAt(x, y))
                {
                    _map.AddEnemy(EnemyFactory.CreateEnemy(x, y, type));
                    break;
                }
            }
        }

        public GameMap GetMap()
        {
            return _map;
        }
    }
}
