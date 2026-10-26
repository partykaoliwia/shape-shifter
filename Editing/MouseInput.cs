using System.Numerics;

namespace ShapeShifter.Editing
{
    public readonly record struct MouseInput(
        Vector2 Position,
        bool LeftPressed,
        bool LeftDown,
        bool MouseAvailable,
        bool ExtendSelection);
}