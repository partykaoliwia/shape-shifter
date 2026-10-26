using System.Numerics;
using ShapeShifter.Editing.Constraints;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
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

        // The anchor may have moved during solving.
        target.Point.MoveTo(target.Position);

        return true;
    }

    public bool TryConvertEdgeToBezier(
        Edge edge,
        out int removedConstraintCount)
    {
        EnsureEdgeBelongsToScene(edge);
        removedConstraintCount = 0;

        if (edge.Bezier != null)
        {
            return true;
        }

        var affectedConstraints = scene.Constraints
            .Where(constraint => constraint.AffectedEdges.Contains(edge))
            .ToArray();

        var remainingConstraints = scene.Constraints
            .Except(affectedConstraints)
            .ToArray();

        edge.EnableBezier();

        if (!TryApplyCurrentGeometry(remainingConstraints))
        {
            edge.DisableBezier();
            return false;
        }

        foreach (var constraint in affectedConstraints)
        {
            scene.RemoveConstraint(constraint);
        }

        removedConstraintCount = affectedConstraints.Length;
        return true;
    }

    public bool TryConvertEdgeToLine(Edge edge)
    {
        EnsureEdgeBelongsToScene(edge);

        var previous = edge.Bezier;

        if (previous == null)
        {
            return true;
        }

        edge.DisableBezier();

        if (TryApplyCurrentGeometry(scene.Constraints))
        {
            return true;
        }

        edge.EnableBezier(previous);
        return false;
    }
}