using System.Numerics;
using ShapeShifter.Editing.Constraints;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
    /// <summary>
    /// Moves a Bezier handle to an absolute position while preserving relations.
    /// Tries fixed curve endpoints first, then movable endpoints, then a connected-group translation.
    /// </summary>
    public void MoveControlPointTo(
        BezierControlPoint controlPoint,
        Vector2 position)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            return;
        }

        var edge = scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .FirstOrDefault(edge =>
                edge.Bezier is { } segment
                && (segment.Control1 == controlPoint
                    || segment.Control2 == controlPoint));

        if (edge == null)
        {
            return;
        }

        var constraints = GetSolverConstraints(scene.Constraints);
        var target = new ControlPointTarget(controlPoint, position);

        if (TryMoveControlPoint(edge, target, constraints, true)
            || TryMoveControlPoint(edge, target, constraints, false))
        {
            return;
        }

        Vector2 offset = position - controlPoint.Position;

        foreach (var vertex in GetConnectedVertices(
            new[] { controlPoint.Anchor }, constraints))
        {
            vertex.MoveTo(vertex.Position + offset);
        }

        controlPoint.MoveTo(position);
    }

    /// <summary>
    /// Solves one handle move on a fresh snapshot with optional endpoint locks.
    /// Applies the snapshot only on success and keeps the handle at its absolute target.
    /// </summary>
    private bool TryMoveControlPoint(
        Edge edge,
        ControlPointTarget target,
        IReadOnlyList<IGeometricConstraint> constraints,
        bool keepCurveEnds)
    {
        var positions = CapturePositions();

        positions[target.Point] =
            target.Position - positions[target.Point.Anchor];

        var fixedVariables = new HashSet<IGeometryVariable>
        {
            target.Point
        };

        if (keepCurveEnds)
        {
            fixedVariables.Add(edge.Start);
            fixedVariables.Add(edge.End);
        }

        if (!solver.TrySolve(
            constraints,
            positions,
            fixedVariables,
            target))
        {
            return false;
        }

        ApplyPositions(positions);

        // Recompute the offset from the final anchor so the handle stays at the cursor.
        target.Point.MoveTo(target.Position);

        return true;
    }
}
