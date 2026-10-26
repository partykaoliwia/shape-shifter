using System.Numerics;

namespace ShapeShifter.Editing.Interaction
{
    public readonly record struct MouseInput(
        Vector2 Position,
        bool LeftPressed,
        bool LeftDown,
        bool CanInteractWithScene,
        bool ExtendSelection);
}