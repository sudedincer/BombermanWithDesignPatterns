using Bomberman.Core.Patterns.Structural.Adapter;
using Bomberman.UI.Controller;

namespace Bomberman.UI.Patterns.Structural.Adapter
{
    /// <summary>
    /// Adapter class that adapts the static InputController to the IInputService interface.
    /// This allows GameScene to depend on abstraction rather than concrete implementation.
    /// </summary>
    public class KeyboardInputAdapter : IInputService
    {
        // Internal method to access the Adaptee (InputController)
        public (double DeltaX, double DeltaY, bool ShouldPlaceBomb) GetInput()
        {
            // Call the Adaptee
            return InputController.GetCurrentInput();
        }
    }
}
