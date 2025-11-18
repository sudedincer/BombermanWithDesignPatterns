namespace Bomberman.Core
{
    // Temel oyuncu özelliklerini tanımlayan arayüz (Component)
    public interface IPlayer
    {
        // Zorunlu Power-up gereksinimlerine karşılık gelen metotlar
        int GetMaxBombs();      // Artan Bomba Sayısı [cite: 39]
        int GetBombPower();     // Artan Bomba Gücü [cite: 40]
        double GetSpeed();      // Hız Takviyesi [cite: 41]
        
        // Temel hareket/pozisyon metotları
        void Move(double x, double y);
        (double X, double Y) GetPosition();
    }
}