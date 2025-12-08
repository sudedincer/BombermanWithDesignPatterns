using System;
using Bomberman.Core.Config;
using Bomberman.Core.Entities;
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
        // private readonly Random _rng; // Use GameConfig.Rng

        public ClassicMapBuilder(int seed)
        {
            Bomberman.Core.Config.GameConfig.Instance.SetSeed(seed);
        }

        public ClassicMapBuilder()
        {
            // Default seed (Time-based / Random)
            Bomberman.Core.Config.GameConfig.Instance.SetSeed(new Random().Next());
        }

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
                    if (Bomberman.Core.Config.GameConfig.Instance.Rng.Next(100) < 10)
                    {
                        var wall = _factory.CreateWall(WallType.Hard, x, y, _map);
                        _map.SetWall(x, y, wall);
                        continue;
                    }

                    // Random Breakable Walls (40%)
                    if (Bomberman.Core.Config.GameConfig.Instance.Rng.Next(100) < 40)
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
            // Clear Top-Left (1,1) -> (2,1), (1,2)
            _map.SetWall(1, 1, null);
            _map.SetWall(1, 2, null);
            _map.SetWall(2, 1, null);

            // Clear Bottom-Right (W-2, H-2) -> (W-3, H-2), (W-2, H-3)
            int w = _width - 2;
            int h = _height - 2;
            _map.SetWall(w, h, null);
            _map.SetWall(w, h - 1, null);
            _map.SetWall(w - 1, h, null);
        }

        public void SpawnEnemies(int count)
        {
            // SPECIFIC CHALLENGE: Dual Chasing Enemies
            // 1. Chaser for Player 1
            var chaser1 = SpawnEnemy(EnemyType.Chaser);
            if (chaser1 != null) chaser1.TargetPlayerIndex = 1;

            // 2. Chaser for Player 2
            var chaser2 = SpawnEnemy(EnemyType.Chaser);
            if (chaser2 != null) chaser2.TargetPlayerIndex = 2;

            // 3. Filler Enemies
            SpawnEnemy(EnemyType.RandomWalker);
            SpawnEnemy(EnemyType.Static);
        }
        
        private Enemy? SpawnEnemy(EnemyType type)
        {
            while (true)
            {
                int x = Bomberman.Core.Config.GameConfig.Instance.Rng.Next(1, _width - 1);
                int y = Bomberman.Core.Config.GameConfig.Instance.Rng.Next(1, _height - 1);

                // Use the map's collision check (which now checks center) or simple IsWallAt
                // Since spawning happens on Integer coordinates, IsWallAt works fine if using Floor logic we fixed
                if (!_map.IsWallAt(x, y))
                {
                    var enemy = EnemyFactory.CreateEnemy(x, y, type);
                    _map.AddEnemy(enemy);
                    return enemy;
                }
            }
        }

        public GameMap GetMap()
        {
            return _map;
        }
    }
}
