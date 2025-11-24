using System;
using System.Collections.Generic;
using Bomberman.Core.Config;
using Bomberman.Core.Enums;
using Bomberman.Core.Patterns.Creational;
using Bomberman.Core.PowerUps;
using Bomberman.Core.Walls;
using Bomberman.Core.PowerUps;
using System.Collections.Generic;

namespace Bomberman.Core.GameLogic
{
    public class GameMap
    {
        public int Width { get; }
        public int Height { get; }
        public Wall[,] Walls { get; }

        // POWER-UP LİSTESİ
        public List<PowerUp> PowerUps { get; } = new();

        private readonly PowerUpFactory _powerUpFactory = new PowerUpFactory();

        public GameMap(int width, int height, IWallFactory factory)
        {
            Width = width;
            Height = height;
            Walls = new Wall[height, width];

            var rng = new Random();

            // HARİTA DOLDURMA MANTIĞI:
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 1. Kırılamaz Dış Kenarlar
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Unbreakable, x, y, this);
                        continue;
                    }

                    // 2. Kırılamaz İç Kolonlar
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Unbreakable, x, y, this);
                        continue;
                    }

                    // 3. %10 Hard Wall
                    if (rng.Next(0, 100) < 10)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Hard, x, y, this);
                        continue;
                    }

                    // 4. %40 Breakable Wall
                    if (rng.Next(0, 100) < 40)
                    {
                        Walls[y, x] = factory.CreateWall(WallType.Breakable, x, y, this);
                        continue;
                    }

                    // 5. Diğer kareler boş
                }
            }

            // BAŞLANGIÇ ALANI TEMİZLEME (3x3 bölge)
            for (int y = 1; y <= 3; y++)
            {
                for (int x = 1; x <= 3; x++)
                {
                    if (y < Height && x < Width)
                    {
                        // Kırılamaz duvarlara dokunmuyoruz, varsa kırılabilir veya hard wall'ı temizliyoruz.
                        if (!(Walls[y, x] is UnbreakableWall))
                        {
                            Walls[y, x] = null;
                        }
                    }
                }
            }
        }

        // Player.Move / Enemy hareket için çarpışma kontrolü
        public bool IsWallAt(double x, double y)
        {
            const double Buffer = 0.15;

            (double tx, double ty)[] testPoints = new[]
            {
                (x + Buffer, y + Buffer),
                (x + Buffer, y - Buffer),
                (x - Buffer, y + Buffer),
                (x - Buffer, y - Buffer)
            };

            foreach (var (tx, ty) in testPoints)
            {
                int gridX = (int)Math.Round(tx);
                int gridY = (int)Math.Round(ty);

                if (IsOutsideBounds(gridX, gridY))
                    return true;

                Wall wall = Walls[gridY, gridX];

                if (wall != null && !wall.IsDestroyed)
                    return true;
            }

            return false;
        }

        public bool IsOutsideBounds(int x, int y)
        {
            return x < 0 || y < 0 || x >= Width || y >= Height;
        }

        public Wall GetWallAt(int x, int y)
        {
            if (IsOutsideBounds(x, y))
                return null; // DIŞARIYA ÇIKAN PATLAMA, DUVAR YOK SAYILSIN

            return Walls[y, x];
        }

        public void RemoveWall(int x, int y)
        {
            if (IsOutsideBounds(x, y))
                return;

            Wall wall = Walls[y, x];

            if (wall.CanBeDestroyed())
            {
                wall.IsDestroyed = true;

                if (GameConfig.Instance.Rng.NextDouble() < GameConfig.Instance.PowerUpDropRate)
                {
                    var powerUp = PowerUpFactory.CreateRandomPowerUp(x, y);
                    PowerUps.Add(powerUp);
                }
            }

            // Duvarı gerçekten sil
            Walls[y, x] = null;
        }
        
    
    }
}