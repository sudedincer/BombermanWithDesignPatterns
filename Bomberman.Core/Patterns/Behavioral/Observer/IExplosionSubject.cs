using System.Collections.Generic;

namespace Bomberman.Core.Patterns.Behavioral.Observer
{
    // Subject Arayüzü: Gözlemcileri yönetecek metotları tanımlar
    public interface IExplosionSubject
    {
        void Attach(IExplosionObserver observer); // Gözlemci Ekle
        void Detach(IExplosionObserver observer); // Gözlemci Çıkar
        void Notify(int range);                   // Gözlemcilere Bildirim Yap (Patlama Menzilini Gönder)
    }
}