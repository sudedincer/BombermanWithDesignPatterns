namespace Bomberman.Core.Patterns.Behavioral.Observer
{
    // Observer Arayüzü: Bildirim alacak nesneler tarafından uygulanır
    public interface IExplosionObserver
    {
        // Subject'ten bildirim geldiğinde çağrılır
        // Patlama merkezini (X, Y) ve gücünü/menzilini alır.
        void OnExplosion(int explosionX, int explosionY, int range); 
    }
}