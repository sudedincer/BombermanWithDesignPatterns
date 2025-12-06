using System;
using System.Collections.Generic;
using Bomberman.Core.Config;
using Bomberman.Core.Enums;
using Bomberman.Core.PowerUps;
using Bomberman.Core.Walls;
using Bomberman.Core.Entities;
using Bomberman.Core.Factories;
using Bomberman.Core.Patterns.Behavioral.Observer;
using Bomberman.Core.Patterns.Creational;

namespace Bomberman.Core.GameLogic
{
    public class GameMap :IExplosionObserver
    {
        public int Width { get; }
        public int Height { get; }
        public Wall[,] Walls { get; }
        public List<Enemy> Enemies { get; } = new();
        public List<PowerUp> PowerUps { get; } = new();

        private readonly Random _rng = new();

        // GameView'in patlamayı çizebilmesi için event
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
                    if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Unbreakable, x, y, this);
                        continue;
                    }

                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Unbreakable, x, y, this);
                        continue;
                    }

                    if (_rng.Next(100) < 10)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Hard, x, y, this);
                        continue;
                    }

                    if (_rng.Next(100) < 40)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Breakable, x, y, this);
                        continue;
                    }
                }
            }
        }

        // ============================================================
        // 2) BAŞLANGIÇ ALANINI TEMİZLE
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
        // 3) ENEMY SPAWN
        // ============================================================

        private void SpawnEnemiesRandomAndSafe()
        {
            SpawnEnemy(EnemyType.RandomWalker);
            SpawnEnemy(EnemyType.Static);
            SpawnEnemy(EnemyType.Chaser);
        }

        private void SpawnEnemy(EnemyType type)
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
        // 4) TEK HÜCREYE PATLAMA ETKİSİ
        // ============================================================

        public bool ApplyExplosionToCell(int x, int y)
        {
            // GameView görsel çizebilsin
            ExplosionCell?.Invoke(x, y);

            var wall = GetWallAt(x, y);

            if (wall is UnbreakableWall)
                return false;

            if (wall is BreakableWall || wall is HardWall)
            {
                RemoveWall(x, y);
                return false;
            }

            return true; // boş → patlama devam edebilir
        }

        // ============================================================
        // 5) DUVAR KIRMA
        // ============================================================

        public void RemoveWall(int x, int y)
        {
            if (IsOutsideBounds(x, y))
                return;

            var wall = Walls[y, x];
            if (wall == null)
                return;

            if (!wall.CanBeDestroyed())
                return;

            if (_rng.NextDouble() < GameConfig.Instance.PowerUpDropRate)
                PowerUps.Add(PowerUpFactory.CreateRandomPowerUp(x, y));

            Walls[y, x] = null;
        }

        public Wall GetWallAt(int x, int y)
        {
            if (IsOutsideBounds(x, y))
                return null;

            return Walls[y, x];
        }

        // ============================================================
        // 6) KONTROLLER
        // ============================================================

        public bool IsOutsideBounds(int x, int y)
            => x < 0 || y < 0 || x >= Width || y >= Height;

        public bool IsWallAt(double x, double y)
        {
            int gx = (int)Math.Round(x);
            int gy = (int)Math.Round(y);

            if (IsOutsideBounds(gx, gy))
                return true;

            var wall = Walls[gy, gx];
            return wall != null && !wall.IsDestroyed;
        }
        public void OnExplosion(int x, int y, int power)
        {
            // Merkez patlama
            ApplyExplosionToCell(x, y);

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

                    // Harita dışında → durdur
                    if (IsOutsideBounds(cx, cy))
                        break;

                    // Patlama bu hücrede devam ederse ilerle
                    if (!ApplyExplosionToCell(cx, cy))
                        break;
                }
            }
        }
    }
}