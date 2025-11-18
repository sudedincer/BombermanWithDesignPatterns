using Microsoft.Xna.Framework.Input;
using Shared;

namespace Bomberman.UI.Controller
{
    public static class InputController
    {
        // Controller, klavye durumunu kontrol eder ve hareket yönünü belirler.
        public static (double DeltaX, double DeltaY, bool ShouldPlaceBomb) GetCurrentInput()
        {
            var keyboardState = Keyboard.GetState();
            double deltaX = 0;
            double deltaY = 0;
            bool shouldPlaceBomb = false;

            // Hareket Kontrolleri
            if (keyboardState.IsKeyDown(Keys.W)) deltaY -= 1; // Yukarı
            if (keyboardState.IsKeyDown(Keys.S)) deltaY += 1; // Aşağı
            if (keyboardState.IsKeyDown(Keys.A)) deltaX -= 1; // Sol
            if (keyboardState.IsKeyDown(Keys.D)) deltaX += 1; // Sağ

            // Bomba Yerleştirme Kontrolü
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                shouldPlaceBomb = true;
            }

            // Delta değerlerini normalize edebilirsiniz (Çapraz hareket için)
            // (Bu kodu şimdilik basit tutalım)

            return (deltaX, deltaY, shouldPlaceBomb);
        }
    }
}