namespace Bomberman.Core.Walls
{
    public class UnbreakableWall : Wall
    {
        public override bool CanBeDestroyed() => false;

        // Patlamaya tepki vermesine gerek yok.
        // Game1.ApplyExplosionToCell içinde zaten UnbreakableWall için
        // RemoveWall çağrılmıyor, sadece patlama burada duruyor.
    }
}