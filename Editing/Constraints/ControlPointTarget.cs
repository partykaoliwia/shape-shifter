using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Editing.Constraints;

/// <summary>
/// Keeps a dragged handle's absolute target separate from its stored offset.
/// The solver uses it even when the handle's anchor is allowed to move.
/// </summary>
public readonly record struct ControlPointTarget(
    BezierControlPoint Point,
    Vector2 Position);
