using System;
using Bomberman.Core.Entities;
using Bomberman.Core.Enums;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.Strategy
{
    public class RandomWalkMovement : IMovementStrategy
    {
        private readonly Random _rng = new Random();
        private int _stepsRemaining = 0;

        public (double DeltaX, double DeltaY) CalculateMovement(Enemy enemy, GameMap map, IPlayer targetPlayer)
        {
            // Eğer adım hakkı bittiyse veya duvara çarpmışsa yön değiştir
            if (_stepsRemaining <= 0)
            {
                enemy.Direction = (Direction)_rng.Next(0, 4);
                _stepsRemaining = _rng.Next(10, 30);
            }

            // Yön vektörü
            double dx = 0;
            double dy = 0;
            double step = enemy.Speed * 0.1; // hızın görünür olması için 0.1 çarpanı

            switch (enemy.Direction)
            {
                case Direction.Up:
                    dy = -step;
                    break;

                case Direction.Down:
                    dy = step;
                    break;

                case Direction.Left:
                    dx = -step;
                    break;

                case Direction.Right:
                    dx = step;
                    break;
            }

            double nextX = enemy.X + dx;
            double nextY = enemy.Y + dy;

            // Eğer ileride duvar varsa anında yeni yön seç
            if (map.IsWallAt(nextX, nextY))
            {
                enemy.Direction = (Direction)_rng.Next(0, 4);
                _stepsRemaining = _rng.Next(8, 25);
                return (0, 0); // bu frame'de durabilir
            }

            _stepsRemaining--;
            return (dx, dy);
        }
    }
}