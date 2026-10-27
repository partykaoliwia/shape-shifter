using System.Numerics;

namespace ShapeShifter.Editing.Interaction
{
    /// <summary>
    /// Input for one editor update, including scene access and selection modifiers.
    /// Keeps Raylib and ImGui input details outside the controller.
    /// </summary>
    public readonly record struct MouseInput(
        Vector2 Position,
        bool LeftPressed,
        bool LeftDown,
        bool CanInteractWithScene,
        bool ExtendSelection,
        bool PreferControlPoints = false);
}
