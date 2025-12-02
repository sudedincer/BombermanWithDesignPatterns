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

        public (double DeltaX, double DeltaY) CalculateMovement(
            Enemy enemy, GameMap map, IPlayer targetPlayer)
        {
            // Yeni yön seçme zamanı geldiyse
            if (_stepsRemaining <= 0)
            {
                enemy.Direction = (Direction)_rng.Next(0, 4);
                _stepsRemaining = _rng.Next(15, 45);
            }

            double dx = 0, dy = 0;
            double step = enemy.Speed * 0.05;

            switch (enemy.Direction)
            {
                case Direction.Up: dy = -step; break;
                case Direction.Down: dy = +step; break;
                case Direction.Left: dx = -step; break;
                case Direction.Right: dx = +step; break;
            }

            double nextX = enemy.X + dx;
            double nextY = enemy.Y + dy;

            // Çarparsa yeni yön seç
            if (map.IsWallAt(nextX, nextY))
            {
                enemy.Direction = (Direction)_rng.Next(0, 4);
                _stepsRemaining = _rng.Next(10, 40);
                return (0, 0);
            }

            _stepsRemaining--;
            return (dx, dy);
        }
    }
}