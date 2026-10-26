using ShapeShifter.Models;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
    public bool TrySetEdgeGeometry(
        Edge edge,
        EdgeGeometryType geometryType,
        out int removedConstraintCount)
    {
        EnsureEdgeBelongsToScene(edge);
        removedConstraintCount = 0;

        if (edge.GeometryType == geometryType)
        {
            return true;
        }

        if (geometryType == EdgeGeometryType.CircularArc
            && (edge.Start.Continuity != ContinuityType.G0
                || edge.End.Continuity != ContinuityType.G0))
        {
            return false;
        }

        var previousBezier = edge.Bezier;
        var previousArc = edge.Arc;

        var affectedConstraints = scene.Constraints
            .Where(constraint => constraint.AffectedEdges.Contains(edge))
            .ToArray();

        var remainingConstraints = scene.Constraints
            .Except(affectedConstraints)
            .ToArray();

        switch (geometryType)
        {
            case EdgeGeometryType.Line:
                edge.EnableLine();
                break;

            case EdgeGeometryType.Bezier:
                edge.EnableBezier();
                break;

            case EdgeGeometryType.CircularArc:
                edge.EnableArc();
                break;

            default:
                return false;
        }

        if (!TryApplyCurrentGeometry(remainingConstraints))
        {
            edge.EnableLine();

            if (previousBezier != null)
            {
                edge.EnableBezier(previousBezier);
            }
            else if (previousArc != null)
            {
                edge.EnableArc(previousArc);
            }

            return false;
        }

        foreach (var constraint in affectedConstraints)
        {
            scene.RemoveConstraint(constraint);
        }

        removedConstraintCount = affectedConstraints.Length;
        return true;
    }
}