using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Editing.Constraints;

public readonly record struct ControlPointTarget(
    BezierControlPoint Point,
    Vector2 Position);