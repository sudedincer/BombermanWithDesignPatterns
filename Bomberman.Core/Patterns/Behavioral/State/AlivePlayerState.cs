using System;
using Bomberman.Core.Entities;
using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Patterns.Behavioral.State
{
    public class AlivePlayerState : IPlayerState
    {
        public void HandleMove(BasePlayer context, double dx, double dy, GameMap map)
        {
            // Move Logic copied from BasePlayer
            double sensitivity = 0.15;
            double cx = context.X;
            double cy = context.Y;

            // 1. Try Moving X
            if (!map.CheckCollision(cx + dx + 0.5, cy + 0.5))
            {
                context.SetPosition(cx + dx, cy);
            }
            else if (dx != 0) // Blocked on X? Try to Slide Y
            {
                double slideSpeed = Math.Abs(dx);
                
                // Check Up
                if (!map.CheckCollision(cx + dx + 0.5, cy + 0.5 - sensitivity))
                    context.SetPosition(cx, cy - slideSpeed);
                // Check Down
                else if (!map.CheckCollision(cx + dx + 0.5, cy + 0.5 + sensitivity))
                    context.SetPosition(cx, cy + slideSpeed);
            }

            // Re-read position in case it changed
            // Actually, we should probably use local variables and set once, but BasePlayer logic sets X/Y independently
            cx = context.X;
            cy = context.Y;

            // 2. Try Moving Y
            if (!map.CheckCollision(cx + 0.5, cy + dy + 0.5))
            {
                context.SetPosition(cx, cy + dy);
            }
            else if (dy != 0) // Blocked on Y? Try to Slide X
            {
                double slideSpeed = Math.Abs(dy);

                // Check Left
                if (!map.CheckCollision(cx + 0.5 - sensitivity, cy + dy + 0.5))
                    context.SetPosition(cx - slideSpeed, cy);
                // Check Right
                else if (!map.CheckCollision(cx + 0.5 + sensitivity, cy + dy + 0.5))
                    context.SetPosition(cx + slideSpeed, cy);
            }
        }

        public void HandleExplosion(BasePlayer context, int x, int y, int power)
        {
            // Check if player is hit
            if ((int)Math.Floor(context.X + 0.5) == x &&
                (int)Math.Floor(context.Y + 0.5) == y)
            {
                Console.WriteLine("Player died from explosion!");
                // Transition to Dead State
                context.TransitionTo(new DeadPlayerState());
            }
        }
    }
}
