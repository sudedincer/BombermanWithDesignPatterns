using System;
using System.Collections.Generic;
using Bomberman.Core.Config;
using Bomberman.Core.Enums;
using Bomberman.Core.PowerUps;
using Bomberman.Core.Walls;
using Bomberman.Core.Entities;
using Bomberman.Core.Factories;
using Bomberman.Core.Patterns.Creational;

namespace Bomberman.Core.GameLogic
{
    public class GameMap
    {
        public int Width { get; }
        public int Height { get; }
        public Wall[,] Walls { get; }
        public List<Enemy> Enemies { get; } = new();
        public List<PowerUp> PowerUps { get; } = new();

        private readonly Random _rng = new();
        public event Action<int, int> ExplosionCell;

        public GameMap(int width, int height, IWallFactory factory)
        {
            Width = width;
            Height = height;
            Walls = new Wall[height, width];

            GenerateWalls(factory);
            ClearStartArea();
            SpawnEnemiesRandomAndSafe();
        }

        // ============================================================
        // 1) DUVAR OLUŞTURMA
        // ============================================================

        private void GenerateWalls(IWallFactory factory)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Dış kenarlar
                    if (x == 0 || x == Width - 1 ||
                        y == 0 || y == Height - 1)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Unbreakable, x, y, this);
                        continue;
                    }

                    // İç kolonlar
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Unbreakable, x, y, this);
                        continue;
                    }

                    // %10 hard
                    if (_rng.Next(0, 100) < 10)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Hard, x, y, this);
                        continue;
                    }

                    // %40 breakable
                    if (_rng.Next(0, 100) < 40)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Breakable, x, y, this);
                        continue;
                    }
                }
            }
        }

        // ============================================================
        // 2) BAŞLANGIÇ ALANI TEMİZLE
        // ============================================================

        private void ClearStartArea()
        {
            for (int y = 1; y <= 3; y++)
            {
                for (int x = 1; x <= 3; x++)
                {
                    if (!(Walls[y, x] is UnbreakableWall))
                        Walls[y, x] = null;
                }
            }
        }

        // ============================================================
        // 3) SAFE RANDOM ENEMY SPAWN
        // ============================================================

        private void SpawnEnemiesRandomAndSafe()
        {
            SpawnEnemyAtRandomEmptyTile(EnemyType.RandomWalker);
            SpawnEnemyAtRandomEmptyTile(EnemyType.Static);
            SpawnEnemyAtRandomEmptyTile(EnemyType.Chaser);
        }

        private void SpawnEnemyAtRandomEmptyTile(EnemyType type)
        {
            while (true)
            {
                int x = _rng.Next(1, Width - 1);
                int y = _rng.Next(1, Height - 1);

                if (!IsWallAt(x, y))
                {
                    Enemies.Add(EnemyFactory.CreateEnemy(x, y, type));
                    break;
                }
            }
        }

        // ============================================================
        // 4) PATLAMA YAYILIMI — TEK MERKEZ
        // ============================================================

        public void HandleExplosion(int x, int y, int power)
        {
            ApplyExplosionToCell(x, y);

            // 4 yön
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int dir = 0; dir < 4; dir++)
            {
                int cx = x;
                int cy = y;

                for (int step = 1; step <= power; step++)
                {
                    cx += dx[dir];
                    cy += dy[dir];

                    if (IsOutsideBounds(cx, cy))
                        break;

                    if (!ApplyExplosionToCell(cx, cy))
                        break;
                }
            }
        }

        /// <summary>
        /// Patlamanın uygulandığı tek hücre.
        /// True → patlama devam eder.
        /// False → patlama bu yönde durur.
        /// </summary>
        private bool ApplyExplosionToCell(int x, int y)
        {
            ExplosionCell?.Invoke(x, y);
            // Duvar kırma
            var wall = GetWallAt(x, y);

            if (wall is UnbreakableWall)
                return false;

            if (wall is BreakableWall || wall is HardWall)
            {
                RemoveWall(x, y);
                return false;
            }

            return true;
        }

        // ============================================================
        // 5) DUVAR KIRMA
        // ============================================================

        public Wall GetWallAt(int x, int y)
        {
            if (IsOutsideBounds(x, y)) return null;
            return Walls[y, x];
        }

        public void RemoveWall(int x, int y)
        {
            if (IsOutsideBounds(x, y))
                return;

            var wall = Walls[y, x];
            if (wall == null)
                return;

            if (!wall.CanBeDestroyed())
                return;

            // Power-up?
            if (_rng.NextDouble() < GameConfig.Instance.PowerUpDropRate)
            {
                PowerUps.Add(PowerUpFactory.CreateRandomPowerUp(x, y));
            }

            Walls[y, x] = null;
        }

        // ============================================================
        // 6) DUVAR/GRID KONTROLLERİ
        // ============================================================

        public bool IsOutsideBounds(int x, int y)
        {
            return x < 0 || y < 0 || x >= Width || y >= Height;
        }

        public bool IsWallAt(double x, double y)
        {
            int gx = (int)Math.Round(x);
            int gy = (int)Math.Round(y);

            if (IsOutsideBounds(gx, gy))
                return true;

            var wall = Walls[gy, gx];
            return wall != null && !wall.IsDestroyed;
        }
    }
}