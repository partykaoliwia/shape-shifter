using System.Numerics;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing.Constraints;

public partial class ConstraintSolver
{
    private static bool IsArcGeometrySatisfied(
        ArcGeometryConstraint constraint,
        IReadOnlyDictionary<IGeometryVariable, Vector2> positions)
    {
        var arc = constraint.Edge.Arc;

        return arc != null
            && arc.TryGetGeometry(
                positions[arc.Start],
                positions[arc.End],
                positions[arc],
                out _);
    }

    private static bool ProjectArcGeometry(
        ArcGeometryConstraint constraint,
        Dictionary<IGeometryVariable, Vector2> positions,
        IReadOnlySet<IGeometryVariable> fixedVariables)
    {
        var arc = constraint.Edge.Arc;

        if (arc == null
            || !CircularArcSegment.SupportsContinuity(arc.Start, arc.End))
        {
            return false;
        }

        if (arc.Start.Continuity == ContinuityType.G0
            && arc.End.Continuity == ContinuityType.G0)
        {
            return false;
        }

        Vector2 chord = positions[arc.End] - positions[arc.Start];
        Vector2 tangent = positions[arc];

        if (tangent.LengthSquared() <= MinLengthSquared)
        {
            if (fixedVariables.Contains(arc))
            {
                return false;
            }

            Vector2 initial = new(chord.Y, -chord.X);

            if (initial.LengthSquared() <= MinLengthSquared)
            {
                initial = Vector2.UnitY;
            }

            if (!arc.Clockwise)
            {
                initial = -initial;
            }

            if (arc.End.Continuity == ContinuityType.G1)
            {
                initial = -initial;
            }

            positions[arc] = initial;
            return true;
        }

        Vector2 direction = Vector2.Normalize(tangent);
        Vector2 normal = new(-direction.Y, direction.X);

        float startWeight =
            fixedVariables.Contains(arc.Start) ? 0f : 1f;

        float endWeight =
            fixedVariables.Contains(arc.End) ? 0f : 1f;

        float totalWeight = startWeight + endWeight;

        if (totalWeight > 0f)
        {
            float separation = Vector2.Dot(chord, normal);
            float required = MathF.Max(1f, chord.Length() * 0.01f);

            float target = separation < 0f
                ? -required
                : required;

            Vector2 correction =
                normal * ((target - separation) / totalWeight);

            positions[arc.Start] -= correction * startWeight;
            positions[arc.End] += correction * endWeight;

            return true;
        }

        if (chord.LengthSquared() <= MinLengthSquared
            || fixedVariables.Contains(arc))
        {
            return false;
        }

        const float angle = 0.02f;
        float cosine = MathF.Cos(angle);
        float sine = MathF.Sin(angle);

        positions[arc] = new Vector2(
            cosine * tangent.X - sine * tangent.Y,
            sine * tangent.X + cosine * tangent.Y);

        return true;
    }
}