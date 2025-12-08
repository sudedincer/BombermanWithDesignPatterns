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

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
            Walls = new Wall[height, width];
        }

        // Methods for Builder to use
        public void SetWall(int x, int y, Wall wall)
        {
            if (!IsOutsideBounds(x, y))
            {
                Walls[y, x] = wall;
            }
        }

        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }

        // ============================================================
        // 4) TEK HÜCREYE PATLAMA ETKİSİ
        // ============================================================

        public bool ApplyExplosionToCell(int x, int y)
        {
            // GameView görsel çizebilsin
            ExplosionCell?.Invoke(x, y);

           
            var wall = GetWallAt(x, y);

            // KIRILMAZ DUVAR → geçirmez
            if (wall is UnbreakableWall)
                return false;

            // KIRILABİLİR DUVAR → tek vuruşta kırılır
            if (wall is BreakableWall)
            {
                RemoveWall(x, y);
                return false;
            }

            // HARD WALL → Canı azalır, bitince kırılır ama HER ZAMAN patlamayı durdurur
            if (wall is HardWall hw)
            {
                hw.HitsRemaining--;

                if (hw.HitsRemaining <= 0)
                {
                    hw.IsDestroyed = true;
                    RemoveWall(x, y);
                }

                return false; // HardWall patlama geçirmez
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
            int gx = (int)Math.Floor(x);
            int gy = (int)Math.Floor(y);

            if (IsOutsideBounds(gx, gy))
                return true;

            var wall = Walls[gy, gx];
            return wall != null && !wall.IsDestroyed;
        }

        public bool CheckCollision(double x, double y, double size = 0.55)
        {
            // Check 4 corners of the bounding box
            double half = size / 2.0;
            
            // Top-Left
            if (IsWallAt(x - half, y - half)) return true;
            // Top-Right
            if (IsWallAt(x + half, y - half)) return true;
            // Bottom-Left
            if (IsWallAt(x - half, y + half)) return true;
            // Bottom-Right
            if (IsWallAt(x + half, y + half)) return true;
            
            return false;
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