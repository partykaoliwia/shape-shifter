using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
    /// <summary>
    /// Applies a vertex's continuity setting if adjacent arcs and the solver accept it.
    /// Restores the previous setting if the requested relation cannot be applied.
    /// </summary>
    /// <returns>True if the setting was accepted; otherwise, false.</returns>
    public bool TrySetContinuity(
        Vertex vertex,
        ContinuityType continuity)
    {
        if (!Enum.IsDefined(continuity))
        {
            return false;
        }

        var incidentEdges = scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .Where(edge => edge.Start == vertex || edge.End == vertex)
            .ToArray();

        if (incidentEdges.Length == 0)
        {
            return false;
        }

        var previous = vertex.Continuity;
        vertex.SetContinuity(continuity);

        bool invalidArc = incidentEdges.Any(edge =>
            edge.Arc != null
            && !CircularArcSegment.SupportsContinuity(
                edge.Start, edge.End));

        if (invalidArc)
        {
            vertex.SetContinuity(previous);
            return false;
        }

        if (TryApplyCurrentGeometry(scene.Constraints))
        {
            return true;
        }

        vertex.SetContinuity(previous);
        return false;
    }

    /// <summary>
    /// Combines explicit scene relations with continuity and arc validity conditions.
    /// </summary>
    private IGeometricConstraint[] GetSolverConstraints(
        IEnumerable<IGeometricConstraint> constraints)
    {
        return constraints
            .Concat(GetContinuityConstraints())
            .Concat(GetArcGeometryConstraints())
            .ToArray();
    }

    /// <summary>
    /// Rebuilds tangent relations from the current edge loop and vertex settings.
    /// G0 is already satisfied by shared vertices, and joins between two lines are skipped.
    /// </summary>
    private IEnumerable<ContinuityConstraint> GetContinuityConstraints()
    {
        foreach (var polygon in scene.Polygons)
        {
            foreach (var outgoing in polygon.Edges)
            {
                var vertex = outgoing.Start;

                if (vertex.Continuity == ContinuityType.G0)
                {
                    continue;
                }

                var incoming = polygon.Edges.First(
                    edge => edge.End == vertex);

                if (incoming.IsLine && outgoing.IsLine)
                {
                    continue;
                }

                yield return new ContinuityConstraint(
                    vertex, incoming, outgoing);
            }
        }
    }

    /// <summary>
    /// Adds a validity check for every arc so solving cannot leave an undefined circle.
    /// </summary>
    private IEnumerable<ArcGeometryConstraint> GetArcGeometryConstraints()
    {
        foreach (var edge in scene.Polygons.SelectMany(p => p.Edges))
        {
            if (edge.Arc != null)
            {
                yield return new ArcGeometryConstraint(edge);
            }
        }
    }

    /// <summary>
    /// Tries to satisfy the current relations while keeping every vertex fixed.
    /// Only handle offsets and arc tangents may change; a failed snapshot is discarded.
    /// </summary>
    private bool TryApplyCurrentGeometry(
        IEnumerable<IGeometricConstraint> constraints)
    {
        var positions = CapturePositions();

        var fixedVertices = positions.Keys
            .OfType<Vertex>()
            .Cast<IGeometryVariable>()
            .ToHashSet();

        if (!solver.TrySolve(
            GetSolverConstraints(constraints),
            positions,
            fixedVertices))
        {
            return false;
        }

        ApplyPositions(positions);
        return true;
    }

    /// <summary>
    /// Validates a changed edge loop using only relations whose edges still exist.
    /// Removes stale relations on success, or restores the previous edge loop on failure.
    /// </summary>
    private bool TryFinishTopologyChange(
        Polygon polygon,
        IReadOnlyList<Edge> previousEdges)
    {
        var validConstraints = scene.Constraints
            .Where(constraint =>
                constraint.AffectedEdges.All(scene.ContainsEdge))
            .ToArray();

        if (!TryApplyCurrentGeometry(validConstraints))
        {
            polygon.RestoreEdges(previousEdges);
            return false;
        }

        scene.RemoveInvalidConstraints();
        return true;
    }
}
