namespace Bomberman.Core.Patterns.Structural.Adapter
{
    /// <summary>
    /// Target Interface for Input.
    /// Decouples the game logic from specific input devices (Keyboard, Gamepad, AI).
    /// </summary>
    public interface IInputService
    {
        (double DeltaX, double DeltaY, bool ShouldPlaceBomb) GetInput();
    }
}
