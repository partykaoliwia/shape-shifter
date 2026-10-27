using ShapeShifter.Models;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
    /// <summary>
    /// Changes an edge's representation and solves the remaining relations.
    /// Removes affected explicit constraints on success, or restores the original representation on failure.
    /// </summary>
    /// <returns>True if the type was already selected or the change was accepted.</returns>
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
            && !CircularArcSegment.SupportsContinuity(edge.Start, edge.End))
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
