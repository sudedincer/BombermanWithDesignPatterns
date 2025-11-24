using Bomberman.Core.GameLogic;

namespace Bomberman.Core.Entities
{
    public interface IPlayer
    {
        // KONUM
        (double X, double Y) GetPosition();

        // YAŞAM DURUMU
        bool IsAlive { get; set; }

        // HIZ (Decorator ile artırılır)
        double GetSpeed();

        // BOMBA GÜCÜ (Power decorator)
        int GetBombPower();

        // AYNI ANDA KAÇ BOMBA BIRAKABİLİR?
        int GetMaxBombCount();

        // HAREKET
        void Move(double dx, double dy, GameMap map);
    }
}