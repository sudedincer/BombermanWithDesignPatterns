namespace Bomberman.Core
{
    public class BasePlayer : IPlayer
    {
        private int _maxBombs = 1; 
        private int _bombPower = 1;
        private double _speed = 3.0;
        private double _x = 0, _y = 0; 

        public int GetMaxBombs() => _maxBombs;
        public int GetBombPower() => _bombPower;
        public double GetSpeed() => _speed;

        public void Move(double x, double y)
        {
            _x += x * GetSpeed(); // Hız özelliğini kullan
            _y += y * GetSpeed();
        }
        
        public (double X, double Y) GetPosition() => (_x, _y);
    }
}